module Shared

/// Defines how routes are generated on server and mapped from client
let routerPaths typeName method = sprintf "/api/%s" method

type Shortcut = {
    Id: int
    Title: string
    Description: Option<string>
    Keys: List<string>
  } with
    static member Default = {
      Id = 0
      Title = "New Shortcut"
      Description = None
      Keys = []
    }

type Section = {
    Id: int
    Title: string
    Shortcuts: List<Shortcut>
  } with
    static member Default = {
      Id = 0
      Title = "Click me to edit"
      Shortcuts = []
    }
    // Could eventually become a guid instead of an int.
    member this.GetNextShortcutId () = 
      if this.Shortcuts.Length = 0 then 1 
      else 
        let maxId = this.Shortcuts |> List.maxBy (fun f -> f.Id)
        maxId.Id + 1

type Poster = {
    Id: System.Guid // Should probably be a guid... for the other sub items, we should be kind of fine.
    Title: string
    Sections: List<Section>
    Background: Option<string>
    Color: Option<string>
    Font: Option<string>
  }
  with
    static member Default = {
      Id = System.Guid.NewGuid()
      Title = ""
      Sections = []
      Background = None
      Color = None
      Font = None
    }
    static member FakeData = {
      Id = System.Guid.NewGuid()
      Title = "My poster title ;)"
      Sections = [
        { Id = 1; Title="My first section"; Shortcuts = [{Id = 1; Title="Open"; Description = None; Keys=["Ctrl"; "Alt"; "Shift"; "O"]}] }
        { Id = 2; Title="My second section"; Shortcuts = [{Id = 2; Title="Archive"; Description = None; Keys=["Y"]}; { Id = 3; Title="Delete"; Description = None; Keys=["D"] }] }
        { Id = 3; Title="Empty third section"; Shortcuts = [] }
        ]
      Background = None
      Color = None
      Font = None
    }

    // Could eventually become a guid instead of an int.
    member this.GetNextSectionId () = 
      if this.Sections.Length = 0 then 1 
      else 
        let maxId = this.Sections |> List.maxBy (fun f -> f.Id)
        maxId.Id + 1

type Counter = { value : int }

/// A type that specifies the communication protocol between client and server
/// to learn more, read the docs at https://zaid-ajaj.github.io/Fable.Remoting/src/basics.html
type IServerApi = {
    Counter : unit -> Async<Counter>
}
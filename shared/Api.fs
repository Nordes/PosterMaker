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
    
type Poster = {
    Id: int
    Title: string
    Sections: List<Section>
    Background: Option<string>
    Color: Option<string>
    Font: Option<string>
  }
  with
    static member Default = {
      Id = 0
      Title = ""
      Sections = []
      Background = None
      Color = None
      Font = None
    }
    static member FakeData = {
      Id = 1
      Title = "My poster title ;)"
      Sections = [
        {Id = 1; Title="My first section"; Shortcuts = [{Id = 1; Title="Open"; Description = None; Keys=[]}]}
        {Id = 2; Title="My second section"; Shortcuts = [{Id = 2; Title="Archive"; Description = None; Keys=[]}; {Id = 3; Title="Delete"; Description = None; Keys=[]}]}
        {Id = 3; Title="Empty third section"; Shortcuts = []}
        ]
      Background = None
      Color = None
      Font = None
    }


type Counter = { value : int }

/// A type that specifies the communication protocol between client and server
/// to learn more, read the docs at https://zaid-ajaj.github.io/Fable.Remoting/src/basics.html
type IServerApi = {
    Counter : unit -> Async<Counter>
}
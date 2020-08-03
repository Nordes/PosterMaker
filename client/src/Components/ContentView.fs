module App.Components

open Feliz
open Shared
open Fable.React.Helpers
open Fulma

// https://zaid-ajaj.github.io/Feliz/#/Feliz/React/StatelessComponents
type Msg =
  | ShortcutEditVisible of bool

type EditableSpan = {
  Value: string
  HandleSave: string -> Unit
}

let editableSpan = React.functionComponent( fun (props: EditableSpan) ->
  let (edit, setEdit) = React.useState(false)
  let (name, setName) = React.useState(props.Value)
  let (nameOrig, setNameOrig) = React.useState(props.Value)

  let handleEdit = (fun e -> 
    setEdit true
    )
  let handleSaveChange = (fun e -> 
    props.HandleSave name
    setNameOrig name // or get repopulated by React?.... seriously don't know
    // etc.
    setEdit false
    )
  let handleCancelChange = (fun () -> 
    setName nameOrig
    setEdit false
    )

  if not edit then
    Html.span [
      prop.text name
      prop.className ["pointer"]
      prop.onClick handleEdit // Eventually, maybe double click + preventDefault ... to avoid selecting
    ]
  else 
    Html.input [
      prop.autoFocus true
      prop.onFocus (fun e -> 
                        let target = e.target :?> Browser.Types.HTMLInputElement
                        target.select() )
      prop.placeholder "Section"
      prop.className ["input"; "is-small"]
      prop.type' "text"
      prop.value name
      prop.onTextChange setName
      prop.onBlur handleSaveChange
      prop.onKeyUp <| fun (key) -> match key.keyCode with 
                                    | 13. -> handleSaveChange()
                                    | 27. -> handleCancelChange()
                                    | _ -> ()
    ]
  )

type RenderShortcut = {
  Data: Shortcut
  HandleSave: string -> Unit
  HandleEditMapping: Unit -> Unit
}

let renderShortcut = React.functionComponent( fun (props: RenderShortcut)  ->
  let (edit, setEdit) = React.useState(false)
  let cancelEdit () = 
    setEdit(false)

  // Using Level
  Level.level [ ] [
    Level.left [ ] [ 
      Level.item [ ] [ 
        editableSpan ({Value = props.Data.Title; HandleSave = props.HandleSave })
      ]
    ]
    Level.right [ ] [
      Level.item [ ] [
        let getTagColor key = 
          match key with
          | "Ctrl" | "Alt" | "Fn" | "Shift" | "Esc" | "Tab" | "CapsLock" | "Backspace" | "Space" | "Enter" | "Meta" -> IsDark
          | "F1" | "F2" | "F3" | "F4" | "F5" | "F6" | "F7" | "F8" | "F9" | "F10" | "F11" | "F12" -> IsLink
          | _ -> IsLink
        Html.div [
          prop.onClick (fun _ -> props.HandleEditMapping())
          prop.className ["pointer"]
          prop.children [
            // Small hack: reverse... tail ... reverse
            if props.Data.Keys.Length > 0 then
              let stuff  = props.Data.Keys |> List.rev |> List.tail |> List.rev
              let stuff2 = props.Data.Keys |> List.rev |> List.head
              
              for part in stuff do
                Tag.tag [ Tag.Color <| getTagColor part ] [ str part ] 
                // If last, avoid +
                Html.span [ prop.text "+"; prop.className [StyleLiterals.TagPlus]]
              
              Tag.tag [ Tag.Color <| getTagColor stuff2] [ str stuff2 ]
            else
              Tag.tag [ Tag.Color IsWarning] [ str "...set shortcut..." ]
          ]
        ]
      ]
    ]
  ]
)

type ContentRawView = {
  Poster: Poster
  ToggleModal: Option<Components.ModalShortcut.DialogProps> -> Unit
  // CloseShortcut: Unit
}

let contentRawView = React.functionComponent("posterRawContent", fun (props:ContentRawView) ->
  let (isLoading, setLoading) = React.useState(false)
  let (poster, setPoster) = React.useState(props.Poster)//Poster.Default)
  
  // let loadData() = async {
  //     setLoading true
  //     do! Async.Sleep 500 // fake download... heh
  //     setLoading false
  //     printf "Content loaded \o/"
  //     setPoster Poster.FakeData
  // }

  // React.useEffect(loadData >> Async.StartImmediate, [| |])
  
  let handleEditSection posterId sectionId e = 
    printf  "handleEditSection ||> Poster Id: %A; Section Id: %i; data %s" posterId sectionId e
  let handleEditScText posterId sectionId shortcutId newTitle = 
    // If empty, we should delete it
    let sec = poster.Sections |> List.map (fun s -> 
                                            if s.Id <> sectionId then s
                                            else 
                                              { s with Shortcuts = s.Shortcuts |> List.map (fun sc -> 
                                                                                    if sc.Id <> shortcutId then sc 
                                                                                    else { sc with Title = newTitle } ) }
                                           )
    setPoster ({ poster with Sections = sec })
    printf "handleEditShortcut [Text] ||> Poster Id: %A; Section Id: %i; Shortcut Id: %i; data %s" posterId sectionId shortcutId newTitle

  let handleEditScKeys posterId sectionId shortcutId keys = 
    // Sc keys
    let sec = poster.Sections |> List.map (fun s -> 
                                            if s.Id <> sectionId then s
                                            else 
                                              { s with Shortcuts = s.Shortcuts |> List.map (fun sc -> 
                                                                                    if sc.Id <> shortcutId then sc 
                                                                                    else { sc with Keys = keys } ) }
                                           )
    setPoster ({ poster with Sections = sec })

    printf "handleEditShortcut [Text] ||> Poster Id: %A; Section Id: %i; Shortcut Id: %i; data %A" posterId sectionId shortcutId keys


  let handleAddSection posterId = 
    printf  "handleAddSection ||> Poster Id: %A; " posterId
    // Find max section ID + 1... kind of... normally should be a service call
    // if not a "live" save, then we could handle the tree differently.
    setPoster ({poster with Sections = poster.Sections @ [{Shared.Section.Default with Id = poster.GetNextSectionId() }] })

  let handleAddShortcut posterId sectionId =
    let sec = poster.Sections |> List.map (fun s -> 
                                            if s.Id <> sectionId then s
                                            else { s with Shortcuts = s.Shortcuts @ [{Shortcut.Default with Id = s.GetNextShortcutId()}] }
                                           )
    setPoster ({ poster with Sections = sec })

  Html.div [
    if isLoading
    then Html.h1 "Loading"
    else 
      Html.h1 [
        prop.text poster.Title
        prop.className ["title"; "center"; "has-text-centered"]
      ]

      // _generateAsMenu ()
      Columns.columns [ Columns.IsMultiline; Columns.IsMobile ] [
        for section in poster.Sections do
          Column.column [ Column.Width(Screen.All, Column.IsOneThird); ] [
            Card.card [ ] [ 
              Card.header [ ] [ 
                Card.Header.title [ ] [ 
                  editableSpan ({Value = section.Title; HandleSave = (fun e -> handleEditSection poster.Id section.Id e )})
                ]
              ]
              Card.content [ ] [
                Html.ul [
                  for shortcut in section.Shortcuts do
                    Html.li [
                      renderShortcut {
                        Data = shortcut
                        HandleSave = fun (e) -> handleEditScText poster.Id section.Id shortcut.Id e 
                        HandleEditMapping = fun _ -> props.ToggleModal ( 
                                                        Some { 
                                                          IsActive = true // Not really needed anymore
                                                          Data = shortcut.Keys
                                                          Close = (fun _ -> props.ToggleModal None)
                                                          Save = (fun keys -> handleEditScKeys poster.Id section.Id shortcut.Id keys)
                                                        })
                      }
                    ]
                    
                  Html.li [ 
                    Button.button [ 
                      Button.OnClick <| fun e -> handleAddShortcut poster.Id section.Id // printf "Poster: %i; Section: %i; New shortcut" poster.Id section.Id;
                      Button.IsLight; Button.Color IsPrimary; Button.Size IsSmall ] [ str "+ Add new shortcut +" ] ]
                ]
              ]
            ]
          ]

        Column.column [ Column.Width(Screen.All, Column.IsOneQuarter); ] [
          Button.button [ 
            Button.OnClick <| fun e -> handleAddSection poster.Id; 
            Button.Size IsLarge; Button.Color IsLight; Button.IsLink ] [ str "+ Add new section +" ]
        ]
      ]
  ]
)

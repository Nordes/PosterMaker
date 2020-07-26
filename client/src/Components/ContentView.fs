module App.Components

open Feliz
open Shared
open Fable.React.Helpers
open Fulma

// https://zaid-ajaj.github.io/Feliz/#/Feliz/React/StatelessComponents

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
      prop.onClick handleEdit
    ]
  else 
    Html.input [
      prop.autoFocus true
      prop.placeholder "Section"
      prop.value name
      prop.onTextChange setName
      prop.onBlur handleSaveChange
      prop.onKeyUp <| fun (key) -> match key.keyCode with 
                                    | 13. -> handleSaveChange()
                                    | 27. -> handleCancelChange()
                                    | _ -> ()
    ]
  )

let contentRawView = React.functionComponent("posterRawContent", fun () ->
  let (isLoading, setLoading) = React.useState(false)
  let (poster, setPoster) = React.useState(Poster.Default)
  
  let loadData() = async {
      setLoading true
      do! Async.Sleep 500 // fake download... heh
      setLoading false
      printf "Content loaded \o/"
      setPoster Poster.FakeData
  }

  React.useEffect(loadData >> Async.StartImmediate, [| |])
  
  let handleEditSection posterId sectionId e = 
    printf  "Poster Id: %A; Section Id: %i; data %s" posterId sectionId e
  let handleEditShortcut posterId sectionId shortcutId e = 
    printf "Poster Id: %A; Section Id: %i; Shortcut Id: %i; data %s" posterId sectionId shortcutId e

  let handleAddSection posterId = 
    printf  "Poster Id: %A; " posterId
    // Find max section ID + 1... kind of... normally should be a service call
    // if not a "live" save, then we could handle the tree differently.
    setPoster ({poster with Sections = poster.Sections @ [{Shared.Section.Default with Id = poster.GetNextSectionId() }] })

  let handleAddShortcut posterId sectionId =
    let sec = poster.Sections |> List.map (fun s -> 
                                            if s.Id <> sectionId then s
                                            else { s with Shortcuts = s.Shortcuts @ [{Shared.Shortcut.Default with Id = s.GetNextShortcutId()}] }
                                           )
    // TODO : Send update to server
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
          Column.column [ Column.Width(Screen.All, Column.IsOneQuarter); ] [
            Card.card [ ] [ 
              Card.header [ ] [ 
                Card.Header.title [ ] [ 
                  editableSpan ({Value = section.Title; HandleSave = (fun e -> handleEditSection poster.Id section.Id e )})
                  Html.span (sprintf " - %i" section.Id)
                ]
              ]
              Card.content [ ] [
                Html.ul [
                  for shortcut in section.Shortcuts do
                    Html.li [
                      editableSpan ({Value = shortcut.Title; HandleSave = fun (e) -> handleEditShortcut poster.Id section.Id shortcut.Id e })
                      Html.span (sprintf " - %i" shortcut.Id)
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

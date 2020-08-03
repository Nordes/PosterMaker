module App.Components.ModalShortcut
open Feliz
open System
open Fulma
open Fable.React.Helpers
open Fable.React.Props

type DialogProps = {
  IsActive: bool
  Data:     List<string>
  Close:    Unit -> Unit
  Save:     List<string> -> Unit
  }

let recorder = React.functionComponent("modalShortcutRecorder", fun (props: DialogProps) ->
  let modalKey (ev:Browser.Types.Event) =
    let kev = ev :?> Browser.Types.KeyboardEvent
    if kev.keyCode = 27. then // Escape to close
      props.Close()

  let windowsKeyDownEffect () =
    Browser.Dom.window.addEventListener("keydown", modalKey)
    { new IDisposable with member this.Dispose() =
                            Browser.Dom.window.removeEventListener("keydown", modalKey)
                           }

  React.useEffect(windowsKeyDownEffect)

  let (keys, setKeys) = React.useState([])
  let (keysBis, setKeysBis) = React.useState([])

  let addKey (k) =
    let key = match k with 
               | "Control" -> "Ctrl" 
               | "Escape" -> "Esc"
               | " " -> "Space"
               | a when a >= "a" && a <= "z" -> a.ToUpper()
               | _ -> k

    let isKeyPresent = keys |> List.contains key
    if not isKeyPresent then
      setKeysBis ( keysBis @ [key] )
      if (keysBis.Length = 0) then 
        setKeys ( [key] )
      else
        setKeys ( keys @ [key] )

  let removeKey (k) =
    let key = match k with 
               | "Control" -> "Ctrl" 
               | "Escape" -> "Esc"
               | " " -> "Space"
               | a when a >= "a" && a <= "z" -> a.ToUpper()
               | _ -> k

    let newKeys = keysBis |> List.filter (fun f -> f <> key)
    setKeysBis newKeys

  Modal.modal [ Modal.IsActive props.IsActive ] [
    Modal.background [ Props [ OnClick (fun _ -> props.Close()) ] ] []
    Modal.Card.card [] [
      Modal.Card.head [ ] [ 
        Modal.Card.title [  ] [ str "Set shortcut key-binding" ]
        // Close button
        // Html.a [ prop.className ["icon"; "fa"; "fa-trash" ] ]
        Delete.delete [ Delete.OnClick <| fun _ -> props.Close() ] [ ] ]
      Modal.Card.body [  ] [
        let drawCurrent () =
          let getTagColor key = 
            match key with
            | "Ctrl" | "Alt" | "Fn" | "Shift" -> IsDark
            | _ -> IsInfo
          Html.div [ 
            if props.Data.Length > 0 then
              let stuff  = props.Data |> List.rev |> List.tail |> List.rev
              let stuff2 = props.Data |> List.rev |> List.head
              
              for part in stuff do
                Tag.tag [ Tag.Color <| getTagColor part ] [ str part ] 
                Html.span [ prop.text "+"; prop.className [StyleLiterals.TagPlus]]
              
              Tag.tag [ Tag.Color <| getTagColor stuff2 ] [ str stuff2 ]
            else
              Tag.tag [ Tag.Color IsWarning] [ str "...set shortcut..." ]
          ]

        let d = keys |> List.fold (fun acc k -> sprintf "%A[%A]+" acc k) ""
        Html.form [
          Field.div [] [
            Label.label [] [str "Current" ]
            Control.div [] [
              drawCurrent()
            ]
          ]
          Field.div [] [
            Label.label [] [ str "New" ]
            Control.div [] [ 
              // TODO: Possibly, we should also be able to input manually... because... keys can't be accessed otherwise.
              Html.input [
                prop.type' "text"
                prop.className ["input"]
                prop.value (d.Substring(0, (d.Length-1)))
                prop.autoFocus true
                prop.readOnly true
                prop.placeholder "Push the keys"
                prop.onKeyDown (fun e -> e.preventDefault(); addKey e.key)
                prop.onKeyUp (fun e-> e.preventDefault(); removeKey e.key)
                prop.onBlur (fun e -> setKeysBis [])
              ]
            ]
          ]
        ]

        Html.br []
        Html.br []

        Html.div [ Button.button [Button.Color IsSuccess; Button.OnClick (fun _ -> props.Save(keys); props.Close())] [str "Save"] 
                   Html.span " "
                   Button.button [Button.Color IsWarning; Button.OnClick (fun _ -> props.Close())] [str "Cancel"]]
      ]
    ]
  ]
)
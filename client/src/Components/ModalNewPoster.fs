module App.Components.ModalNewPoster
open Feliz
open System
open Fulma
open Fable.React.Helpers
open Fable.React.Props

type DialogProps = {
  IsActive: bool
  Close:    Unit -> Unit
  Save:     string -> Unit
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

  let (title, setTitle) = React.useState("")
  let save () = 
    props.Save(title)
    props.Close()

  Modal.modal [ Modal.IsActive props.IsActive ] [
    Modal.background [ Props [ OnClick (fun _ -> props.Close()) ] ] []
    Modal.Card.card [] [
      Modal.Card.head [ ] [ 
        Modal.Card.title [  ] [ str "🚀 New Poster 🚀" ]
        // Close button
        // Html.a [ prop.className ["icon"; "fa"; "fa-trash" ] ]
        Delete.delete [ Delete.OnClick <| fun _ -> props.Close() ] [ ] ]
      Modal.Card.body [  ] [
        Html.form [
          prop.onSubmit (fun e -> e.preventDefault(); save())
          prop.children [
            Field.div [] [
              Label.label [] [ str "Title" ]
              Control.div [] [ 
                // TODO: Possibly, we should also be able to input manually... because... keys can't be accessed otherwise.
                Html.input [
                  prop.type' "text"
                  prop.className ["input"]
                  prop.defaultValue title
                  prop.onChange setTitle
                  prop.autoFocus true
                  prop.placeholder "E.g.: My new poster"
                ]
              ]
            ]
          ]
        ]

        Html.br []
        Html.br []

        Html.div [ Button.button [Button.Color IsSuccess; Button.OnClick (fun _ -> save())] [str "Save"] 
                   Html.span " "
                   Button.button [Button.Color IsWarning; Button.OnClick (fun _ -> props.Close())] [str "Cancel"]]
      ]
    ]
  ]
)
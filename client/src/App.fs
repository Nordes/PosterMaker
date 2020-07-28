module App

open Feliz
open Elmish
open Shared
open Fulma
open Fable.React.Helpers


type State = { 
  Counter: Deferred<Result<Counter, string>> 
  Poster: Poster
  ShortcutDialog: option<App.Components.ModalShortcut.DialogProps>
  }

type Msg =
  | LoadCounter of AsyncOperationStatus<Result<Counter, string>>
  | EditShortcutDialog of option<App.Components.ModalShortcut.DialogProps>

let init() = { Counter = HasNotStartedYet; Poster = Poster.FakeData; ShortcutDialog = None }, Cmd.ofMsg (LoadCounter Started)

let update (msg: Msg) (state: State) =
    match msg with
    | LoadCounter Started ->
        let loadCounter = async {
            try
                let! counter = Server.api.Counter()
                return LoadCounter (Finished (Ok counter))
            with error ->
                Log.developmentError error
                return LoadCounter (Finished (Error "Error while retrieving Counter from server"))
        }

        { state with Counter = InProgress }, Cmd.fromAsync loadCounter

    | LoadCounter (Finished counter) ->
      { state with Counter = Resolved counter }, Cmd.none

    | EditShortcutDialog myDialogProps ->
      let newState = { state with ShortcutDialog = myDialogProps }
      printfn "Edit mapping visibility should be: %A" <| match newState.ShortcutDialog with | Some -> true | None -> false
      newState, Cmd.none

    // | Increment ->
    //     let updatedCounter =
    //         state.Counter
    //         |> Deferred.map (function
    //             | Ok counter -> Ok { counter with value = counter.value + 1 }
    //             | Error error -> Error error)

    //     { state with Counter = updatedCounter }, Cmd.none

    // | Decrement ->
    //     let updatedCounter =
    //         state.Counter
    //         |> Deferred.map (function
    //             | Ok counter -> Ok { counter with value = counter.value - 1 }
    //             | Error error -> Error error)
    //
    //     { state with Counter = updatedCounter }, Cmd.none

let renderCounter (counter: Deferred<Result<Counter, string>>)=
    match counter with
    | HasNotStartedYet -> Html.none
    | InProgress -> Html.h1 "Loading..."
    | Resolved (Ok counter) -> Html.h1 counter.value
    | Resolved (Error errorMsg) ->
        Html.h1 [
            prop.style [ style.color.crimson ]
            prop.text errorMsg
        ]

// let fableLogo() = StaticFile.import "./imgs/fable_logo.png"

// Create a React component
// Display list of shortcut
let showContent (dispatch: Msg -> unit) = 
    Html.div [
        prop.children [
            Html.p "Stuff's going here"
        ]
    ]

let render (state: State) (dispatch: Msg -> unit) =
  Html.div [
    Navbar.navbar [ Navbar.Color IsPrimary] [
      Navbar.Brand.div [ ] [ 
        Navbar.Item.a [ ]
          [ Html.img [
                  prop.src "img/favicon-32x32.png" ] ] ] 

          // [ Html.img [
          //         prop.src (StaticFile.import "./imgs/fable_logo.png") ] ] ] 
      Navbar.Item.div [] [ 
        Html.h2 [
          prop.className ["has-text-white"]
          prop.text "Poster Maker" ] 
        ] 
      ]

    Section.section [ ]
      [ Container.container [ Container.IsFluid ]
          [ 
            App.Components.contentRawView ({ ToggleModal = fun (content) -> dispatch (EditShortcutDialog content) })
          ] ]

    Footer.footer [ ]
      [ Content.content [ Content.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
          [ 
            Html.h1  "Poster Maker"
            Html.p  "A way to create your poster for shortcuts... somehow!"  ] ]

    
    match state.ShortcutDialog with
    | Some e -> App.Components.ModalShortcut.recorder e
    | None   -> Html.none
   ]

module App

open Feliz
open Elmish
open Shared
open Fulma
open Fable.React.Helpers
open Fable.React.Props

type State = { 
  Counter: Deferred<Result<Counter, string>> 
  Poster: Poster
  ZePoster: Deferred<Result<Poster, string>> 
  ShortcutDialog: option<App.Components.ModalShortcut.DialogProps>
  NewPosterDialog: option<App.Components.ModalNewPoster.DialogProps>
  }

type Msg =
  | LoadCounter of AsyncOperationStatus<Result<Counter, string>>
  | LoadPoster of AsyncOperationStatus<Result<Poster, string>>
  | EditShortcutDialog of option<App.Components.ModalShortcut.DialogProps>
  | NewPosterDialog of option<App.Components.ModalNewPoster.DialogProps>
  | SaveNewPoster of string

let init() = { 
                Counter = HasNotStartedYet; 
                Poster = Poster.FakeData; 
                ZePoster = HasNotStartedYet; 
                ShortcutDialog = None 
                NewPosterDialog = None
              }, Cmd.ofMsg (LoadPoster Started)

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

    | LoadPoster Started ->
        let loadPoster = async {
            try
                let! poster = Server.posterApi.Get(System.Guid.Parse("a27b291a-0df6-45c3-9954-eb64ca91727d"))
                return LoadPoster (Finished (Ok poster))
            with error ->
                Log.developmentError error
                return LoadPoster (Finished (Error "Error while retrieving Poster from server"))
        }

        { state with ZePoster = InProgress }, Cmd.fromAsync loadPoster

    | LoadPoster (Finished poster) ->
      { state with ZePoster = Resolved poster }, Cmd.none

    | EditShortcutDialog myDialogProps ->
      let newState = { state with ShortcutDialog = myDialogProps }
      printfn "Edit mapping visibility should be: %A" <| match newState.ShortcutDialog with | Some -> true | None -> false
      newState, Cmd.none
    
    | NewPosterDialog myDialogProps -> 
      let newState = { state with NewPosterDialog = myDialogProps }
      printfn "Edit mapping visibility should be: %A" <| match newState.NewPosterDialog with | Some -> true | None -> false
      newState, Cmd.none

    | SaveNewPoster title -> 
      let loadPoster = async {
          try
              let myNewPoster = { Title = title }
              let! poster = Server.posterApi.Create(myNewPoster)
              return LoadPoster (Finished (Ok poster))
          with error ->
              Log.developmentError error
              return LoadPoster (Finished (Error "Error while saving Poster on server"))
      }

      { state with ZePoster = InProgress }, Cmd.fromAsync loadPoster

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

let renderPoster (poster: Deferred<Result<Poster, string>>, dispatch)=
    match poster with
    | HasNotStartedYet -> Html.none
    | InProgress -> Html.h1 "Loading..."
    | Resolved (Ok poster) -> 
        App.Components.contentRawView ({ 
            Poster = poster
            ToggleModal = fun (content) -> dispatch (EditShortcutDialog content) })
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

      Navbar.Item.div [Navbar.Item.HasDropdown;Navbar.Item.IsHoverable ] [
          Navbar.Link.a [ ]
            [ str "File" ]
          Navbar.Dropdown.div [ ]
            [ Navbar.Item.a [
                    Navbar.Item.Props [OnClick (fun _->
                      dispatch (NewPosterDialog (Some {
                        IsActive = true
                        Close = (fun _ -> dispatch (NewPosterDialog None) )
                        Save = (fun title -> dispatch (SaveNewPoster title) )
                       }))
                      )]
                    ]  //Navbar.Item.Props [ OnClick (fun _ -> printf "Hello") ] ]
                [ str "New" ]
              Navbar.Item.a [ ]
                [ str "Load" ]
              Navbar.divider [ ] [ ]
              Navbar.Item.a [ ]
                [ str "Export...maybe" ] ]         
      ]
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
            renderPoster (state.ZePoster, dispatch) // could load the poster... instead like this. but the component i have is not really "redux"
          ] ]

    Footer.footer [ ]
      [ Content.content [ Content.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
          [ 
            Html.h1  "Poster Maker"
            Html.p  "A way to create your poster for shortcuts... somehow!"  ] ]

    match state.ShortcutDialog with
    | Some e -> App.Components.ModalShortcut.recorder e
    | None   -> Html.none

    match state.NewPosterDialog with
    | Some e -> App.Components.ModalNewPoster.recorder e
    | None   -> Html.none

   ]

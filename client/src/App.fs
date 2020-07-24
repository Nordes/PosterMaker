module App

open Feliz
open Elmish
open Shared
open Fulma

type Shortcut = {
        Title: string
        Keys: seq<string>
    }

type Section = {
        Name: string
    }

type State = { 
    Counter: Deferred<Result<Counter, string>> 
    Sections: List<Section>
    }

type Msg =
    | CreateSection of string
    | CreateShortut of seq<string>
    | LoadCounter of AsyncOperationStatus<Result<Counter, string>>

let init() = { Counter = HasNotStartedYet; Sections = [] }, Cmd.ofMsg (LoadCounter Started)

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

let nav () =
    Html.nav [
      prop.className ["navbar"; "is-fulma"]
      prop.children [
        Html.div [
          prop.className "container"
          prop.children [
            Html.div [
              prop.className "navbar-start"
              prop.children [
                Html.div [
                  prop.className "navbar-start"
                  prop.children [
                    Html.div [
                      prop.className "navbar-brand"
                      prop.children [
                        Html.h1 [
                          prop.className ["title"; "is-1"]
                          prop.children [
                            Html.img [
                              prop.alt "logo"
                              prop.className ""
                            ]
                          ]
                        ]
                      ]
                    ]
                  ]
                ]
              ]
            ]
          ]
        ]
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

    Html.section [
      prop.className ["section"]
      prop.children [
          // drawCalendar model dispatch
      ] ]

    Footer.footer [ ]
      [ Content.content [ Content.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
          [ 
            Html.h1  "Poster Maker"
            Html.p  "A way to create your poster for shortcuts... somehow!" ] ]

   ]
   
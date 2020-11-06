module AzureAd.Bolero.Web.Main

open System
open Elmish
open Bolero
open Bolero.Html
open Bolero.Remoting.Client
open Bolero.Templating.Client

type Page =
    | [<EndPoint "/">] Home

type Model =
    { Page: Page
      LoginState : Msal.Model }

type Message =
    | SetPage of Page
    | MsalMsg of Msal.Msg

let init _ =
    let initialMsalState, initialMsalCmd = Msal.init()
    let initialCmd = Cmd.map MsalMsg initialMsalCmd
    { Page = Home
      LoginState = initialMsalState
    }, initialCmd

let update js msg model =
    Console.WriteLine (msg.ToString())
    match msg with
    | SetPage page ->
        { model with Page = page }, Cmd.none
    | MsalMsg msg' ->
        let msalState, msalCmd = Msal.update js msg' model.LoginState
        { model with LoginState = msalState}, Cmd.map MsalMsg msalCmd

let router = Router.infer SetPage (fun model -> model.Page)

type Main = Template<"wwwroot/main.html">

let menuItem (model: Model) (page: Page) (text: string) =
    Main.MenuItem()
        .Active(if model.Page = page then "is-active" else "")
        .Url(router.Link page)
        .Text(text)
        .Elt()

let view model _ =
    Main()
        .Menu(concat [
            menuItem model Home "Home"
        ])
        .Body(
            cond model.Page <| function
            | Home -> Main.Home().Elt()
        )
        .Elt()

type MyApp() =
    inherit ProgramComponent<Model, Message>()

    override this.Program =
        let update = update this.JSRuntime
        Program.mkProgram init update view
        |> Program.withRouter router
#if DEBUG
        |> Program.withHotReload
#endif

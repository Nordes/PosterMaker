module Program

open Saturn
open Giraffe
open Shared
open Server
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Microsoft.Extensions.DependencyInjection
open Giraffe.Core
open Giraffe.ResponseWriters


let webApi: HttpHandler =
    Remoting.createApi()
    // dependency injection... here
    |> Remoting.fromContext (fun ctx -> ctx.GetService<ServerApi>().Build())
    |> Remoting.withRouteBuilder routerPaths
    |> Remoting.buildHttpHandler

let publicPath = System.IO.Path.GetFullPath "../client/public"

let api = pipeline {
    plug acceptJson
    set_header "x-pipeline-type" "Api"
}

let stuff = router {
    get "/what" (fun next ctx -> FSharp.Control.Tasks.ContextSensitive.task {
        return! text "Hello world" next ctx 
    })
}

let webApp =
    choose [ 
      webApi // With DI
      Remoting.Poster.Api
      GET >=> text "Not found" 
    ]

let serviceConfig (services: IServiceCollection) =
    services
      .AddSingleton<ServerApi>()
      .AddSingleton<Remoting.Poster.Implementation>()
      .AddLogging()

let application = application {
    use_router webApp
    // use_static "wwwroot"
    use_static publicPath
    use_gzip
    // use_iis
    service_config serviceConfig
    host_config Env.configureHost
}

run application

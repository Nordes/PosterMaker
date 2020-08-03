module Remoting.Poster
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Configuration
open Shared
open Saturn
open Giraffe
open Fable.Remoting.Server
open Fable.Remoting.Giraffe

// Use some database stuff https://api.elephantsql.com/ ... (github login)
type Implementation(logger: ILogger<Implementation>, config: IConfiguration) =
  member this.Get(id:System.Guid) = 
    async {
        logger.LogInformation("Executing {Function}", "poster")
        // do! Async.Sleep 1000
        return Poster.FakeData
    }

  member this.GetAll() =
    async {
      logger.LogInformation("Executing {Function}", "poster")
      return [
        {Title=Poster.FakeData.Title; Id= Poster.FakeData.Id}
        {Title="Really fake inexisting"; Id= System.Guid.NewGuid()}
        ]
    }
    
  member this.Save(poster:Poster) = 
    async {
      return poster
    }

  member this.Create(newPosterReq: NewPosterReq): Async<Poster> = 
    async {
      return { Poster.Default with Title = newPosterReq.Title }
    }

  member this.Build() : IPosterApi =
    {
      GetAll = this.GetAll
      Get = this.Get
      Save = this.Save
      Create = this.Create
    }

let Api: HttpHandler = 
  Remoting.createApi()
  |> Remoting.fromContext (fun ctx -> ctx.GetService<Implementation>().Build())
  // |> Remoting.withDiagnosticsLogger (printfn "%s")
  |> Remoting.withRouteBuilder Shared.routerPaths2
  |> Remoting.buildHttpHandler

namespace <%= namespace %>

open Owin
open Microsoft.Owin
open System
open System.Web
open Microsoft.Owin.Hosting
open WebSharper.Owin

[<Sealed>]
type Startup() =
    member __.Configuration(builder: IAppBuilder) =
        let path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase
        builder.UseSitelet(path, Site.Main)
        |> ignore

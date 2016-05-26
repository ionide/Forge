module Services

open Suave
open Suave.Web
open System
open System.Fabric
open System.Fabric.Description
open System.Threading
open System.Threading.Tasks
open Microsoft.ServiceFabric.Services.Runtime
open Microsoft.ServiceFabric.Services.Communication.Runtime
open HttpClient

type <%= namespace %>Service() =
    inherit StatelessService()


    let buildConfig (parameters : StatelessServiceInitializationParameters) =
        let ip = FabricRuntime.GetNodeContext().IPAddressOrFQDN
        let port = parameters.CodePackageActivationContext.GetEndpoint("SuaveEndpoint").Port

        { defaultConfig with
            bindings =
            [ { defaultConfig.bindings.Head with
                    socketBinding =
                        { defaultConfig.bindings.Head.socketBinding with
                            ip = if ip = "localhost" then Net.IPAddress.Any else Net.IPAddress.Parse ip
                            port = uint16 port } } ] }, sprintf "http://%s:%d/" ip port

    let app =
        OK "Hello World"



    override __.CreateServiceInstanceListeners() =
        seq {
            yield ServiceInstanceListener(fun parameters ->
                { new ICommunicationListener with
                    member __.Abort() = ()
                    member __.CloseAsync _ = Task.FromResult() :> Task
                    member __.OpenAsync cancellationToken =
                        async {
                            let config, listeningAdress = buildConfig parameters
                            let starting, server = startWebServerAsync config app

                            Async.Start(server, cancellationToken)
                            do! starting |> Async.Ignore
                            return listeningAdress
                        } |> Async.StartAsTask
                }) }
namespace <%= namespace %>
    module Main=
    
    open Nancy
    open System

    [<Literal>]
    let uriString = "http://localhost:9000"

    [<EntryPoint>]
    let main argv =
        let nancy = new Nancy.Hosting.Self.NancyHost(new Uri(uriString))
        nancy.Start()
        Console.WriteLine("Nancy is running at {0}", uriString)
        while true do Console.ReadLine() |> ignore
        0

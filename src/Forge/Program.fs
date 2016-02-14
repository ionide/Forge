module ForgeLib
open System
open Argu
open Common
open Commands

let rec consoleLoop f =
    Console.Write("> ")
    let input = Console.ReadLine()
    let result = match input with
                 | null -> 1
                 | _ -> input.Split(' ')  |> f
    if result > 0
    then result
    else consoleLoop f

let handleInput (parser : ArgumentParser<_>) args =

    let result = parser.Parse(inputs = args,
                    ignoreMissing = true,
                    ignoreUnrecognized = true,
                    raiseOnUsage = false)
    match result.IsUsageRequested, result.GetAllResults () with
    | true, [] ->
        parser.Usage("Available commands:") |> System.Console.WriteLine
        0
    | _, [command] ->
        let h =
            match command with
            | New -> processCommand project
            | File -> processCommand file
            | Reference -> processCommand reference
            | Update -> processCommand update
            | Command.Paket -> (fun _ args -> Paket.Run args; 0)
            | Command.Fake -> (fun _ args -> Fake.Run args; 0)
            | Refresh -> (fun _ _ -> Templates.Refresh (); 0)
            | Help -> (fun _ _ -> parser.Usage "Avaliable commands" |> Console.WriteLine; 0)
            | Exit -> (fun _ _ -> 1)
        let args = args.[1 ..]
        h command args
    | _, [] ->
        System.Console.WriteLine "Command was:"
        System.Console.WriteLine ("  " + String.Join(" ", args))
        parser.Usage("Available commands:") |> System.Console.WriteLine
        0
    | _ ->
        System.Console.WriteLine "Expected only one command"
        parser.Usage("Available commands:") |> System.Console.WriteLine
        0

[<EntryPoint>]
let main argv =
    let parser = ArgumentParser.Create<Command>()
    if argv |> Array.isEmpty
    then
        parser.Usage("Available commands:") |> System.Console.WriteLine
        consoleLoop ^ handleInput parser
    else handleInput parser argv

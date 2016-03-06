module Forge.App

open System
open Argu
open Forge.Commands

// Console Configuration
Console.Title <- "FORGE"
Console.OutputEncoding <- System.Text.Encoding.UTF8

let parser = ArgumentParser.Create<Command>()


let rec consoleLoop () =
    trace Environment.CurrentDirectory
    Console.Write "λ "
    let input = Console.ReadLine()
    let result = match input with
                 | null -> Result.Exit
                 | _ -> input.Split ' '  |> processMain
    match result with
    | Continue -> consoleLoop ()
    | Help ->
        parser.Usage "Available commands:" |> printfn "%s"
        consoleLoop()
    | _ -> 1

[<EntryPoint>]
let main argv =
    match argv with
    | [||] ->
        parser.Usage "Available commands:" |> System.Console.WriteLine
        consoleLoop ()
    | _ ->        
        match processMain argv with
        | Continue -> consoleLoop ()
        | Help ->
            parser.Usage "Available commands:" |> printfn "%s"
            consoleLoop()
        | _ -> 1

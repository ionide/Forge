module Forge.App

open System
open Argu
open Forge.Commands

// Console Configuration
Console.Title <- "FORGE"
Console.OutputEncoding <- System.Text.Encoding.UTF8

let parser = ArgumentParser.Create<Command>()


let rec consoleLoop () =
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
    if argv |> Array.isEmpty
    then
        parser.Usage "Available commands:" |> System.Console.WriteLine
        consoleLoop ()
    else
        let result = processMain argv
        match result with
        | Continue -> consoleLoop ()
        | Help ->
            parser.Usage "Available commands:" |> printfn "%s"
            consoleLoop()
        | _ -> 1

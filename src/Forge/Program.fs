module Forge.App

open System
open Argu
open Forge.Commands

// Console Configuration
Console.Title <- "FORGE"
Console.OutputEncoding <- System.Text.Encoding.UTF8

let defaultForeground = Console.ForegroundColor
let defaultBackground = Console.BackgroundColor

let red         = ConsoleColor.Red
let darkRed     = ConsoleColor.DarkRed
let blue        = ConsoleColor.Blue
let darkBlue    = ConsoleColor.DarkBlue
let darkCyan    = ConsoleColor.DarkCyan
let cyan        = ConsoleColor.Cyan
let grey        = ConsoleColor.Gray
let darkGrey    = ConsoleColor.DarkGray
let darkGreen   = ConsoleColor.DarkGreen
let darkMagenta = ConsoleColor.DarkMagenta
let green       = ConsoleColor.Green
let yellow      = ConsoleColor.Yellow

let parser = ArgumentParser.Create<Command>()

let write color (msg:string) =
    Console.ForegroundColor <- color
    Console.Write msg
    Console.ForegroundColor <- defaultForeground

let writeln color (msg:string) =
    Console.ForegroundColor <- color
    Console.WriteLine msg
    Console.ForegroundColor <- defaultForeground



let highlight fcol bcol (msg:string) =
    Console.ForegroundColor <- fcol
    Console.BackgroundColor <- bcol
    Console.Write msg
    Console.ForegroundColor <- defaultForeground
    Console.BackgroundColor <- defaultBackground

let highlightln fcol bcol (msg:string) =
    Console.ForegroundColor <- fcol
    Console.BackgroundColor <- bcol
    Console.WriteLine msg
    Console.ForegroundColor <- defaultForeground
    Console.BackgroundColor <- defaultBackground


let rec consoleLoop () =
    write   green Environment.CurrentDirectory
    writeln darkRed " [-FORGE-] "
    Console.Write "λ "

    match Console.ReadLine() with
    | null  -> Result.Exit
    | input -> input.Split ' '  |> interactive
    |> function
    | Continue    -> consoleLoop ()
    | Result.Exit -> 1

let (|ShouldPrompt|_|) (argv : string []) =
    let dirs = System.IO.Directory.EnumerateDirectories Environment.CurrentDirectory
    let files = System.IO.Directory.EnumerateFiles Environment.CurrentDirectory

    if argv.Length <> 0 && argv.[argv.Length - 1] = "--no-prompt" then None
    elif dirs |> Seq.isEmpty && files |> Seq.isEmpty then None
    elif dirs |> Seq.exists(fun d -> d.Contains ".git" || d.Contains ".paket" || d.Contains "packages") then None
    elif files |> Seq.exists(fun d -> d.Contains ".sln" || d.Contains ".gitignore" || d.Contains "paket.dependencies") then None
    else Some ()

[<EntryPoint>]
let main argv =
    let k =
        match argv with
        | ShouldPrompt _ ->
            writeln yellow "\nForge should be run from solution/repository root. Please ensure you don't run it from folder containing other solutions"
            writeln yellow "\nDo You want to continue? [Y/n]"
            Console.ReadLine ()
        | _ -> ""
    if k = "Y" || k = "" then
        match argv with
        | [||] ->
            writeln cyan "\nInitializing Forge... use -h or --help to see commands\n"
            consoleLoop ()
        | _ ->
            match singlePass argv with
            | Continue -> consoleLoop ()
            | Result.Exit -> 1
    else
        0
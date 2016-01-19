module Common
open System
open System.IO
open Fake

let (^) = (<|)

let exeLocation = System.Reflection.Assembly.GetEntryAssembly().Location |> Path.GetDirectoryName
let templatesLocation = exeLocation </> "templates"
let directory = System.Environment.CurrentDirectory
let packagesDirectory = directory </> "packages"

let paketLocation = exeLocation </> "Tools" </> "Paket"
let fakeLocation = exeLocation </> "Tools" </> "FAKE"
let fakeToolLocation = fakeLocation </> "tools"

let prompt text =
    printfn text
    Console.Write("> ")
    Console.ReadLine()

let promptSelect text list =
    printfn text
    list |> Seq.iter (printfn " - %s")
    printfn ""
    Console.Write("> ")
    Console.ReadLine()

let run cmd args dir =
    if execProcess( fun info ->
        info.FileName <- cmd
        if not ^ String.IsNullOrWhiteSpace dir then
            info.WorkingDirectory <- dir
        info.Arguments <- args
    ) System.TimeSpan.MaxValue = false then
        traceError ^ sprintf "Error while running '%s' with args: %s" cmd args
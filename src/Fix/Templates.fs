module Templates
open Common
open Fake
open Fake.Git
open System.IO

let Refresh () =
    printfn "Getting templates..."
    exeLocation </> "templates" |> FileHelper.CleanDir
    Repository.cloneSingleBranch exeLocation "https://github.com/fsprojects/generator-fsharp.git" "templates" "templates"

let GetList () = 
    Directory.GetDirectories(templatesLocation)
    |> Seq.map Path.GetFileName
    |> Seq.where (fun x -> not ^ x.StartsWith("."))
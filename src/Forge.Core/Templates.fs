module Forge.Templates

open Forge.Git
open System.IO

let Refresh () =
    printfn "Getting templates..."
    cleanDir templatesLocation
    cloneSingleBranch (exeLocation </> "..") "https://github.com/fsprojects/generator-fsharp.git" "templates" "templates"

let GetList () =
    Directory.GetDirectories templatesLocation
    |> Seq.map Path.GetFileName
    |> Seq.filter (fun x -> not ^ x.StartsWith ".")
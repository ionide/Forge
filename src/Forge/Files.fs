module Forge.Files

open System.IO
open Forge.ProjectSystem

let nodeType fileName =
    match Path.GetExtension fileName with
    | ".fs" -> "Compile"
    | ".config" | ".html"-> "Content"
    | _ -> "None"

let Order file1 file2 =
    Project.execOnProject (fun x -> x.OrderFiles file1 file2)

let Add fileName =
    let node = nodeType fileName
    Project.execOnProject (fun x -> x.AddFile fileName node)
    directory </> fileName |> createFile

let Remove fileName =
    Project.execOnProject (fun x -> x.RemoveFile fileName)
    directory </> fileName |> deleteFile

let List () =
    let listFilesOfProject (project:ProjectFile) =
        project.ProjectFiles
        |> List.iter (printfn "%s")
        project

    Project.execOnProject listFilesOfProject
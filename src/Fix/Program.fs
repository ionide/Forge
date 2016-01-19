module FixLib

open Fake
open Fake.FileHelper
open Fix.ProjectSystem
open System.IO
open System
open System.Diagnostics
open Common



let promptNoProjectFound () = printfn "No project found in this directory."

let alterProject project (f : ProjectFile -> ProjectFile) =
    let fsProj = ProjectFile.FromFile(project)
    let updatedProject = fsProj |> f
    updatedProject.Save(project)

let nodeType fileName =
    match Path.GetExtension fileName with
    | ".fs" -> "Compile"
    | ".config" | ".html"-> "Content"
    | _ -> "None"

let addFileToProject fileName project nodeType = alterProject project (fun x -> x.AddFile fileName nodeType)
let removeFileFromProject fileName project _ = alterProject project (fun x -> x.RemoveFile fileName)
let addReferenceToProject reference project = alterProject project (fun x -> x.AddReference reference)
let removeReferenceOfProject reference project = alterProject project (fun x -> x.RemoveReference reference)

let listReferencesOfProject project =
    let fsProj = ProjectFile.FromFile(project)
    fsProj.References
    |> List.iter (fun i -> printfn "%s" i)

let listFilesOfProject project =
    let fsProj = ProjectFile.FromFile(project)
    fsProj.ProjectFiles
    |> List.iter (fun i -> printfn "%s" i)

let getProjects() =
    DirectoryInfo(directory) |> Fake.FileSystemHelper.filesInDirMatching "*.fsproj"

let file fileName f =
    let node = nodeType fileName
    match getProjects() with
    | [| project |] -> f fileName project.Name node
    | [||] -> promptNoProjectFound()
    | projects ->
        let project = projects |> Seq.map (fun x -> x.Name) |> promptSelect "Choose a project:"
        f fileName project node

let Order file1 file2 =
    let orderFiles project = alterProject project (fun x -> x.OrderFiles file1 file2)

    match getProjects() with
    | [| project |] -> orderFiles project.Name
    | [||] -> promptNoProjectFound()
    | projects ->
        let project = projects |> Seq.map (fun x -> x.Name) |> promptSelect "Choose a project:"
        orderFiles project

let Add fileName =
    file fileName addFileToProject
    Path.Combine(directory, fileName) |> Fake.FileHelper.CreateFile

let executeForProject exec =
    match getProjects() with
    | [| project |] -> exec project.Name
    | [||] -> promptNoProjectFound()
    | projects ->
        let project = projects |> Seq.map (fun x -> x.Name) |> promptSelect "Choose a project:"
        exec project

let AddReference reference =
    let add = addReferenceToProject reference
    executeForProject add

let RemoveReference reference =
    let remove = removeReferenceOfProject reference
    executeForProject remove

let ListReference() =
    executeForProject listReferencesOfProject

let ListFiles() =
    executeForProject listFilesOfProject

let Remove fileName =
    file fileName removeFileFromProject
    directory </> fileName |> Fake.FileHelper.DeleteFile



let Help () =
    printfn"Fix (Mix for F#)\n\
            Available Commands:\n\n\
            new [projectName] [projectDir] [templateName] [--no-paket] - Creates a new project\
          \n                      with the given name, in given directory\
          \n                      (relative to working directory) and given template.\
          \n                      If parameters are not provided, program prompts user for them\n\
            file add [fileName] - Adds a file to the current folder and project.\
          \n                      If more than one project is in the current\
          \n                      directory you will be prompted which to use.\n\
            file remove [fileName]\
          \n                    - Removes the filename from disk and the project.\
          \n                      If more than one project is in the current\
          \n                      directory you will be prompted which to use.\n\
            file list           - List all files\n\
            file order [file1] [file2]\
          \n                    - Moves file1 immediately before file2 in the project.
          \n                      If more than one project is in the current\
          \n                      directory you will be prompted which to use.\n\
            reference add [reference]\
          \n                    - Add reference to the current project.\
          \n                      If more than one project is in the current\
          \n                      directory you will be prompted which to use.\n\
            reference remove [reference]\
                                - Remove reference from the current project.\
          \n                      If more than one project is in the current\
          \n                      directory you will be prompted which to use.\n\
            reference list      - list all references\n\
            update paket        - Updates Paket to latest version\n\
            update fake         - Updates FAKE to latest version\n\
            paket [args]        - Runs Paket with given arguments\n\
            fake [args]         - Runs FAKE with given arguments\n\
            refresh             - Refreshes the template cache\n\
            help                - Displays this help\n\
            exit                - Exit interactive mode\n"


let rec consoleLoop f =
    Console.Write("> ")
    let input = Console.ReadLine()
    let result = input.Split(' ') |> Array.toList |> f
    if result > 0
    then result
    else consoleLoop f

//TODO: Better input handling, maybe Argu ?
let handleInput = function
    | [ "new" ] -> Project.New "" "" "" true; 1
    | [ "new"; "--no-paket" ] -> Project.New "" "" "" false; 1
    | [ "new"; projectName ] -> Project.New projectName "" "" true; 1
    | [ "new"; projectName; "--no-paket"] -> Project.New projectName "" "" false; 1
    | [ "new"; projectName; projectDir ] -> Project.New projectName projectDir "" true; 1
    | [ "new"; projectName; projectDir; "--no-paket" ] -> Project.New projectName projectDir "" false; 1
    | [ "new"; projectName; projectDir; templateName ] -> Project.New projectName projectDir templateName true; 1
    | [ "new"; projectName; projectDir; templateName; "--no-paket" ] -> Project.New projectName projectDir templateName false; 1

    | [ "file"; "add"; fileName ] -> Add fileName; 0
    | [ "file"; "remove"; fileName ] -> Remove fileName; 0
    | [ "file"; "list"] -> ListFiles(); 0
    | [ "file"; "order"; file1; file2 ] -> Order file1 file2; 0

    | [ "reference"; "add"; fileName ] -> AddReference fileName; 0
    | [ "reference"; "remove"; fileName ] -> RemoveReference fileName; 0
    | [ "reference"; "list"] -> ListReference(); 0

    | [ "update"; "paket"] -> Paket.Update (); 0
    | [ "update"; "fake"] -> Fake.Update (); 0
    | "paket"::xs -> Paket.Run xs; 0
    | "fake"::xs -> Fake.Run xs; 0
    | [ "refresh" ] -> Templates.Refresh(); 0
    | [ "exit" ] -> 1
    | _ -> Help(); 0


[<EntryPoint>]
let main argv =
    if argv |> Array.isEmpty
    then
        Help ()
        consoleLoop handleInput
    else handleInput (argv |> Array.toList)

module FixLib

open Fake.Git
open Fake.FileHelper
open Fix.ProjectSystem
open System.IO
open System

let exeLocation = System.Reflection.Assembly.GetEntryAssembly().Location |> Path.GetDirectoryName
let directory = System.Environment.CurrentDirectory

let RefreshTemplates () =
    printfn "Getting templates..."
    Path.Combine(exeLocation, "templates") |> Fake.FileHelper.CleanDir 
    Repository.cloneSingleBranch exeLocation "https://github.com/fsprojects/generator-fsharp.git" "templates" "templates"

let applicationNameToProjectName folder projectName =
    let applicationName = "ApplicationName"
    let files = Directory.GetFiles folder |> Seq.where (fun x -> x.Contains applicationName)
    files |> Seq.iter (fun x -> File.Move(x, x.Replace(applicationName, projectName)))

let sed (find:string) replace folder =
    printfn "Changing %s to %s" find replace
    folder 
    |> Directory.GetFiles
    |> Seq.iter (fun x -> 
                    let contents = File.ReadAllText(x).Replace(find, replace)
                    File.WriteAllText(x, contents))

let promptList list =
    list |> Seq.iter (fun x -> printfn " - %s" x)
    printfn ""
    Console.Write("> ")
    Console.ReadLine()

let New projectName =
    let templatePath = Path.Combine(exeLocation, "templates")
    let projectFolder = Path.Combine(directory, projectName)

    if not <| Directory.Exists templatePath
    then RefreshTemplates ()

    printfn "Choose a template:"
    let templates = Directory.GetDirectories(templatePath) 
                    |> Seq.map Path.GetFileName
                    |> Seq.where (fun x -> not <| x.StartsWith("."))
    
    let templateChoice = promptList templates
    printfn "Fixing template %s" templateChoice
    let templateDir = Path.Combine(templatePath, templateChoice)

    Fake.FileHelper.CopyDir projectFolder templateDir (fun _ -> true)

    printfn "Changing filenames from ApplicationName.* to %s.*" projectName
    applicationNameToProjectName projectFolder projectName

    sed "<%= namespace %>" projectName projectFolder
    
    let guid = Guid.NewGuid().ToString()
    sed "<%= guid %>" guid projectFolder 
    printfn "Done!"

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

let file fileName f =
    let projects = DirectoryInfo(directory) |> Fake.FileSystemHelper.filesInDirMatching "*.fsproj"
    let node = nodeType fileName
    match projects with
    | [| project |] -> f fileName project.Name node
    | [||] -> printfn "No project found in this directory."
    | _ -> 
        let project = projects |> Seq.map (fun x -> x.Name) |> promptList 
        f fileName project node

let Add fileName =
    file fileName addFileToProject
    Path.Combine(directory, fileName) |> Fake.FileHelper.CreateFile

let Remove fileName =
    file fileName removeFileFromProject
    Path.Combine(directory, fileName) |> Fake.FileHelper.DeleteFile


let Help () = 
    printfn "Fix (Mix for F#)"
    printfn "Available Commands:"
    printfn " new [projectName]   - Creates a new project with the given name"
    printfn " file add [fileName] - Adds a file to the current folder and project."
    printfn "                       If more than one project is in the current"
    printfn "                       directory you will be prompted which to use."
    printfn " file remove [fileName]"
    printfn "                     - Removes the filename from disk and the project."
    printfn "                       If more than one project is in the current"
    printfn "                       directory you will be prompted which to use."
    printfn " refresh             - Refreshes the template cache"
    printfn " help                - Displays this help"
    printfn " exit                - Exit interactive mode"
    printfn ""

let rec consoleLoop f =
    Console.Write("> ")
    let input = Console.ReadLine()
    let result = input.Split(' ') |> f
    if result > 0
    then result
    else consoleLoop f 

let handleInput = function
    | [| "new"; projectName |] -> New projectName; 1
    | [| "file"; "add"; fileName |] -> Add fileName; 0
    | [| "file"; "remove"; fileName |] -> Remove fileName; 0
    | [| "refresh" |] -> RefreshTemplates (); 0
    | [| "exit" |] -> 1
    | _ -> Help(); 0


[<EntryPoint>]
let main argv = 
    if argv |> Array.isEmpty
    then
        Help () 
        consoleLoop handleInput
    else handleInput argv
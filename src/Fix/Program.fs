module Fix

open Fake.Git
open Fake.FileHelper
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
    folder 
    |> Directory.GetFiles
    |> Seq.iter (fun x -> 
                    let contents = File.ReadAllText(x).Replace(find, replace)
                    File.WriteAllText(x, contents))

let New projectName =
    let templatePath = Path.Combine(exeLocation, "templates")
    let projectFolder = Path.Combine(directory, projectName)

    if not <| Directory.Exists templatePath
    then RefreshTemplates ()

    printfn "Choose a template:"
    let templates = Directory.GetDirectories(templatePath) 
                    |> Seq.map (fun x -> x.Replace(Path.GetDirectoryName(x) + "\\", ""))
                    |> Seq.where (fun x -> not <| x.StartsWith("."))
    
    templates |> Seq.iter (fun x -> printfn " - %s" x)

    printfn ""
    let templateChoice = Console.ReadLine()
    printfn "Fixing template %s" templateChoice
    let templateDir = Path.Combine(templatePath, templateChoice)

    Fake.FileHelper.CopyDir projectFolder templateDir (fun _ -> true)

    printfn "Changing filenames from ApplicationName.* to %s.*" projectName
    applicationNameToProjectName projectFolder projectName

    printfn "Changing namespace to %s" projectName
    sed "<%= namespace %>" projectName projectFolder
    
    let guid = Guid.NewGuid().ToString()
    printfn "Changing guid to %s" guid
    sed "<%= guid %>" guid projectFolder 
    printfn "Done!"

let Help () = 
    printfn "Fix (Mix for F#)"
    printfn "Available Commands:"
    printfn " new [projectName] - Creates a new project with the given name"
    printfn " refresh           - Refreshes the template cache"
    printfn " help              - Displays this help"
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
    | [| "refresh" |] -> RefreshTemplates (); 0
    | [| "exit" |] -> 1
    | _ -> Help(); 0


[<EntryPoint>]
let main argv = 
    if argv |> Array.isEmpty
    then consoleLoop handleInput
    else handleInput argv
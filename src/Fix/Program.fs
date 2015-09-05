module Fix

open Fake.ProcessHelper
open Fake.Git
open System.IO
open System

let RefreshTemplates path =
    printfn "Getting templates..."
    Repository.cloneSingleBranch "." "https://github.com/fsprojects/generator-fsharp.git" "templates" "templates"

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
    let directory = System.Environment.CurrentDirectory
    let templatePath = Path.Combine(directory, "templates")
    let projectFolder = Path.Combine(directory, projectName)

    if not <| Directory.Exists templatePath
    then RefreshTemplates templatePath

    printfn "Choose a template:"
    let templates = Directory.GetDirectories(templatePath) 
                    |> Seq.map (fun x -> x.Replace(Path.GetDirectoryName(x) + "\\", ""))
                    |> Seq.where (fun x -> not <| x.StartsWith("."))
    
    templates |> Seq.iter (fun x -> printfn "%s" x)

    let templateChoice = Console.ReadLine()
    printfn "Fixing template %s" templateChoice
    
    Directory.Move(Path.Combine(templatePath, templateChoice), projectFolder)

    printfn "Changing filenames from ApplicationName.* to %s.*" projectName
    applicationNameToProjectName projectFolder projectName

    printfn "Changing namespace to %s" projectName
    projectFolder |> sed "<%= namespace %>" projectName
    
    let guid = Guid.NewGuid().ToString()
    printfn "Changing guid to %s" guid
    projectFolder |> sed "<%= guid %>" guid
    ()


[<EntryPoint>]
let main argv = 
    New "TestProject"
    0
module Fix

open Fake.ProcessHelper
open Fake.Git
open System.IO
open System

[<EntryPoint>]
let main argv = 
    let directory = System.Environment.CurrentDirectory
    let projectName = "MyProject"
    let templatePath = Path.Combine(directory, "templates")
    let projectFolder = Path.Combine(directory, projectName)

    if not <| Directory.Exists templatePath
    then printfn "Getting templates..."
         Repository.cloneSingleBranch "." "https://github.com/fsprojects/generator-fsharp.git" "templates" "templates"

    printfn "Choose a template:"
    let templates = Directory.GetDirectories(templatePath) 
                    |> Seq.map (fun x -> x.Replace(Path.GetDirectoryName(x) + "\\", ""))
                    |> Seq.where (fun x -> not <| x.StartsWith("."))
    
    templates |> Seq.iter (fun x -> printfn "%s" x)

    let templateChoice = Console.ReadLine()
    printfn "Fixing template %s" templateChoice
    
    Directory.Move(Path.Combine(templatePath, templateChoice), projectFolder)

    
    //perform replacements
    //ApplicationName.* files to projectName.*
    //<%= namespace %> to projectName
    //<%= guid %> to Guid.NewGuid()

    //what about solution file?

    let foo = Console.ReadLine()
    Directory.Delete(templatePath)
    0
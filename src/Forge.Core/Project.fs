module Forge.Project

open System
open System.IO
open Forge.ProjectSystem

let applicationNameToProjectName folder projectName =
    let applicationName = "ApplicationName"
    let files = Directory.GetFiles folder |> Seq.where (fun x -> x.Contains applicationName)
    files |> Seq.iter (fun x -> File.Move(x, x.Replace(applicationName, projectName)))

let sed (find:string) replace folder =
    folder
    |> Directory.GetFiles
    |> Seq.iter (fun x ->
                    let r = replace x
                    let contents = File.ReadAllText(x).Replace(find, r)
                    File.WriteAllText(x, contents))

let relative (path1 : string) (path2 : string) =
    let p1, p2 = Uri path1, Uri path2
    Uri.UnescapeDataString(
        p2.MakeRelativeUri(p1)
          .ToString()
          .Replace('/', Path.DirectorySeparatorChar)
    )

let getProjects() =
    DirectoryInfo(directory) |> filesInDirMatching "*.fsproj"


let New projectName projectDir templateName paket fake =
    if not ^ Directory.Exists templatesLocation then Templates.Refresh ()

    let templates = Templates.GetList()

    let projectName' = defaultArg projectName ^ prompt "Enter project name:"
    let projectDir' = defaultArg projectDir ^ prompt "Enter project directory (relative to working directory):" 
    let templateName' = defaultArg templateName (templates |> promptSelect "Choose a template:")
    let projectFolder = directory </> projectDir' </> projectName'
    let templateDir = templatesLocation </> templateName'

    if templates |> Seq.contains templateName' then
        printfn "Generating project..."
        copyDir projectFolder templateDir (fun _ -> true)
        applicationNameToProjectName projectFolder projectName'

        sed "<%= namespace %>" (fun _ -> projectName') projectFolder
        sed "<%= guid %>" (fun _ -> Guid.NewGuid().ToString()) projectFolder
        sed "<%= paketPath %>" (relative directory) projectFolder
        sed "<%= packagesPath %>" (relative packagesDirectory) projectFolder
        if paket then
            Paket.Copy directory
            Paket.Run ["convert-from-nuget";"-f"]
        if fake then
            if paket then
                Paket.Run ["add"; "nuget"; "FAKE"]
            Fake.Copy directory

        printfn "Done!"
    else
        printfn "Wrong template name"
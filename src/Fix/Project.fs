module Project
open Fake
open Common
open System
open System.IO
open Fix.ProjectSystem

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
    let p1 = Uri(path1)
    let p2 = Uri(path2)
    Uri.UnescapeDataString(
        p2.MakeRelativeUri(p1)
          .ToString()
          .Replace('/', Path.DirectorySeparatorChar)
    )

let getProjects() =
    DirectoryInfo(directory) |> Fake.FileSystemHelper.filesInDirMatching "*.fsproj"

let alter (f : ProjectFile -> ProjectFile) project =
    let fsProj = ProjectFile.FromFile(project)
    let updatedProject = fsProj |> f
    updatedProject.Save(project)

let execOnProject fn =
    match getProjects() with
    | [| project |] -> project.Name |> alter fn
    | [||] -> printfn "No project found in this directory."
    | projects ->
        let project = projects |> Seq.map (fun x -> x.Name) |> promptSelect "Choose a project:"
        project |> alter fn



let New projectName projectDir templateName paket =
    if not ^ Directory.Exists templatesLocation then Templates.Refresh ()

    let projectName' = if String.IsNullOrWhiteSpace projectName then prompt "Enter project name:" else projectName
    let projectDir' = if String.IsNullOrWhiteSpace projectDir then prompt "Enter project directory (relative to working directory):" else projectDir
    let templateName' = if String.IsNullOrWhiteSpace templateName then Templates.GetList() |> promptSelect "Choose a template:" else templateName
    let projectFolder = directory </> projectDir' </> projectName'
    let templateDir = templatesLocation </> templateName'

    printfn "Generating project..."

    Fake.FileHelper.CopyDir projectFolder templateDir (fun _ -> true)
    applicationNameToProjectName projectFolder projectName'

    sed "<%= namespace %>" (fun _ -> projectName') projectFolder
    sed "<%= guid %>" (fun _ -> Guid.NewGuid().ToString()) projectFolder
    sed "<%= paketPath %>" (relative directory) projectFolder
    sed "<%= packagesPath %>" (relative packagesDirectory) projectFolder
    if paket then
        Paket.Copy directory
        Paket.Run ["convert-from-nuget";"-f"]
    printfn "Done!"
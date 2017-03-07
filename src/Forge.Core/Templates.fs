module Forge.Templates

open Forge.Git
open Forge.ProjectSystem
open System.IO
open System
open Mono.Unix.Native
open FSharp.Data

/// Result type for project comparisons.
type ProjectComparison =
    { TemplateProjectFileName: string
      ProjectFileName: string
      MissingFiles: string seq
      DuplicateFiles: string seq
      UnorderedFiles: string seq }

      member this.HasErrors =
        not (Seq.isEmpty this.MissingFiles &&
             Seq.isEmpty this.UnorderedFiles &&
             Seq.isEmpty this.DuplicateFiles)

/// Compares the given project files againts the template project and returns which files are missing.
/// For F# projects it is also reporting unordered files.
let findMissingFiles templateProject projects =
    let isFSharpProject file = file |> String.endsWith ".fsproj"

    let templateFiles = (FsProject.load templateProject).SourceFiles.Files
    let templateFilesSet = Set.ofSeq templateFiles

    projects
    |> Seq.map (fun fileName -> FsProject.load fileName, fileName)
    |> Seq.map (fun (ps,fn) ->
            let missingFiles = Set.difference templateFilesSet (Set.ofSeq ps.SourceFiles.Files)

            let duplicateFiles =
                Seq.duplicates ps.SourceFiles.Files

            let unorderedFiles =
                if not <| isFSharpProject templateProject then [] else
                if not <| Seq.isEmpty missingFiles then [] else
                let remainingFiles = ps.SourceFiles.Files |> List.filter (fun file -> Set.contains file templateFilesSet)
                if remainingFiles.Length <> templateFiles.Length then [] else

                templateFiles
                |> List.zip remainingFiles
                |> List.filter (fun (a,b) -> a <> b)
                |> List.map fst

            { TemplateProjectFileName = templateProject
              ProjectFileName = fn
              MissingFiles = missingFiles
              DuplicateFiles = duplicateFiles
              UnorderedFiles = unorderedFiles })
    |> Seq.filter (fun pc -> pc.HasErrors)

/// Compares the given projects to the template project and adds all missing files to the projects if needed.
let FixMissingFiles templateProject projects =


    findMissingFiles templateProject projects
    |> Seq.iter (fun pc ->
            let addMissing project missingFile =
                printfn "Adding %s to %s" missingFile pc.ProjectFileName
                project |> FsProject.addSourceFile  "" {SourceFile.Include = missingFile; Condition = None; OnBuild = BuildAction.Compile; Link = None; Copy = None; Paket = None}


            let project = FsProject.load pc.ProjectFileName
            if not ^ Seq.isEmpty pc.MissingFiles then
                pc.MissingFiles
                |> Seq.fold addMissing project
                |> FsProject.save pc.ProjectFileName )

/// It removes duplicate files from the project files.
let RemoveDuplicateFiles projects =
    projects
    |> Seq.iter (fun fileName ->
            let project = FsProject.load fileName
            let duplicates =
                Seq.duplicates project.SourceFiles.Files
            if not ^ Seq.isEmpty duplicates then
                let newProject = project //TODO: .RemoveDuplicates()
                newProject |> FsProject.save fileName)

/// Compares the given projects to the template project and adds all missing files to the projects if needed.
/// It also removes duplicate files from the project files.
let FixProjectFiles templateProject projects =
    FixMissingFiles templateProject projects
    RemoveDuplicateFiles projects

/// Compares the given project files againts the template project and fails if any files are missing.
/// For F# projects it is also reporting unordered files.
let CompareProjectsTo templateProject projects =
    let errors =
        findMissingFiles templateProject projects
        |> Seq.map (fun pc ->
                seq {
                    if Seq.isEmpty pc.MissingFiles |> not then
                        yield sprintf "Missing files in %s:\r\n%s" pc.ProjectFileName (String.toLines pc.MissingFiles)
                    if Seq.isEmpty pc.UnorderedFiles |> not then
                        yield sprintf "Unordered files in %s:\r\n%s" pc.ProjectFileName (String.toLines pc.UnorderedFiles)
                    if Seq.isEmpty pc.DuplicateFiles |> not then
                        yield sprintf "Duplicate files in %s:\r\n%s" pc.ProjectFileName (String.toLines pc.DuplicateFiles)}
                    |> String.toLines)
        |> String.toLines

    if String.isNotNullOrEmpty errors then failwith errors


let Refresh () =
    printfn "Getting templates..."
    cleanDir templatesLocation
    cloneSingleBranch (exeLocation </> "..") "https://github.com/fsharp-editing/Forge.git" "templates" "templates"

let EnsureTemplatesExist () =
    if not ^ Directory.Exists templatesLocation then Refresh ()

let GetList () =
    if Directory.Exists templatesLocation then
        Directory.GetDirectories templatesLocation
        |> Seq.map Path.GetFileName
        |> Seq.filter (fun x -> not ^ x.StartsWith ".")
    else Seq.empty

//type Definitions = JsonProvider<""" {"Templates": [ { "name": "Console Application", "value": "console" }], "Files": [{ "name": "F# Module", "value": "fs", "extension": "fs" }]}""">


module Project =
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

    let getProjects() =
        DirectoryInfo(directory) |> filesInDirMatching "*.fsproj"


    let New projectName projectDir templateName _ fake =
        EnsureTemplatesExist ()

        let templates = GetList()

        let pathCheck path = 
            try Path.GetFullPath path |> ignore; isValidPath path
            with _ -> false

        let projectName' =
            match projectName with
            | Some p -> p
            | None -> promptCheck "Enter project name:" (fun p -> pathCheck (directory </> p)) (sprintf "%s is not valid project name.")
        let projectDir' =
            match projectDir with
            | Some p -> p
            | None -> prompt "Enter project directory (relative to working directory):"
        let templateName' =
            match templateName with
            | Some p -> p
            | None -> (templates |> promptSelect "Choose a template:")
        let projectFolder = directory </> projectDir' </> projectName'
        let templateDir = templatesLocation </> templateName'
        let gitignorePath = (templatesLocation </> ".vcsignore" </> ".gitignore")

        if templates |> Seq.contains templateName' then
            printfn "Generating project..."
            copyDir projectFolder templateDir (fun _ -> true)
            applicationNameToProjectName projectFolder projectName'
            if Directory.GetFiles directory |> Seq.exists (fun n -> n.EndsWith ".gitignore") |> not then
              File.Copy(gitignorePath, (directory </> ".gitignore"), false)

            sed "<%= namespace %>" (fun _ -> projectName') projectFolder
            sed "<%= guid %>" (fun _ -> Guid.NewGuid().ToString()) projectFolder
            sed "<%= paketPath %>" (relative directory) projectFolder
            sed "<%= packagesPath %>" (relative packagesDirectory) projectFolder

            Paket.Copy directory
            if Directory.GetFiles directory |> Seq.exists (fun n -> n.EndsWith "paket.dependencies") |> not then
                Paket.Run ["init"]

            Directory.GetFiles projectFolder
            |> Seq.tryFind (fun n -> n.EndsWith "paket.references")
            |> Option.iter (File.ReadAllLines >> Seq.iter (fun ref -> Paket.Run ["add"; "nuget"; ref; "--no-install"]) )
            Paket.Run ["install";]
            if fake then
                Paket.Run ["add"; "nuget"; "FAKE"]
                Fake.Copy directory
                let buildSh = projectDir' </> "build.sh"
                let ctn = File.ReadAllText buildSh
                let ctn = ctn.Replace("\r\n", "\n")
                File.WriteAllText(buildSh, ctn)
                if isMono then
                    let perms = FilePermissions.S_IRWXU ||| FilePermissions.S_IRGRP ||| FilePermissions.S_IROTH // 0x744
                    Syscall.chmod(buildSh, perms) |> ignore
            printfn "Done!"
        else
            printfn "Wrong template name"

module File =
    open System
    open System.IO
    open Forge.ProjectSystem
    open Forge.Json
    open Json.JsonExtensions

    let getTemplates () =
        let value = System.IO.File.ReadAllText(templateFile) |> JsonValue.Parse
        value?Files |> JsonValue.AsArray |> Array.map (fun n ->
            n?name.ToString().Replace("\"","" ),
            n?value.ToString().Replace("\"","" ),
            n?extension.ToString().Replace("\"","" ) )

    let sed (find:string) replace file =
        let r = replace file
        let contents = File.ReadAllText(file).Replace(find, r)
        File.WriteAllText(file, contents)


    let New fileName template project buildAction =
        EnsureTemplatesExist ()

        let templates = getTemplates ()
        let template' =
            match template with
            | Some t -> t
            | None -> (templates |> Array.map( fun (n,v,_) -> n,v) |> promptSelect2 "Choose a template:")
        let (_, value, ext) = templates |> Seq.find (fun (_,v,_) -> v = template')
        let oldFile = value + "." + ext
        let newFile = fileName + "." + ext
        let newFile' =  (directory </> newFile)
        let project' =
            match project with
            | Some p ->
                let p = if p |> String.endsWith ".fsproj" then p else p + ".fsproj"
                if File.Exists p then
                    Some p
                else
                    traceWarning "Provided project not found. Trying to find project file automatically."
                    ProjectManager.Furnace.tryFindProject newFile'
            | None -> ProjectManager.Furnace.tryFindProject newFile'

        System.IO.File.Copy(filesLocation </> oldFile, newFile')
        match project' with
        | Some f ->
            ProjectManager.Furnace.loadFsProject f
            |> ProjectManager.Furnace.addSourceFile (newFile, None, buildAction, None, None, None)
            |> ignore
        | None ->
            traceWarning "Project file not found, use `add file --project<string>` to add file to project"

        sed "<%= namespace %>" (fun _ -> fileName.Split('\\', '/', '.') |> Seq.last ) newFile'
        sed "<%= guid %>" (fun _ -> System.Guid.NewGuid().ToString()) newFile'
        sed "<%= paketPath %>" (relative directory) newFile'
        sed "<%= packagesPath %>" (relative packagesDirectory) newFile'

module Solution =
    let New name =
        EnsureTemplatesExist ()
        let template = templatesLocation </> ".sln" </> "ApplicationName.sln"
        let newName = directory </> (name + ".sln")
        File.Copy(template, newName, false)

module Scaffold =
    let New () =
        EnsureTemplatesExist ()
        printfn "Cloning project scaffold..."
        let whiteSpaceProtectedDir = ("\"" + directory + "\"")
        cloneSingleBranch (exeLocation </> "..") "https://github.com/fsprojects/ProjectScaffold.git" "master" whiteSpaceProtectedDir
        ()

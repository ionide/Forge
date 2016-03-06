module Forge.Templates

open Forge.Git
open Forge.ProjectSystem
open System.IO


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

    let templateFiles = (ProjectFile.FromFile templateProject).Files
    let templateFilesSet = Set.ofSeq templateFiles

    projects
    |> Seq.map (fun fileName -> ProjectFile.FromFile fileName)
    |> Seq.map (fun ps ->
            let missingFiles = Set.difference templateFilesSet (Set.ofSeq ps.Files)

            let unorderedFiles =
                if not <| isFSharpProject templateProject then [] else
                if not <| Seq.isEmpty missingFiles then [] else
                let remainingFiles = ps.Files |> List.filter (fun file -> Set.contains file templateFilesSet)
                if remainingFiles.Length <> templateFiles.Length then [] else

                templateFiles
                |> List.zip remainingFiles
                |> List.filter (fun (a,b) -> a <> b)
                |> List.map fst

            { TemplateProjectFileName = templateProject
              ProjectFileName = ps.ProjectFileName
              MissingFiles = missingFiles
              DuplicateFiles = ps.FindDuplicateFiles()
              UnorderedFiles = unorderedFiles })
    |> Seq.filter (fun pc -> pc.HasErrors)

/// Compares the given projects to the template project and adds all missing files to the projects if needed.
let FixMissingFiles templateProject projects =
    let addMissing (project:ProjectFile) missingFile =
        printfn "Adding %s to %s" missingFile project.ProjectFileName
        project.AddFile missingFile "Compile"

    findMissingFiles templateProject projects
    |> Seq.iter (fun pc ->
            let project = ProjectFile.FromFile pc.ProjectFileName
            if not (Seq.isEmpty pc.MissingFiles) then
                let newProject = Seq.fold addMissing project pc.MissingFiles
                newProject.Save())

/// It removes duplicate files from the project files.
let RemoveDuplicateFiles projects =
    projects
    |> Seq.iter (fun fileName ->
            let project = ProjectFile.FromFile fileName
            if not (project.FindDuplicateFiles().IsEmpty) then
                let newProject = project.RemoveDuplicates()
                newProject.Save())

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
let refresh () =
    printfn "Getting templates..."
    cleanDir templatesLocation
    cloneSingleBranch (exeLocation </> "..") "https://github.com/fsprojects/generator-fsharp.git" "templates" "templates"

let GetList () =
    Directory.GetDirectories templatesLocation
    |> Seq.map Path.GetFileName
    |> Seq.filter (fun x -> not ^ x.StartsWith ".")
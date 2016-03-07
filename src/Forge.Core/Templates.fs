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
                project |> FsProject.addSourceFile  "" {SourceFile.Include = missingFile; Condition = None; OnBuild = BuildAction.Compile; Link = None; Copy = None}
    
    
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
    cloneSingleBranch (exeLocation </> "..") "https://github.com/fsprojects/generator-fsharp.git" "templates" "templates"

let GetList () =
    Directory.GetDirectories templatesLocation
    |> Seq.map Path.GetFileName
    |> Seq.filter (fun x -> not ^ x.StartsWith ".")
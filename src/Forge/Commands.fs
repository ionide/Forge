module Forge.Commands

open System
open System.IO
open System.Text
open Argu
open Forge
open Forge.Prelude
open Forge.ProjectSystem
open Forge.ProjectManager
open Forge.Fake

/// Custom Command Line Argument
type CLIArg = CustomCommandLineAttribute
/// Alternative Command Line Argument
type CLIAlt = AltCommandLineAttribute

type Result =
| Continue
| Exit


//-----------------------------------------------------------------
// Main commands
//-----------------------------------------------------------------

(*  TODO Add commands
    - delete file|dir

*)

let internal getCaseName (item:'T) =
    let uci,_ = Microsoft.FSharp.Reflection.FSharpValue.GetUnionFields(item, typeof<'T>)
    match uci.GetCustomAttributes(typeof<CustomCommandLineAttribute>) with
    | [||] -> uci.Name
    | arr -> (arr.[0] :?> CustomCommandLineAttribute).Name

let getUsage (x:#IArgParserTemplate) = sprintf "  %s : %s" (getCaseName x) x.Usage


type Command =
    | [<CLIArg "new">] New
    | [<CLIArg "add">] Add
    | [<CLIArg "move">] Move
    | [<CLIArg "remove">] Remove
    | [<CLIArg "rename">] Rename
    | [<CLIArg "list">] List
    | [<CLIArg "update-version">] Update
    | [<CLIArg "paket">] Paket
    | [<CLIArg "fake">] Fake
    | [<CLIArg "refresh">] Refresh
    | Version

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | New -> "<project|file|solution|scaffold> Create new file, project, solution or scaffold"
            | Add -> "<file|reference|project> Adds file, reference or project reference"
            | Move -> "<file> Move the file within the project hierarchy"
            | Remove -> "<file|folder|reference|project> Removes file, folder, reference or project reference"
            | Rename -> "<project|file> Renames file or project"
            | List -> "<files|projects|references|projectReferences|templates|gac> List files, project in solution, references, project references, avaliable templates or libraries installed in gac"
            | Update -> "<paket|fake> Updates Paket or FAKE"
            | Paket -> "Runs Paket"
            | Fake -> "Runs FAKE"
            | Refresh -> "Refreshes the template cache"
            | Version -> "Prints Forge version."



//-----------------------------------------------------------------
// New Commands
//-----------------------------------------------------------------

type NewCommand =
    | [<First>][<CLIArg "project">] Project
    | [<First>][<CLIArg "file">] File
    | [<First>][<CLIArg "solution">] Solution
    | [<First>][<CLIArg "scaffold">] Scaffold

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Project -> "Creates new project"
            | File -> "Creates new file"
            | Solution -> "Creates new solution"
            | Scaffold -> "Clones project scaffold"


type NewProjectArgs =
    | [<CLIAlt "-n">] Name of string
    | [<CLIAlt "dir">][<CLIAlt "--dir">] Folder of string
    | [<CLIAlt "-t">] Template of string
    | No_Paket
    | No_Fake
    | VSCode

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Name _ -> "Project name"
            | Folder _ -> "Project folder, relative to Forge working folder"
            | Template _ -> "Template name"
            | No_Paket -> "Don't use Paket for dependency managment"
            | No_Fake -> "Don't use FAKE for build"
            | VSCode -> "Include VSCode build and debug files"




type NewFileArgs =
    | [<CLIAlt "-n">] Name of string
    | [<CLIAlt "-t">] Template of string
    | [<CLIAlt "-p">] Project of string
    | [<CLIAlt "-s">] Solution of string
    | [<CLIArg "--build-action">] [<CLIAlt "-b">] BuildAction of string
    | [<CLIArg "--copy-to-output">] [<CLIAlt "-c">] CopyToOutputDirectory of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Name _-> "File name"
            | Template _ -> "File template"
            | Project _ -> "Project to which file will be added"
            | Solution _ -> "Solution to which file will be added"
            | BuildAction _ -> "File build action"
            | CopyToOutputDirectory _ -> "When to copy file to output directory"


type NewSolutionArgs =
    | [<CLIAlt "-n">] Name of string
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Name _-> "Solution name"


type NewScaffoldArgs =
    | [<Hidden>] NoArgs

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | _ -> "There are no arguments for this command"

//-----------------------------------------------------------------
// Add commands
//-----------------------------------------------------------------

type AddCommands =
    | [<First>][<CLIArg "file">] File
    | [<First>][<CLIArg "reference">] Reference
    | [<First>][<CLIArg "project">] Project
    interface IArgParserTemplate with
        member self.Usage = self |> function
            | File -> "Adds file to project or solution"
            | Reference -> "Adds reference to project"
            | Project -> "Adds project reference to project"


type AddFileArgs =
    | [<First>][<CLIAlt "-p">] Project of string
    | [<First>][<CLIAlt "-s">] Solution of string
    | [<CLIAlt "-n">] Name of string
    | [<CLIAlt "dir">][<CLIAlt "--dir">] Folder of string
    | [<CLIArg "--build-action">] [<CLIAlt "-ba">] BuildAction of string
    | [<CLIArg "--copy-to-output">] [<CLIAlt "-c">] CopyToOutputDirectory of string
    | [<CLIArg "--above">] [<CLIAlt "-a">] Above of string
    | [<CLIArg "--below">] [<CLIAlt "-b">] Below of string
    | Link

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Name _-> "File name"
            | Project _ -> "Add the file to this project"
            | Solution _ -> "Add the file to this solution"
            | Folder _   -> "Add the file to this folder"
            | BuildAction _ -> "File build action"
            | CopyToOutputDirectory _ -> "When to copy file to output directory"
            | Link -> "Adds as link"
            | Above _ -> "Adds above given file"
            | Below _ -> "Adds below given file"


type AddReferenceArgs =
    | [<CLIAlt "-n">] Name of string
    | [<CLIAlt "-p">] Project of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Name _-> "Reference name"
            | Project _ -> "Project to which reference will be added"

type AddProjectArgs =
    | [<CLIAlt "-n">] Name of string
    | [<CLIAlt "-p">] Project of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Name _-> "Project reference path"
            | Project _ -> "Project to which reference will be added"


//-----------------------------------------------------------------
// Remove commands
//-----------------------------------------------------------------


type RemoveCommands =
    | [<First>][<CLIArg "file">] File
    | [<First>][<CLIArg "reference">] Reference
    | [<First>][<CLIArg "project">] Project
    | [<First>][<CLIArg "folder">][<CLIAlt "dir">][<CLIAlt "--dir">] Folder

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | File -> "Removes file from project or solution"
            | Reference -> "Removes reference from project"
            | Folder -> "Removes the folder from the project or solution"
            | Project -> "Removes project reference from project"


type RemoveFileArgs =
    | [<CLIAlt "-p">] Project of string
    | [<CLIAlt "-s">] Solution of string
    | [<CLIAlt "-n">] Name of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Name _    -> "File name"
            | Project _ -> "Project from which file will be removed"
            | Solution _ -> "Solution from which file will be removed"


type RemoveReferenceArgs =
    | [<CLIAlt "-n">] Name of string
    | [<CLIAlt "-p">] Project of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Name _-> "Reference name"
            | Project _ -> "Project from which reference will be removed"

type RemoveProjectReferenceArgs =
    | [<CLIAlt "-n">] Name of string
    | [<CLIAlt "-p">] Project of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Name _-> "Path of project reference"
            | Project _ -> "Project from which reference will be removed"


type RemoveFolderArgs =
    | [<CLIAlt "-p">] Project of string
    | [<CLIAlt "-s">] Solution of string
    | [<CLIAlt "-n">] Name of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Name _    -> "Name of the folder"
            | Project _ -> "Remove folder from this project"
            | Solution  _ -> "Remove folder from this solution"


//-----------------------------------------------------------------
// Rename
//-----------------------------------------------------------------

type RenameCommands =
    | [<First>][<CLIArg "file">] File
    | [<First>][<CLIArg "project">] Project
    | [<First>][<CLIArg "folder">][<CLIAlt "dir">][<CLIAlt "--dir">] Folder

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | File -> "Renames file"
            | Project -> "Renames project"
            | Folder -> "Renames folder"


type RenameFileArgs =
    | [<CLIAlt "-n">] Name of string
    | [<CLIAlt "-r">] Rename of string
    | [<CLIAlt "-p">] Project of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Name _-> "File name"
            | Rename _ -> "New file name"
            | Project _ -> "Project containing file"


type RenameFolderArgs =
    | [<CLIAlt "-n">] Name of string
    | [<CLIAlt "-r">] Rename of string
    | [<CLIAlt "-p">] Project of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Name _-> "Folder name"
            | Rename _ -> "New folder name"
            | Project _ -> "Project containing folder"


type RenameProjectArgs =
    | [<CLIAlt "-n">] Name of string
    | [<CLIAlt "-r">] Rename of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Name _-> "Project name"
            | Rename _ -> "New project name"


//-----------------------------------------------------------------
// List commands
//-----------------------------------------------------------------


type ListCommands =
    | [<First>][<CLIArg "files">] File
    | [<First>][<CLIArg "references">] Reference
    | [<First>][<CLIArg "projects">] Project
    | [<First>][<CLIArg "gac">] GAC
    | [<First>][<CLIArg "templates">] Templates
    | [<First>][<CLIArg "projectReferences">] ProjectReferences
    | [<CLIArg "--filter">] Filter of string
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | File -> "List file from project"
            | Reference -> "List reference from project"
            | ProjectReferences -> "List project reference from project"
            | Project -> "List projects in solution"
            | Templates -> "List the templates in Forge's cache"
            | GAC -> "List the assemblies in the Global Assembly Cache"
            | Filter _ -> "Filter list via fuzzy search for this string"


type ListFilters =
    | Filter of string
    | Count of int

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Filter _ -> "Filter list via fuzzy search for this string"
            | Count _ -> "Return the x best search results"


type ListFilesArgs =
    | [<CLIAlt "-p">] Project of string
    | [<CLIAlt "-s">] Solution of string
    | [<CLIArg "--filter">] Filter of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Project _ -> "List the files in this project"
            | Solution _ -> "List the files in solution folders"
            | Filter _ -> "Filter list via fuzzy search for this string"


type ListReferencesArgs =
    | [<CLIAlt "-p">] Project of string
    | [<CLIArg "--filter">] Filter of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Project _ -> "List the references in this project"
            | Filter _ -> "Filter list via fuzzy search for this string"

type ListProjectReferencesArgs =
    | [<CLIAlt "-p">] Project of string
    | [<CLIArg "--filter">] Filter of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Project _ -> "List project references in this project"
            | Filter _ -> "Filter list via fuzzy search for this string"


type ListProjectsArgs =
    | [<CLIAlt "-s">] Solution of string
    | [<CLIAlt ("dir")>][<CLIAlt "--dir">] Folder of string
    | [<CLIArg "--filter">] Filter of string
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Solution _ -> "List the projects in this solution"
            | Folder _ -> "List the projects in this directory"
            | Filter _ -> "Filter list via fuzzy search for this string"

type ListGacArgs =
    | [<CLIArg "gac">] GAC of string

    interface IArgParserTemplate with
        member this.Usage = "List all assemblies in the GAC"

//-----------------------------------------------------------------
// Move commands
//-----------------------------------------------------------------

type MoveCommands =
    | [<First>][<CLIArg "file">] File

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | File -> "Move file in project"


type MoveFileArgs =
    | [<First>][<CLIAlt "-p">] Project of string
    | [<CLIAlt "-n">] Name of string
    | [<CLIArg "--up">] [<CLIAlt "-u">] Up
    | [<CLIArg "--down">] [<CLIAlt "-d">] Down

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Name _-> "File name"
            | Project _ -> "Project name"
            | Up -> "Moves file up"
            | Down -> "Moves file down"


//-----------------------------------------------------------------
// Update commands
//-----------------------------------------------------------------


type UpdateArgs =
    |[<CLIArg "paket">] Paket
    |[<CLIArg "fake">] Fake

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Paket -> "Updates Paket to latest version"
            | Fake _ -> "Updates FAKE to latest version"


//-----------------------------------------------------------------
// Command Handlers
//-----------------------------------------------------------------
let printHelp<'T when 'T :> IArgParserTemplate> () =
    let parser = ArgumentParser.Create<'T>(programName = "forge.exe", errorHandler = ProcessExiter())
    parser.PrintUsage "   Available parameters:" |> System.Console.WriteLine


let parseCommand<'T when 'T :> IArgParserTemplate> args =
    let parser = ArgumentParser.Create<'T>(programName = "forge.exe", errorHandler = ProcessExiter())
    let results =
        parser.Parse
            (inputs = args,
             ignoreUnrecognized = true,
             raiseOnUsage = false,
             ignoreMissing = true)
    if results.IsUsageRequested then
        match results.GetAllResults() with
        | [] ->
            parser.PrintUsage "   Available parameters:" |> System.Console.WriteLine
            None
        | [hd] ->
            (getUsage>>traceWarning) hd
            Some results
        | ls ->
            ls  |> List.iter (getUsage>>traceWarning)
            Some results
    else
    Some results


let execCommand fn args =
    args |> parseCommand |> Option.bind fn

let execCommand' fn args =
    args |> parseCommand |> Option.map fn

let subCommandArgs args =
    args |> parseCommand<_>
    |> Option.bind (fun res ->
        match res.GetAllResults() |> List.tryHead |> Option.map (fun x -> x, args.[1..]) with
        | None ->
            res.Raise (error = exn "Bad or missing parameters.", showUsage = true)
            None
        | x -> x
    )


//-----------------------------------------------------------------
// New Command Handlers
//-----------------------------------------------------------------


let newProject (results : ParseResults<_>) =
    let projectName = results.TryGetResult <@ NewProjectArgs.Name @>
    let projectDir  = results.TryGetResult <@ NewProjectArgs.Folder @>
    let templateName = results.TryGetResult <@ NewProjectArgs.Template @>
    let paket = not ^ results.Contains <@ NewProjectArgs.No_Paket @>
    let fake = not ^ results.Contains <@ NewProjectArgs.No_Fake @>
    let vscode = results.Contains <@ NewProjectArgs.VSCode @>

    Templates.Project.New projectName projectDir templateName paket fake vscode


let newFile (results : ParseResults<_>) =
    let fn = results.GetResult <@ NewFileArgs.Name @>
    let template = results.TryGetResult <@ NewFileArgs.Template @>
    let ba = results.TryGetResult <@ NewFileArgs.BuildAction @> |> Option.bind BuildAction.TryParse
    let copy = results.TryGetResult <@ NewFileArgs.CopyToOutputDirectory @> |> Option.bind CopyToOutputDirectory.TryParse
    let project = results.TryGetResult <@ NewFileArgs.Project @>
    Templates.File.New fn template project ba copy


let newSolution (results : ParseResults<_>) =
    let name = results.GetResult <@ NewSolutionArgs.Name @>
    Templates.Solution.New name

let newScaffold (results : ParseResults<NewScaffoldArgs>) =
    Templates.Scaffold.New ()

let processNew args =
    match subCommandArgs args with
    | Some (cmd, subArgs) ->
        match cmd with
        | NewCommand.Project  -> execCommand' newProject subArgs
        | NewCommand.File     -> execCommand' newFile subArgs
        | NewCommand.Solution -> execCommand' newSolution subArgs
        | NewCommand.Scaffold -> execCommand' newScaffold subArgs
    | _ ->
        traceWarning "Unknown command"
        None


//-----------------------------------------------------------------
// Add Command Handlers
//-----------------------------------------------------------------

let addFile (results : ParseResults<AddFileArgs>) =
    maybe {
        let! name = results.TryGetResult <@ AddFileArgs.Name @>
        let project = results.TryGetResult <@ AddFileArgs.Project @> //TODO this can't stay like this, adding to projects and solutions need to be mutally exclusive
        let solution = results.TryGetResult <@ AddFileArgs.Solution @> //TODO
        let build = results.TryGetResult <@ AddFileArgs.BuildAction @> |> Option.bind BuildAction.TryParse
        let copy = results.TryGetResult <@ AddFileArgs.CopyToOutputDirectory @> |> Option.bind CopyToOutputDirectory.TryParse
        let link = results.TryGetResult <@ AddFileArgs.Link @> |> Option.map (fun _ -> name)
        let below = results.TryGetResult <@ AddFileArgs.Below @>
        let above = results.TryGetResult <@ AddFileArgs.Above @>
        let project' =
            match project with
            | Some p -> Some p
            | None -> Furnace.tryFindProject name
        match project' with
        | None -> traceWarning "Project not found"
        | Some project ->
            let activeState = Furnace.loadFsProject project
            let n =
                if Path.IsPathRooted name then
                    relative name (activeState.ProjectPath + Path.DirectorySeparatorChar.ToString())
                else
                    relative (Path.GetFullPath name) (activeState.ProjectPath + Path.DirectorySeparatorChar.ToString())
            let name' = n |> Path.GetFileName
            let dir = n |> Path.GetDirectoryName
            match below, above with
            | Some b, _ ->
                activeState
                |> Furnace.addBelow (b, name', build, link, copy, None)
                |> ignore
            | None, Some a ->
                activeState
                |> Furnace.addAbove (a, name', build, link, copy, None)
                |> ignore
            | None, None ->
                activeState
                |> Furnace.addSourceFile (n, Some dir , build, link, copy, None)
                |> ignore
    }


let addReference (results : ParseResults<AddReferenceArgs>) =
    maybe {
        let! name = results.TryGetResult <@ AddReferenceArgs.Name @>
        let! project = results.TryGetResult <@ AddReferenceArgs.Project @>
        Furnace.loadFsProject project
        |> Furnace.addReference (name, None, None, None, None, None)
        |> ignore
    }

let addProject (results : ParseResults<AddProjectArgs>) =
    maybe {
        let! path = results.TryGetResult <@ AddProjectArgs.Name @>
        let! project = results.TryGetResult <@ AddProjectArgs.Project @>
        let name = Path.GetFileName path
        Furnace.loadFsProject project
        |> Furnace.addProjectReference(path, Some name, None, None, None)
        |> ignore
    }


let processAdd args =
    match subCommandArgs args  with
    | Some (cmd, subArgs ) ->
        match cmd with
        | AddCommands.Reference -> execCommand addReference subArgs
        | AddCommands.File -> execCommand addFile subArgs
        | AddCommands.Project -> execCommand addProject subArgs
    | _ ->
        traceWarning "Unknown command"
        None


//-----------------------------------------------------------------
// Remove Command Handlers
//-----------------------------------------------------------------

let removeFile (results : ParseResults<RemoveFileArgs>) =
    maybe {
        let! name = results.TryGetResult <@ RemoveFileArgs.Name @>
        let project = results.TryGetResult <@ RemoveFileArgs.Project @>
        let project' =
            match project with
            | Some p -> Some p
            | None -> Furnace.tryFindProject name
        match project' with
        | None -> traceWarning "Project not found"
        | Some project ->
            let name' = relative (getCwd() </> name) ((getCwd() </> project |> Path.GetDirectoryName) + Path.DirectorySeparatorChar.ToString()  )
            Furnace.loadFsProject project
            |> Furnace.removeSourceFile name'
            |> ignore
    }


let removeReference (results : ParseResults<RemoveReferenceArgs>) =
    maybe {
        let! name = results.TryGetResult <@ RemoveReferenceArgs.Name @>
        let! project = results.TryGetResult <@ RemoveReferenceArgs.Project @>
        Furnace.loadFsProject project
        |> Furnace.removeReference name
        |> ignore
    }

let removeFolder (results: ParseResults<RemoveFolderArgs>) =
    maybe {
        let! name = results.TryGetResult <@ RemoveFolderArgs.Name @>
        let project = results.TryGetResult <@ RemoveFolderArgs.Project @>
        let project' =
            match project with
            | Some p -> Some p
            | None -> Furnace.tryFindProject name
        match project' with
        | None -> traceWarning "Project not found"
        | Some project ->
            Furnace.loadFsProject project
            |> Furnace.removeDirectory name
            |> ignore
    }

let removeProject (results : ParseResults<RemoveProjectReferenceArgs>) =
    maybe {
        let! name = results.TryGetResult <@ RemoveProjectReferenceArgs.Name @>
        let! project = results.TryGetResult <@ RemoveProjectReferenceArgs.Project @>

        Furnace.loadFsProject project
        |> Furnace.removeProjectReference name
        |> ignore
    }


let processRemove args =
    match subCommandArgs args with
    | Some (cmd, subArgs) ->
        match cmd with
        | RemoveCommands.Reference -> execCommand removeReference subArgs
        | RemoveCommands.File -> execCommand removeFile subArgs // TODO - change to reflect mutual exclusion of removing solution files and project files
        | RemoveCommands.Folder -> execCommand removeFolder subArgs
        | RemoveCommands.Project -> execCommand removeProject subArgs
    | _ ->
        traceWarning "Unknown command"
        None


//-----------------------------------------------------------------
// Rename Command Handlers
//-----------------------------------------------------------------

let renameFile (results : ParseResults<RenameFileArgs>) =
    maybe {
        let! name = results.TryGetResult <@ RenameFileArgs.Name @>
        let! newName = results.TryGetResult <@ RenameFileArgs.Rename @>
        let project = results.TryGetResult <@ RenameFileArgs.Project @>
        let project' =
            match project with
            | Some p -> Some p
            | None -> Furnace.tryFindProject name
        match project' with
        | None -> traceWarning "Project not found"
        | Some project ->
            Furnace.loadFsProject project
            |> Furnace.renameSourceFile (name, newName)
            |> ignore
    }


let renameProject (results : ParseResults<RenameProjectArgs>) =
    maybe {
        let! name = results.TryGetResult <@ RenameProjectArgs.Name @>
        let! newName = results.TryGetResult <@ RenameProjectArgs.Rename @>

        return ()
    }


let renameFolder (results : ParseResults<RenameFolderArgs>) =
    maybe {
        let! name = results.TryGetResult <@ RenameFolderArgs.Name @>
        let! newName = results.TryGetResult <@ RenameFolderArgs.Rename @>
        let project = results.TryGetResult <@ RenameFolderArgs.Project @>
        let project' =
            match project with
            | Some p -> Some p
            | None -> Furnace.tryFindProject name
        match project' with
        | None -> traceWarning "Project not found"
        | Some project ->
            Furnace.loadFsProject project
            |> Furnace.renameDirectory (name, newName)
            |> ignore
    }


let processRename args =
    match subCommandArgs args with
    | Some (cmd, subArgs) ->
        match cmd with
        | RenameCommands.Project -> execCommand renameProject subArgs
        | RenameCommands.File    -> execCommand renameFile subArgs
        | RenameCommands.Folder  -> execCommand renameFolder subArgs
    | _ ->
        traceWarning "Unknown command"
        None

//-----------------------------------------------------------------
// List Command Handlers
//-----------------------------------------------------------------


let listFiles (results : ParseResults<ListFilesArgs>) =
    maybe {
        let! proj = results.TryGetResult <@ ListFilesArgs.Project @>
        let filter = results.TryGetResult <@ ListFilesArgs.Filter @>
        Furnace.loadFsProject proj
        |> (Furnace.listSourceFiles filter)
        |> ignore
    }


let listReferences (results : ParseResults<ListReferencesArgs>) =
    maybe {
        let! proj = results.TryGetResult <@ ListReferencesArgs.Project @>
        let filter = results.TryGetResult <@ ListReferencesArgs.Filter @>
        Furnace.loadFsProject proj
        |> (Furnace.listReferences filter)
        |> ignore
    }

let listProjectReferences (results : ParseResults<ListProjectReferencesArgs>) =
    maybe {
        let! proj = results.TryGetResult <@ ListProjectReferencesArgs.Project @>
        let filter = results.TryGetResult <@ ListProjectReferencesArgs.Filter @>
        Furnace.loadFsProject proj
        |> (Furnace.listProjectReferences filter)
        |> ignore
    }


let listProject (results : ParseResults<ListProjectsArgs>) =
    maybe {
        let! solution = results.TryGetResult <@ ListProjectsArgs.Solution @>
        let filter = results.TryGetResult <@ ListProjectsArgs.Filter @>
        traceWarning "not implemented yet" //TODO
    }

let listGac (results : ParseResults<ListGacArgs>) =
    maybe {
        GacSearch.searchGac ()
        |> Seq.iter(fun a -> trace a.FullName)
        |> ignore
    }

let listTemplates () =
    Forge.Templates.GetList()
    |> Seq.iter trace

let processList args =
    match subCommandArgs args with
    | Some (cmd, subArgs) ->
        match cmd with
        | ListCommands.Project   -> execCommand listProject subArgs
        | ListCommands.File      -> execCommand listFiles subArgs
        | ListCommands.Reference -> execCommand listReferences subArgs
        | ListCommands.GAC       -> execCommand listGac subArgs
        | ListCommands.ProjectReferences -> execCommand listProjectReferences subArgs
        | ListCommands.Templates -> listTemplates(); Some ()
        | ListCommands.Filter _ -> Some ()
    | _ ->
        traceWarning "Unknown command"
        None

//-----------------------------------------------------------------
// Move Command Handlers
//-----------------------------------------------------------------

let moveFile (results : ParseResults<_>) =
    maybe {
        let proj = results.TryGetResult <@ MoveFileArgs.Project @>
        let! name = results.TryGetResult <@ MoveFileArgs.Name @>
        let up = results.TryGetResult <@ MoveFileArgs.Up @>
        let down = results.TryGetResult <@ MoveFileArgs.Down @>
        let project' =
            match proj with
            | Some p -> Some p
            | None -> Furnace.tryFindProject name
        match project' with
        | None -> traceWarning "Project not found"
        | Some project ->
            let activeState = Furnace.loadFsProject project
            let n =
                if Path.IsPathRooted name then
                    relative name (activeState.ProjectPath + Path.DirectorySeparatorChar.ToString())
                else
                    relative (Path.GetFullPath name) (activeState.ProjectPath + Path.DirectorySeparatorChar.ToString())

            match up, down with
            | Some u, _ ->
                activeState |> Furnace.moveUp n |> ignore
            | None, Some d ->
                activeState |> Furnace.moveDown n |> ignore
            | None, None ->
                traceWarning "Up or Down must be specified"
    }

let processMove args =
    match subCommandArgs args  with
    | Some (cmd, subArgs ) ->
        match cmd with
        | MoveCommands.File -> execCommand moveFile subArgs
    | _ ->
        traceWarning "Unknown command"
        None




//-----------------------------------------------------------------
// Update Command Handlers
//-----------------------------------------------------------------

let processUpdate args =
    args |> execCommand' (fun results ->
        if results.Contains <@ UpdateArgs.Paket @> then Paket.Update()
        if results.Contains <@ UpdateArgs.Fake @>  then Fake.Update ()

    )


//-----------------------------------------------------------------
// Main Command Handlers
//-----------------------------------------------------------------

let applyAlias (args : string []) =
    let alias = Alias.load () |> Map.tryFind args.[0]
    match alias with
    | None -> args
    | Some a ->
        [| yield! a.Split(' '); yield! args.[1 ..] |]



let runForge (args : string []) =
    if args.Length = 0 then
        printHelp<Command> ()
        -1
    else
        let args = applyAlias args
        let result = parseCommand<Command> [| args.[0] |]
        let check (res:ParseResults<_>) =
            match res.GetAllResults() with
            | [cmd] ->
                try
                    let subArgs = args.[1 ..]
                    subArgs |>
                    match cmd with
                    | Command.New -> processNew
                    | Add -> processAdd
                    | Remove -> processRemove
                    | Command.Rename -> processRename
                    | List -> processList
                    | Move -> processMove
                    | Update -> processUpdate
                    | Command.Fake -> fun a -> Fake.Run a; Some ()
                    | Command.Paket -> fun a -> Paket.Run a; Some ()
                    | Refresh -> fun _ -> Templates.Refresh (); Some ()
                    | Version -> fun _ ->
                        printfn "%s" AssemblyVersionInformation.AssemblyVersion
                        Some ()
                with
                | ex ->
                    eprintfn "Unhandled error:\n%s" ex.Message
                    None
            | _ ->
                traceWarning "Unknown command"
                None
        match result |> Option.bind check with
        | None -> -1
        | Some _ -> 0


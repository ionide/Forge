module Forge.Commands

open System
open System.Text
open Argu 
open Forge
open Forge.ProjectSystem 
open Forge.ProjectManager

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
    | [<First>][<CLIArg "new">] New
    | [<First>][<CLIArg "add">] Add
    | [<First>][<CLIArg "move">] Move
    | [<First>][<CLIArg "remove">] Remove
    | [<First>][<CLIArg "rename">] Rename
    | [<First>][<CLIArg "list">] List
    | [<First>][<CLIArg "update">] Update
    | [<First>][<CLIArg "paket">] Paket
    | [<First>][<CLIArg "fake">] Fake
    | [<First>][<CLIArg "refresh">] Refresh
    | [<First>][<CLIArg "exit">][<CLIAlt("quit","-q")>] Exit

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | New -> "<project|file> Create new file or project"
            | Add -> "<file|reference> Adds file or reference"
            | Move -> "<file|folder> Move the file or folder within the project hierarchy"
            | Remove -> "<file|reference> Removes file or refrence"
            | Rename -> "<project|file> Renames file or project"
            | List -> "<project|file|reference|templates|gac> List files or refrences"
            | Update -> "<paket|fake> Updates Paket or FAKE"
            | Paket -> "Runs Paket"
            | Fake -> "Runs FAKE"
            | Refresh -> "Refreshes the template cache"
            | Exit -> "Exits interactive mode"



//-----------------------------------------------------------------
// New Commands
//-----------------------------------------------------------------

type NewCommand =
    | [<First>][<CLIArg "project">] Project
    | [<First>][<CLIArg "file">] File

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Project -> "Creates new project"
            | File -> "Creates new file"




type NewProjectArgs =
    | [<CLIAlt "-n">] Name of string
    | [<CLIAlt "dir">] Folder of string
    | [<CLIAlt "-t">] Template of string
    | No_Paket
    | No_Fake

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Name _ -> "Project name"
            | Folder _ -> "Project folder, relative to Forge working folder"
            | Template _ -> "Template name"
            | No_Paket -> "Don't use Paket for dependency managment"
            | No_Fake -> "Don't use FAKE for build"



type NewFileArgs =
    | [<CLIAlt "-n">] Name of string
    | [<CLIAlt "-t">] Template of string
    | [<CLIAlt "-p">] Project of string
    | [<CLIAlt "-s">] Solution of string
    | [<CLIArg "--build-action">] [<CLIAlt "-b">] BuildAction of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Name _-> "File name"
            | Template _ -> "File template"
            | Project _ -> "Project to which file will be added"
            | Solution _ -> "Solution to which file will be added"
            | BuildAction _ -> "File build action"




//-----------------------------------------------------------------
// Add commands
//-----------------------------------------------------------------

type AddCommands =
    | [<First>][<CLIArg "file">] File
    | [<First>][<CLIArg "reference">] Reference

    interface IArgParserTemplate with
        member self.Usage = self |> function
            | File -> "Adds file to project or solution"
            | Reference -> "Adds reference to project"





type AddFileArgs =
    | [<First>][<CLIAlt "-p">] Project of string
    | [<First>][<CLIAlt "-s">] Solution of string
    | [<CLIAlt "-n">] Name of string
    | [<CLIAlt "dir">] Folder of string
    | [<CLIArg "--build-action">] [<CLIAlt "-ba">] BuildAction of string
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


//-----------------------------------------------------------------
// Remove commands
//-----------------------------------------------------------------


type RemoveCommands =
    | [<First>][<CLIArg "file">] File
    | [<First>][<CLIArg "reference">] Reference
    | [<First>][<CLIArg "folder">][<CLIAlt "dir">] Folder

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | File -> "Removes file from project or solution"
            | Reference -> "Removes reference from project"
            | Folder -> "Removes the folder from the project or solution"




type RemoveFileArgs =
    | [<First>][<CLIAlt "-p">] Project of string
    | [<First>][<CLIAlt "-s">] Solution of string
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


type RemoveFolderArgs =
    | [<First>][<CLIAlt "-p">] Project of string
    | [<First>][<CLIAlt "-s">] Solution of string
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
    | [<First>][<CLIArg "folder">][<CLIAlt "dir">] Folder

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | File -> "Reneames file"
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
            | Project _ -> "Project Containing File"


type RenameFolderArgs =
    | [<CLIAlt "-n">] Name of string
    | [<CLIAlt "-r">] Rename of string
    | [<CLIAlt "-p">] Project of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Name _-> "Folder name"
            | Rename _ -> "New name"
            | Project _ -> "Project containg folder"


type RenameProjectArgs =
    | [<CLIAlt "-n">] Name of string
    | [<CLIAlt "-r">] Rename of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Name _-> "Project name"
            | Rename _ -> "New name"


//-----------------------------------------------------------------
// List commands
//-----------------------------------------------------------------


type ListCommands =
    | [<First>][<CLIArg "files">] File
    | [<First>][<CLIArg "references">] Reference
    | [<First>][<CLIArg "projects">] Project
    | [<First>][<CLIArg "gac">] GAC
    | [<First>][<CLIArg "templates">] Templates
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | File -> "List file from project"
            | Reference -> "List reference from project"
            | Project -> "List projects in solution"
            | Templates -> "List the templates in Forge's cache"
            | GAC -> "List the assembilies in the Global Assembly Cache"


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

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Project _ -> "List the files in this project"
            | Solution _ -> "List the files in solution folders"


type ListReferencesArgs =
    | [<CLIAlt "-p">] Project of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Project _ -> "List the refrences in this project"


type ListProjectsArgs =
    | [<CLIAlt "-s">] Solution of string
    | [<CLIAlt ("dir")>] Folder of string
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Solution _ -> "List the projects in this solution"
            | Folder _ -> "List the projects in this directory"


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


let parseCommand<'T when 'T :> IArgParserTemplate> args =
    let parser = ArgumentParser.Create<'T>()
    let results =
        parser.Parse
            (inputs = args,
             ignoreUnrecognized = true,
             raiseOnUsage = false,
             ignoreMissing = true,
             errorHandler = ProcessExiter())
    if results.IsUsageRequested then
        match results.GetAllResults() with
        | [] ->
            parser.Usage "   Available parameters:" |> System.Console.WriteLine
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


let subCommandArgs args =
    args |> parseCommand<_>
    |> Option.map (fun res ->
        res.GetAllResults().Head, args.[1..]
    )


//-----------------------------------------------------------------
// New Command Handlers
//-----------------------------------------------------------------


let newProject cont (results : ParseResults<_>) =
    let projectName = results.TryGetResult <@ NewProjectArgs.Name @>
    let projectDir  = results.TryGetResult <@ NewProjectArgs.Folder @>
    let templateName = results.TryGetResult <@ NewProjectArgs.Template @>
    let paket = not ^ results.Contains <@ NewProjectArgs.No_Paket @>
    let fake = not ^ results.Contains <@ NewProjectArgs.No_Fake @>
    Templates.Project.New projectName projectDir templateName paket fake
    Some cont 


let newFile cont (results : ParseResults<_>) =
    let fn = results.GetResult <@ NewFileArgs.Name @>
    let template = results.TryGetResult <@ NewFileArgs.Template @>
    let ba = results.TryGetResult <@ NewFileArgs.BuildAction @> |> Option.bind BuildAction.TryParse
    let project = results.TryGetResult <@ NewFileArgs.Project @>
    Templates.File.New fn template project ba
    
    Some cont


let processNew cont args =
    match subCommandArgs args with
    | Some (cmd, subArgs) ->
        match cmd with
        | NewCommand.Project -> execCommand (newProject cont) subArgs
        | NewCommand.File    -> execCommand (newFile cont) subArgs
    | _ -> Some cont


//-----------------------------------------------------------------
// Add Command Handlers
//-----------------------------------------------------------------

let addFile cont (results : ParseResults<AddFileArgs>) =
    maybe {
        let! name = results.TryGetResult <@ AddFileArgs.Name @>
        let! project = results.TryGetResult <@ AddFileArgs.Project @> //TODO this can't stay like this, adding to projects and solutions need to be mutally exclusive
        let solution = results.TryGetResult <@ AddFileArgs.Solution @> //TODO
        let build = results.TryGetResult <@ AddFileArgs.BuildAction @> |> Option.bind BuildAction.TryParse
        let link = results.TryGetResult <@ AddFileArgs.Link @> |> Option.map (fun _ -> name)
        let below = results.TryGetResult <@ AddFileArgs.Below @>
        let above = results.TryGetResult <@ AddFileArgs.Above @>
        let activeState = Furnace.loadFsProject project

        match below, above with
        | Some b, _ ->
            activeState
            |> Furnace.addBelow (b, name, build, link, None, None)
            |> ignore
        | None, Some a ->
            activeState
            |> Furnace.addAbove (a, name, build, link, None, None)
            |> ignore
        | None, None ->
            activeState
            |> Furnace.addSourceFile (name, Some activeState.ProjectPath, build, link, None, None)
            |> ignore

        return cont
    }


let addReference cont (results : ParseResults<AddReferenceArgs>) =
    maybe {
        let! name = results.TryGetResult <@ AddReferenceArgs.Name @>
        let! project = results.TryGetResult <@ AddReferenceArgs.Project @>
        Furnace.loadFsProject project
        |> Furnace.addReference (name, None, None, None, None, None)
        |> ignore
        return cont
    }


let processAdd cont args =
    match subCommandArgs args  with
    | Some (cmd, subArgs ) ->
        match cmd with
        | AddCommands.Reference -> execCommand (addReference cont) subArgs
        | AddCommands.File -> execCommand (addFile cont) subArgs
    | _ -> Some cont


//-----------------------------------------------------------------
// Remove Command Handlers
//-----------------------------------------------------------------

let removeFile cont (results : ParseResults<RemoveFileArgs>) =
    maybe {
        let! name = results.TryGetResult <@ RemoveFileArgs.Name @>
        let! project = results.TryGetResult <@ RemoveFileArgs.Project @>
        Furnace.loadFsProject project
        |> Furnace.removeSourceFile name
        |> ignore
        return cont
    }


let removeReference cont (results : ParseResults<RemoveReferenceArgs>) =
    maybe {
        let! name = results.TryGetResult <@ RemoveReferenceArgs.Name @>
        let! project = results.TryGetResult <@ RemoveReferenceArgs.Project @>
        Furnace.loadFsProject project
        |> Furnace.removeReference name
        |> ignore
        return cont
    }

let removeFolder cont (results: ParseResults<RemoveFolderArgs>) =
    maybe {
        let! name = results.TryGetResult <@ RemoveFolderArgs.Name @>
        let! project = results.TryGetResult <@ RemoveFolderArgs.Project @>
        Furnace.loadFsProject project
        |> Furnace.removeDirectory name
        |> ignore
        return cont
    }


let processRemove cont args =
    match subCommandArgs args with
    | Some (cmd, subArgs) ->
        match cmd with
        | RemoveCommands.Reference -> execCommand (removeReference cont) subArgs
        | RemoveCommands.File -> execCommand (removeFile cont) subArgs // TODO - change to reflect mutual exclusion of removing solution files and project files
        | RemoveCommands.Folder -> execCommand (removeFolder cont) subArgs
    | _ -> Some cont


//-----------------------------------------------------------------
// Rename Command Handlers
//-----------------------------------------------------------------

let renameFile cont (results : ParseResults<RenameFileArgs>) =
    maybe {
        let! name = results.TryGetResult <@ RenameFileArgs.Name @>
        let! newName = results.TryGetResult <@ RenameFileArgs.Rename @>
        let! project = results.TryGetResult <@ RenameFileArgs.Project @>

        Furnace.loadFsProject project
        |> Furnace.renameSourceFile (name, newName)
        |> ignore
        return cont
    }


let renameProject cont (results : ParseResults<_>) =
    traceWarning "not implemented yet"
    Some cont


let renameFolder cont (results : ParseResults<RenameFolderArgs>) =
    maybe {
        let! name = results.TryGetResult <@ RenameFolderArgs.Name @>
        let! newName = results.TryGetResult <@ RenameFolderArgs.Rename @>
        let! project = results.TryGetResult <@ RenameFolderArgs.Project @>

        Furnace.loadFsProject project
        |> Furnace.renameDirectory (name, newName)
        |> ignore

        return cont
    }


let processRename cont args =
    match subCommandArgs args with
    | Some (cmd, subArgs) ->
        match cmd with
        | RenameCommands.Project -> execCommand (renameProject cont) subArgs
        | RenameCommands.File    -> execCommand (renameFile cont) subArgs
        | RenameCommands.Folder  -> execCommand (renameFolder cont) subArgs
    | _ -> Some cont

//-----------------------------------------------------------------
// List Command Handlers
//-----------------------------------------------------------------

let listFiles cont (results : ParseResults<ListFilesArgs>) =
    maybe {
        let! proj = results.TryGetResult <@ ListFilesArgs.Project @>
        Furnace.loadFsProject proj
        |> Furnace.listSourceFiles
        |> ignore
        return cont
    }


let listReferences cont (results : ParseResults<ListReferencesArgs>) =
    maybe {
        let! proj = results.TryGetResult <@ ListReferencesArgs.Project @>
        Furnace.loadFsProject proj
        |> Furnace.listReferences
        |> ignore
        return cont
    }


let listProject cont (results : ParseResults<ListProjectsArgs>) =
    maybe {
        let! solution = results.TryGetResult <@ ListProjectsArgs.Solution @>
        traceWarning "not implemented yet" //TODO
        return cont
    }


let processList cont args =
    match subCommandArgs args with
    | Some (cmd, subArgs) ->
        match cmd with
        | ListCommands.Project   -> execCommand (listProject cont) subArgs
        | ListCommands.File      -> execCommand (listFiles cont) subArgs
        | ListCommands.Reference -> execCommand (listReferences cont) subArgs
        | ListCommands.GAC       -> traceWarning "not implemented yet"; Some cont
        | ListCommands.Templates -> traceWarning "not implemented yet"; Some cont
    | _ -> Some cont

//-----------------------------------------------------------------
// Move Command Handlers
//-----------------------------------------------------------------

let moveFile cont (results : ParseResults<_>) =
    maybe {
        let! proj = results.TryGetResult <@ MoveFileArgs.Project @>
        let! name = results.TryGetResult <@ MoveFileArgs.Name @>
        let up = results.TryGetResult <@ MoveFileArgs.Up @>
        let down = results.TryGetResult <@ MoveFileArgs.Down @>
        let activeState = Furnace.loadFsProject proj

        match up, down with
        | Some u, _ ->
            activeState |> Furnace.moveUp name |> ignore
        | None, Some d ->
            activeState |> Furnace.moveDown name |> ignore
        | None, None ->
            traceWarning "Up or Down must be specified"

        return cont
    }

let processMove cont args =
    match subCommandArgs args  with
    | Some (cmd, subArgs ) ->
        match cmd with
        | MoveCommands.File -> execCommand (moveFile cont) subArgs
    | _ -> Some cont




//-----------------------------------------------------------------
// Update Command Handlers
//-----------------------------------------------------------------

let processUpdate cont args =
    args |> execCommand (fun results ->
        if results.Contains <@ UpdateArgs.Paket @> then Paket.Update()
        if results.Contains <@ UpdateArgs.Fake @>  then Fake.Update ()
        Some cont
    )


//-----------------------------------------------------------------
// Main Command Handlers
//-----------------------------------------------------------------

let strikeForge args (cont:Result) =
    let result = parseCommand<Command> args
    let check (res:ParseResults<_>) =
        match res.GetAllResults() with
        | [cmd] ->
            try
            let subArgs = args.[1 ..]
            subArgs |>
            match cmd with
            | Command.New -> processNew cont
            | Add -> processAdd cont
            | Remove -> processRemove cont
            | Command.Rename -> processRename cont
            | List -> processList cont
            | Move -> processMove cont
            | Update -> processUpdate cont
            | Command.Fake -> fun a -> Fake.Run a; Some cont
            | Command.Paket -> fun a -> Paket.Run a; Some cont
            | Refresh -> fun _ -> Templates.Refresh (); Some cont
            | Exit -> fun _ -> Some Result.Exit
            with
            | _ ->
                printfn "Unrecognized command or missing required parameter\n"
                Some cont
        | _ -> Some cont
    defaultArg (Option.bind check result) cont


let interactive args =
    strikeForge args Continue


let singlePass args =
    strikeForge args Result.Exit

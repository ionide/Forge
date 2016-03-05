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
| Continue //of ActiveState
| Help
| Exit


//-----------------------------------------------------------------
// Main commands
//-----------------------------------------------------------------

(*  TODO Add commands
    - delete file|dir    

*)

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
            | Remove -> "<file|reference> Removes file or refrence" // TODO add dirs too
            | Rename -> "<project|file> Renames file or project"
            | List -> "<project|file|reference> List files or refrences"
            | Update -> "<paket|fake> Updates Paket or FAKE"
            | Paket -> "Runs Paket"
            | Fake -> "Runs FAKE"
            | Refresh -> "Refreshes the template cache"
            | Exit -> "Exits interactive mode"
    member this.Name =
        let uci,_ = Microsoft.FSharp.Reflection.FSharpValue.GetUnionFields(this, typeof<Command>)
        (uci.GetCustomAttributes(typeof<CustomCommandLineAttribute>)
        |> Seq.head
        :?> CustomCommandLineAttribute).Name

//-----------------------------------------------------------------
// New Commands
//-----------------------------------------------------------------

type NewCommand =
    | [<First>][<CLIArg "project">] Project
    | [<First>][<CLIArg "file">] File
with
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
with
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
with
    interface IArgParserTemplate with
        member this.Usage =
            match this with
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
with
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
    | [<First>][<CLIArg "file">] File
    | [<First>][<CLIArg "reference">] Reference
    | [<First>][<CLIArg "project">] Project

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | File -> "List file from project"
            | Reference -> "List reference from project"
            | Project -> "List projects in solution"


type ListFileArgs =
    | [<CLIAlt "-p">] Project of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Project _ -> "List the files in this project"


type ListReferenceArgs =
    | [<CLIAlt "-p">] Project of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Project _ -> "List the refrences in this project"


type ListProjectArgs =
    | [<CLIAlt "-s">] Solution of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Solution _ -> "List the files in this solution"

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
        parser.Usage "Available parameters:" |> System.Console.WriteLine
        None
    else
        Some results

let execCommand fn args = args |> parseCommand |> Option.bind fn 

let subCommandArgs args =
    args |> parseCommand<_>
    |> Option.map (fun res ->
        res.GetAllResults().Head, args.[1..]
    )


let inline defaultResult (cmdarg:'field->#IArgParserTemplate) value (results : ParseResults<_>) =    
    defaultArg (results.TryGetResult <@cmdarg@>) value


//-----------------------------------------------------------------
// New Command Handlers
//-----------------------------------------------------------------


let newProject (results : ParseResults<_>) =
    let projectName = defaultResult NewProjectArgs.Name "" results
    let projectDir  = defaultResult NewProjectArgs.Folder  "" results
    let templateName = defaultResult NewProjectArgs.Template "" results
    let paket = not ^ results.Contains <@ NewProjectArgs.No_Paket @>
    let fake = not ^ results.Contains <@ NewProjectArgs.No_Fake @>
    Project.New projectName projectDir templateName paket fake
    Some Continue


let newFile (results : ParseResults<_>) =
    printfn "Not implemented yet"
    Some Continue


let processNew args =
    match subCommandArgs args with
    | Some (cmd, subArgs) ->
        match cmd with
        | NewCommand.Project -> execCommand newProject subArgs
        | NewCommand.File    -> execCommand newFile subArgs
    | _ -> Some Help


//-----------------------------------------------------------------
// Add Command Handlers
//-----------------------------------------------------------------

let addFile (results : ParseResults<AddFileArgs>) =
    maybe {
        let! name = results.TryGetResult <@ AddFileArgs.Name @>
        let! project = results.TryGetResult <@ AddFileArgs.Project @> //TODO this can't stay like this, adding to projects and solutions need to be mutally exclusive
        let solution = results.TryGetResult <@ AddFileArgs.Solution @> //TODO
        let build = defaultResult AddFileArgs.BuildAction "" results |> BuildAction.TryParse
        let link = results.TryGetResult <@ AddFileArgs.Link @> |> Option.map (fun _ -> name)
        let below = results.TryGetResult <@ AddFileArgs.Below @>
        let above = results.TryGetResult <@ AddFileArgs.Above @>
        let activeState = Furnace.init project
        
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
            
        return Continue
    }


let addReference (results : ParseResults<AddReferenceArgs>) =
    maybe {
        let! name = results.TryGetResult <@ AddReferenceArgs.Name @>
        let! project = results.TryGetResult <@ AddReferenceArgs.Project @>
        Furnace.init project
        |> Furnace.addReference (name, None, None, None, None, None)
        |> ignore
        return Continue
    }


let processAdd args =
    match subCommandArgs args  with
    | Some (cmd, subArgs ) ->
        match cmd with
        | AddCommands.Reference -> execCommand addReference subArgs
        | AddCommands.File -> execCommand addFile subArgs
    | _ -> Some Help


//-----------------------------------------------------------------
// Remove Command Handlers
//-----------------------------------------------------------------

let removeFile (results : ParseResults<RemoveFileArgs>) =
    maybe {
        let! name = results.TryGetResult <@ RemoveFileArgs.Name @>
        let! project = results.TryGetResult <@ RemoveFileArgs.Project @>
        Furnace.init project
        |> Furnace.removeSourceFile name
        |> ignore
        return Continue
    }

let removeReference (results : ParseResults<RemoveReferenceArgs>) =
    maybe {
        let! name = results.TryGetResult <@ RemoveReferenceArgs.Name @>
        let! project = results.TryGetResult <@ RemoveReferenceArgs.Project @> 
        Furnace.init project
        |> Furnace.removeReference name 
        |> ignore
        return Continue
    }     
    
let removeFolder (results: ParseResults<RemoveFolderArgs>) = 
    maybe {
        let! name = results.TryGetResult <@ RemoveFolderArgs.Name @>
        let! project = results.TryGetResult <@ RemoveFolderArgs.Project @> 
        Furnace.init project
        |> Furnace.removeDirectory name 
        |> ignore       
        
        return Continue
    }

let processRemove args =
    match subCommandArgs args with
    | Some (cmd, subArgs) ->
        match cmd with
        | RemoveCommands.Reference -> execCommand removeReference subArgs
        | RemoveCommands.File -> execCommand removeFile subArgs // TODO - change to reflect mutual exclusion of removing solution files and project files
        | RemoveCommands.Folder -> execCommand removeFolder subArgs
    | _ -> Some Help


//-----------------------------------------------------------------
// Rename Command Handlers
//-----------------------------------------------------------------

let renameFile (results : ParseResults<RenameFileArgs>) =
    maybe {
        let! name = results.TryGetResult <@ RenameFileArgs.Name @>
        let! newName = results.TryGetResult <@ RenameFileArgs.Rename @>
        let! project = results.TryGetResult <@ RenameFileArgs.Project @>

        Furnace.init project
        |> Furnace.renameSourceFile (name, newName)
        |> ignore
        return Continue
    }


let renameProject (results : ParseResults<_>) =
    traceWarning "not implemented yet"
    Some Continue
    
let renameFolder (results : ParseResults<RenameFolderArgs>) = 
    maybe {
        let! name = results.TryGetResult <@ RenameFolderArgs.Name @>
        let! newName = results.TryGetResult <@ RenameFolderArgs.Rename @>
        let! project = results.TryGetResult <@ RenameFolderArgs.Project @>
        
        Furnace.init project
        |> Furnace.renameDirectory (name, newName)
        |> ignore
        
        return Continue
    }


let processRename args =
    match subCommandArgs args with
    | Some (cmd, subArgs) ->
        match cmd with
        | RenameCommands.Project -> execCommand renameProject subArgs
        | RenameCommands.File    -> execCommand renameFile subArgs
        | RenameCommands.Folder  -> execCommand renameFolder subArgs
    | _ -> Some Help

//-----------------------------------------------------------------
// List Command Handlers
//-----------------------------------------------------------------

let listFile (results : ParseResults<ListFileArgs>) =
    maybe {
        let! proj = results.TryGetResult <@ ListFileArgs.Project @>
        let activeState = Furnace.init proj
        activeState.ProjectData.SourceFiles.AllFiles()
        |> Seq.iter (printfn "%s")
        return Continue
    }


let listReference (results : ParseResults<_>) =
    maybe {
        let! proj = results.TryGetResult <@ ListReferenceArgs.Project @>
        let activeState = Furnace.init proj
        activeState.ProjectData.References
        |> Seq.iter (fun n -> printfn "%s" n.Include)
        return Continue
    }


let listProject (results : ParseResults<_>) =
    maybe {
        let! solution = results.TryGetResult <@ ListProjectArgs.Solution @>
        traceWarning "not implemented yet"
        return Continue
    }


let processList args =
    match subCommandArgs args with
    | Some (cmd, subArgs) ->
        match cmd with
        | ListCommands.Project   -> execCommand listProject subArgs
        | ListCommands.File      -> execCommand listFile subArgs
        | ListCommands.Reference -> execCommand listReference subArgs
    | _ -> Some Help
    


//-----------------------------------------------------------------
// Update Command Handlers
//-----------------------------------------------------------------

let processUpdate args =
    args |> execCommand (fun results ->
        if results.Contains <@ UpdateArgs.Paket @> then Paket.Update()
        if results.Contains <@ UpdateArgs.Fake @>  then Fake.Update ()
        Some Continue
    )


//-----------------------------------------------------------------
// Main Command Handlers
//-----------------------------------------------------------------

let processMain args =
    let result = parseCommand<Command> args

    let check (res:ParseResults<_>) =
        match res.GetAllResults() with
        | [cmd] ->
            try
            let args' = args.[1 ..]
            args' |>
            match cmd with
            | Command.New -> processNew
            | Add -> processAdd
            | Remove -> processRemove
            | Command.Rename -> processRename
            | List -> processList
            | Update -> processUpdate
            | Command.Fake -> fun a -> Fake.Run a; Some Continue
            | Command.Paket -> fun a -> Paket.Run a; Some Continue
            | Refresh -> fun _ -> Templates.Refresh (); Some Continue
            | Exit -> fun _ -> Some Result.Exit
            with
            | _ ->
                printfn "Unrecognized command or missing required parameter"
                Some Help
        | _ -> Some Continue

//    defaultArg (Option.bind check result) Result.Exit
    defaultArg (Option.bind check result) Result.Continue

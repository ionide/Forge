module Forge.Commands

open System
open System.Text
open Argu

/// Custom Command Line Argument
type CLIArg = CustomCommandLineAttribute
/// Alternative Command Line Argument
type CLIAlt = AltCommandLineAttribute

type Result =
| Continue
| Help
| Exit

//-----------------------------------------------------------------
// Main commands
//-----------------------------------------------------------------

type Command =
    | [<First>][<CLIArg "new">] New
    | [<First>][<CLIArg "add">] Add
    | [<First>][<CLIArg "remove">] Remove
    | [<First>][<CLIArg "rename">] Rename
    | [<First>][<CLIArg "list">] List
    | [<First>][<CLIArg "update">] Update
    | [<First>][<CLIArg "paket">] Paket
    | [<First>][<CLIArg "fake">] Fake
    | [<First>][<CLIArg "refresh">] Refresh
    | [<First>][<CLIArg "exit">][<CLIAlt("quit","-q")>] Exit
with
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | New -> "[project|file] Create new file or project"
            | Add -> "[file|reference] Adds file or reference"
            | Remove -> "[file|reference] Removes file or refrence"
            | Rename -> "[project|file] Renames file or project"
            | List -> "[project|file|reference] List files or refrences"
            | Update -> "[paket|fake] Updates Paket or FAKE"
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
// New
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
    | [<CLIAlt "-d">] Dir of string
    | [<CLIAlt "-t">] Template of string
    | No_Paket
    | No_Fake
with
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Name _ -> "Project name"
            | Dir _ -> "Project directory, relative to Forge working directory"
            | Template _ -> "Template name"
            | No_Paket -> "Don't use Paket for dependency managment"
            | No_Fake -> "Don't use FAKE for build"

type NewFileArgs =
    | [<CLIAlt "-n">] Name of string
    | [<CLIAlt "-t">] Template of string
    | [<CLIAlt "-p">] Project of string
    | [<CLIAlt "-s">] Solution of string
    | [<CLIArg "--build-action">] [<CLIAlt "-b">] BuildAction of string
with
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
    | [<CLIAlt "-n">] Name of string
    | [<CLIAlt "-p">] Project of string
    | [<CLIAlt "-s">] Solution of string
    | [<CLIArg "--build-action">] [<CLIAlt "-b">] BuildAction of string
    | Link
with
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Name _-> "File name"
            | Project _ -> "Project to which file will be added"
            | Solution _ -> "Solution to which file will be added"
            | BuildAction _ -> "File build action"
            | Link -> "Adds as link"

type AddReferenceArgs =
    | [<CLIAlt "-n">] Name of string
    | [<CLIAlt "-p">] Project of string
with
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
with
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | File -> "Removes file from project or solution"
            | Reference -> "Removes reference from project"

type RemoveFileArgs =
    | [<CLIAlt "-n">] Name of string
    | [<CLIAlt "-p">] Project of string
    | [<CLIAlt "-s">] Solution of string
with
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Name _-> "File name"
            | Project _ -> "Project from which file will be removed"
            | Solution _ -> "Solution from which file will be removed"

type RemoveReferenceArgs =
    | [<CLIAlt "-n">] Name of string
    | [<CLIAlt "-p">] Project of string
with
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Name _-> "Reference name"
            | Project _ -> "Project from which reference will be removed"

//-----------------------------------------------------------------
// Rename
//-----------------------------------------------------------------

type RenameCommands =
    | [<First>][<CLIArg "file">] File
    | [<First>][<CLIArg "project">] Project
with
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | File -> "Reneames file"
            | Project -> "Renames project"

type RenameFileArgs =
    | [<CLIAlt "-n">] Name of string
    | [<CLIAlt "-N">] New of string
with
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Name _-> "File name"
            | New _ -> "New file name"

type RenameProjectArgs =
    | [<CLIAlt "-n">] Name of string
    | [<CLIAlt "-N">] New of string
with
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Name _-> "Project name"
            | New _ -> "New name"

//-----------------------------------------------------------------
// List commands
//-----------------------------------------------------------------

type ListCommands =
    | [<First>][<CLIArg "file">] File
    | [<First>][<CLIArg "reference">] Reference
    | [<First>][<CLIArg "project">] Project
with
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | File -> "List file from project"
            | Reference -> "List reference from project"
            | Project -> "List projects in solution"

type ListFileArgs =
    | [<CLIAlt "-p">] Project of string
with
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Project _ -> "Project from which file will be listed"


type ListReferenceArgs =
    | [<CLIAlt "-p">] Project of string
with
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Project _ -> "Project from which reference will be listed"

type ListProjectArgs =
    | [<CLIAlt "-s">] Solution of string
with
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Solution _ -> "Solution from which projects will be listed"

//-----------------------------------------------------------------
// Update commands
//-----------------------------------------------------------------


type UpdateArgs =
    |[<CLIArg "paket">] Paket
    |[<CLIArg "fake">] Fake
with
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Paket -> "Updates Paket to latest version"
            | Fake _ -> "Updates FAKE to latest version"

//-----------------------------------------------------------------
// Command Handlers
//-----------------------------------------------------------------


let processCommand<'T when 'T :> IArgParserTemplate> args =
    let parser = ArgumentParser.Create<'T>()
    let results =
        parser.Parse
            (inputs = args,
             ignoreUnrecognized = true,
             raiseOnUsage = false,
             ignoreMissing = true,
             errorHandler = ProcessExiter())
    if results.IsUsageRequested then
        parser.Usage("Available parameters:") |> System.Console.WriteLine
        None
    else
        Some results

//-----------------------------------------------------------------
// New Command Handlers
//-----------------------------------------------------------------


let newProject (results : ParseResults<_>) =
    let projectName = defaultArg (results.TryGetResult <@ NewProjectArgs.Name @>) ""
    let projectDir = defaultArg (results.TryGetResult <@ NewProjectArgs.Dir @>) ""
    let templateName = defaultArg (results.TryGetResult <@ NewProjectArgs.Template @>) ""
    let paket = not ^ results.Contains <@ NewProjectArgs.No_Paket @>
    let fake = not ^ results.Contains <@ NewProjectArgs.No_Fake @>
    Project.New projectName projectDir templateName paket fake
    Continue

let newFile (results : ParseResults<_>) =
    printfn "Not implemented yet"
    Continue

let processNew args =
    args
    |> processCommand<NewCommand>
    |> Option.bind (fun res ->
        let args' = args.[1 ..]
        match res.GetAllResults () with
        | [cmd] ->
            match cmd with
            | NewCommand.Project -> args' |> processCommand |> Option.map newProject
            | NewCommand.File -> args' |> processCommand |> Option.map newFile
        | _ -> Some Help
    )

//-----------------------------------------------------------------
// Add Command Handlers
//-----------------------------------------------------------------

let addFile (results : ParseResults<_>) =
    let name = results.TryGetResult <@ AddFileArgs.Name @>
    let project = results.TryGetResult <@ AddFileArgs.Project @> //TODO
    let solution = results.TryGetResult <@ AddFileArgs.Solution @> //TODO
    let build = results.TryGetResult <@ AddFileArgs.BuildAction @> //TODO
    let link = results.TryGetResult <@ AddFileArgs.Link @> //TODO

    match name with
    | Some n -> Files.Add n
    | None -> ()
    Continue

let addReference (results : ParseResults<_>) =
    let name = results.TryGetResult <@ AddReferenceArgs.Name @>
    let project = results.TryGetResult <@ AddReferenceArgs.Project @> //TODO
    match name with
    | Some n -> References.Add n
    | None -> ()
    Continue

let processAdd args =
    args
    |> processCommand<AddCommands>
    |> Option.bind (fun res ->
        let args' = args.[1 ..]
        match res.GetAllResults () with
        | [cmd] ->
            match cmd with
            | AddCommands.Reference -> args' |> processCommand |> Option.map addReference
            | AddCommands.File -> args' |> processCommand |> Option.map addFile
        | _ -> Some Help
    )

//-----------------------------------------------------------------
// Remove Command Handlers
//-----------------------------------------------------------------

let removeFile (results : ParseResults<_>) =
    let name = results.TryGetResult <@ RemoveFileArgs.Name @>
    let project = results.TryGetResult <@ RemoveFileArgs.Project @> //TODO
    let solution = results.TryGetResult <@ RemoveFileArgs.Solution @> //TODO

    match name with
    | Some n -> Files.Remove n
    | None -> ()
    Continue

let removeReference (results : ParseResults<_>) =
    let name = results.TryGetResult <@ AddReferenceArgs.Name @>
    let project = results.TryGetResult <@ AddReferenceArgs.Project @> //TODO
    match name with
    | Some n -> References.Remove n
    | None -> ()
    Continue

let processRemove args =
    args
    |> processCommand<RemoveCommands>
    |> Option.bind (fun res ->
        let args' = args.[1 ..]
        match res.GetAllResults () with
        | [cmd] ->
            match cmd with
            | RemoveCommands.Reference -> args' |> processCommand |> Option.map removeReference
            | RemoveCommands.File -> args' |> processCommand |> Option.map removeFile
        | _ -> Some Help
    )

//-----------------------------------------------------------------
// Rename Command Handlers
//-----------------------------------------------------------------

let renameFile (results : ParseResults<_>) =
    printfn "not implemented yet"
    Continue

let renameProject (results : ParseResults<_>) =
    printfn "not implemented yet"
    Continue

let processRename args =
    args
    |> processCommand<RenameCommands>
    |> Option.bind (fun res ->
        let args' = args.[1 ..]
        match res.GetAllResults () with
        | [cmd] ->
            match cmd with
            | RenameCommands.Project -> args' |> processCommand |> Option.map renameProject
            | RenameCommands.File -> args' |> processCommand |> Option.map renameFile
        | _ -> Some Help
    )

//-----------------------------------------------------------------
// List Command Handlers
//-----------------------------------------------------------------

let listFile (results : ParseResults<_>) =
    printfn "not implemented yet"
    Continue

let listReference (results : ParseResults<_>) =
    printfn "not implemented yet"
    Continue

let listProject (results : ParseResults<_>) =
    printfn "not implemented yet"
    Continue

let processList args =
    args
    |> processCommand<ListCommands>
    |> Option.bind (fun res ->
        let args' = args.[1 ..]
        match res.GetAllResults () with
        | [cmd] ->
            match cmd with
            | ListCommands.Project -> args' |> processCommand |> Option.map listProject
            | ListCommands.File -> args' |> processCommand |> Option.map listFile
            | ListCommands.Reference -> args' |> processCommand |> Option.map listReference
        | _ -> Some Help
    )


//-----------------------------------------------------------------
// Update Command Handlers
//-----------------------------------------------------------------

let processUpdate args =
    args
    |> processCommand<UpdateArgs>
    |> Option.map (fun results ->
        let paket = results.Contains <@ UpdateArgs.Paket @>
        let fake = results.Contains <@ UpdateArgs.Fake @>
        match paket,fake with
        | true, _ -> Paket.Update ()
        | _, true -> Fake.Update ()
        | false, false -> ()
        Continue
    )


//-----------------------------------------------------------------
// Main Command Handlers
//-----------------------------------------------------------------

let processMain args =
    let res =
        processCommand<Command> args |> Option.bind (fun res ->

            match res.GetAllResults() with
            | [cmd] ->

                try
                    let args' = args.[1 ..]
                    args'
                    |>  match cmd with
                        | Command.New -> processNew
                        | Add -> processAdd
                        | Remove -> processRemove
                        | Rename -> processRename
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
        )
    defaultArg res Result.Exit

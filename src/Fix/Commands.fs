module Commands

open System
open System.Text
open Argu

type Command =
    | [<First>][<CustomCommandLine("new")>] New
    | [<First>][<CustomCommandLine("file")>] File
    | [<First>][<CustomCommandLine("reference")>] Reference
    | [<First>][<CustomCommandLine("update")>] Update
    | [<First>][<CustomCommandLine("paket")>] Paket
    | [<First>][<CustomCommandLine("fake")>] Fake
    | [<First>][<CustomCommandLine("refresh")>] Refresh
    | [<First>][<CustomCommandLine("help")>] Help
    | [<First>][<CustomCommandLine("exit")>] Exit
with
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | New -> "Create new project"
            | File -> "Adds or removes file from current folder and project. If more than one project is in the current directory you will be prompted which to use"
            | Reference -> "Adds or removes reference from current project. If more than one project is in the current directory you will be prompted which to use"
            | Update -> "Updates Paket or FAKE"
            | Paket -> "Runs Paket"
            | Fake -> "Runs FAKE"
            | Refresh -> "Refreshs the template cache"
            | Help -> "Displays help"
            | Exit -> "Exits interactive mode"
    member this.Name =
        let uci,_ = Microsoft.FSharp.Reflection.FSharpValue.GetUnionFields(this, typeof<Command>)
        (uci.GetCustomAttributes(typeof<CustomCommandLineAttribute>)
        |> Seq.head
        :?> CustomCommandLineAttribute).Name

type NewArgs =
    | ProjectName of string
    | ProjectDir of string
    | Template of string
    | No_Paket
    | No_Fake
with
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | ProjectName _ -> "Project name"
            | ProjectDir _ -> "Project directory, relative to Fix working directory"
            | Template _ -> "Template name"
            | No_Paket -> "Don't use Paket for dependency managment"
            | No_Fake -> "Don't use FAKE for build"

type FileArgs =
    |[<CustomCommandLine("add")>] Add of string
    |[<CustomCommandLine("remove")>] Remove of string
    |[<CustomCommandLine("list")>] List
with
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Add _ -> "Adds a file to the current folder and project"
            | Remove _ -> "Removes the file from disk and the project"
            | List  -> " List all files"

type ReferenceArgs =
    |[<CustomCommandLine("add")>] Add of string
    |[<CustomCommandLine("remove")>] Remove of string
    |[<CustomCommandLine("list")>] List
with
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Add _ -> "Adds a reference to the current project"
            | Remove _ -> "Removes the reference from the project"
            | List  -> " List all references"

type UpdateArgs =
    |[<CustomCommandLine("paket")>] Paket
    |[<CustomCommandLine("fake")>] Fake
with
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Paket -> "Updates Paket to latest version"
            | Fake _ -> "Updates FAKE to latest version"


let cmdLineSyntax (parser:ArgumentParser<_>) commandName =
    "fix " + commandName + " " + parser.PrintCommandLineSyntax()

let cmdLineUsageMessage (command : Command) parser =
    let sb = StringBuilder()
    sb.Append("Fix ")
      .AppendLine(command.Name)
      .AppendLine()
      .AppendLine((command :> IArgParserTemplate).Usage)
      .AppendLine()
      .Append(cmdLineSyntax parser command.Name)
      .ToString()

let processWithValidation<'T when 'T :> IArgParserTemplate> validateF commandF command args =
    let parser = ArgumentParser.Create<'T>()
    let results =
        parser.Parse
            (inputs = args, raiseOnUsage = false, ignoreMissing = true,
             errorHandler = ProcessExiter())

    let resultsValid = validateF (results)
    if results.IsUsageRequested || not resultsValid then
        if not resultsValid then
            parser.Usage(cmdLineUsageMessage command parser) |> Console.WriteLine
            Environment.ExitCode <- 1
        else
            parser.Usage(cmdLineUsageMessage command parser)  |> Console.WriteLine
    else
        commandF results

let processCommand<'T when 'T :> IArgParserTemplate> (commandF : ParseResults<'T> -> unit) =
    processWithValidation (fun _ -> true) commandF

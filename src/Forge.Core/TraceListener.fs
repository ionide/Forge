[<AutoOpen>]
/// Defines default listeners for build output traces
module Forge.TraceListener

open System

/// Defines Tracing information for TraceListeners
type TraceData = 
    | StartMessage
    | ImportantMessage of msg:string    
    | ErrorMessage of msg:string
    | WarningMessage of msg:string * newline:bool
    | LogMessage of msg:string * newline:bool
    | TraceMessage of msg:string * newLine:bool
    | FinishedMessage
    | OpenTag of tag:string * name:string
    | CloseTag of tag:string
    member x.NewLine =
        match x with
        | ImportantMessage _
        | WarningMessage _
        | ErrorMessage _ -> Some true
        | LogMessage (_, newLine)
        | TraceMessage (_, newLine) -> Some newLine
        | StartMessage
        | FinishedMessage
        | OpenTag _
        | CloseTag _ -> None
    member x.Message =
        match x with
        | ImportantMessage text
        | ErrorMessage text
        | WarningMessage (text, _)
        | LogMessage (text, _)
        | TraceMessage (text, _) -> Some text
        | StartMessage
        | FinishedMessage
        | OpenTag _
        | CloseTag _ -> None

/// Defines a TraceListener interface
type ITraceListener = 
    abstract Write : TraceData -> unit

/// A default color map which maps TracePriorities to ConsoleColors
let colorMap traceData = 
    match traceData with
    | ImportantMessage _ -> ConsoleColor.Yellow
    | ErrorMessage _ -> ConsoleColor.Red
    | WarningMessage _ -> ConsoleColor.DarkCyan
    | LogMessage _ -> ConsoleColor.Gray
    | TraceMessage _ -> ConsoleColor.Green
    | FinishedMessage -> ConsoleColor.White
    | _ -> ConsoleColor.Gray

/// Implements a TraceListener for System.Console.
/// ## Parameters
///  - `importantMessagesToStdErr` - Defines whether to trace important messages to StdErr.
///  - `colorMap` - A function which maps TracePriorities to ConsoleColors.
type ConsoleTraceListener(importantMessagesToStdErr, colorMap) = 
    
    let writeText toStdErr color newLine text = 
        let curColor = Console.ForegroundColor
        try
          if curColor <> color then Console.ForegroundColor <- color
          let printer =
            match toStdErr, newLine with
            | true, true -> eprintfn
            | true, false -> eprintf
            | false, true -> printfn
            | false, false -> printf
          printer "%s" text
        finally
          if curColor <> color then Console.ForegroundColor <- curColor
    
    interface ITraceListener with
        /// Writes the given message to the Console.
        member this.Write msg = 
            let color = colorMap msg
            match msg with
            | StartMessage -> ()
            | OpenTag _ -> ()
            | CloseTag _ -> ()
            | ImportantMessage text             
            | ErrorMessage text ->
                writeText importantMessagesToStdErr color true text
            | WarningMessage (text, newLine)
            | LogMessage (text, newLine) 
            | TraceMessage (text, newLine) ->
                writeText false color newLine text
            | FinishedMessage -> ()

//// If we write the stderr on those build servers the build will fail.
//let importantMessagesToStdErr = buildServer <> CCNet && buildServer <> AppVeyor

/// The default TraceListener for Console.
let defaultConsoleTraceListener =
 // ConsoleTraceListener(importantMessagesToStdErr, colorMap)
  ConsoleTraceListener(true, colorMap)


/// A List with all registered listeners
let listeners = new Collections.Generic.List<ITraceListener>()


// register listeners
listeners.Add defaultConsoleTraceListener


/// Allows to post messages to all trace listeners
let postMessage x = listeners.ForEach(fun listener -> listener.Write x)



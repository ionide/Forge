[<AutoOpen>]
module Forge.ProcessHelper

open System
open System.Diagnostics
open System.IO
open System.Collections.Generic

/// [omit]
let startedProcesses = HashSet<_>()

/// [omit]
let start (proc : Process) = 
    if isMono && proc.StartInfo.FileName.ToLowerInvariant().EndsWith(".exe") then
        proc.StartInfo.Arguments <- "--debug \"" + proc.StartInfo.FileName + "\" " + proc.StartInfo.Arguments
        proc.StartInfo.FileName <- monoPath

    ignore ^ proc.Start() 
    ignore ^ startedProcesses.Add(proc.Id, proc.StartTime) 

/// [omit]
let mutable redirectOutputToTrace = false

/// [omit]
let mutable enableProcessTracing = true

/// A record type which captures console messages
type ConsoleMessage = 
    { IsError : bool
      Message : string
      Timestamp : DateTimeOffset }

/// A process result including error code, message log and errors.
type ProcessResult = 
    { ExitCode : int
      Messages : ResizeArray<string>
      Errors : ResizeArray<string> }
    member x.OK = x.ExitCode = 0
    static member New exitCode messages errors = 
        { ExitCode = exitCode
          Messages = messages
          Errors = errors }

/// Arguments on the Mono executable
let mutable monoArguments = ""

/// Modifies the ProcessStartInfo according to the platform semantics
let platformInfoAction (psi : ProcessStartInfo) = 
    if isMono && psi.FileName.EndsWith ".exe" then 
        psi.Arguments <- monoArguments + " " + psi.FileName + " " + psi.Arguments
        psi.FileName <- monoPath


/// Runs the given process and returns the exit code.
/// ## Parameters
///
///  - `configProcessStartInfoF` - A function which overwrites the default ProcessStartInfo.
///  - `timeOut` - The timeout for the process.
///  - `silent` - If this flag is set then the process output is redirected to the given output functions `errorF` and `messageF`.
///  - `errorF` - A function which will be called with the error log.
///  - `messageF` - A function which will be called with the message log.
let ExecProcessWithLambdas configProcessStartInfoF (timeOut : TimeSpan) silent errorF messageF = 
    use proc = new Process()
    proc.StartInfo.UseShellExecute <- false
    configProcessStartInfoF proc.StartInfo
    platformInfoAction proc.StartInfo
    if not ^ isNullOrEmpty proc.StartInfo.WorkingDirectory then 
        if not ^ Directory.Exists proc.StartInfo.WorkingDirectory then 
            failwithf "Start of process %s failed. WorkingDir %s does not exist." proc.StartInfo.FileName 
                proc.StartInfo.WorkingDirectory
    if silent then 
        proc.StartInfo.RedirectStandardOutput <- true
        proc.StartInfo.RedirectStandardError <- true
        proc.ErrorDataReceived.Add(fun d -> 
            if d.Data <> null then errorF d.Data)
        proc.OutputDataReceived.Add(fun d -> 
            if d.Data <> null then messageF d.Data)
    try 
        if enableProcessTracing && not ^ proc.StartInfo.FileName.EndsWith "fsi.exe" then 
            tracefn "%s %s" proc.StartInfo.FileName proc.StartInfo.Arguments
        start proc
    with exn -> failwithf "Start of process %s failed. %s" proc.StartInfo.FileName exn.Message
    if silent then 
        proc.BeginErrorReadLine ()
        proc.BeginOutputReadLine ()
    if timeOut = TimeSpan.MaxValue then proc.WaitForExit () 
    elif not ^ proc.WaitForExit (int timeOut.TotalMilliseconds) then 
        try 
            proc.Kill()
        with exn -> 
            traceError 
            <| sprintf "Could not kill process %s  %s after timeout." proc.StartInfo.FileName 
                    proc.StartInfo.Arguments
        failwithf "Process %s %s timed out." proc.StartInfo.FileName proc.StartInfo.Arguments
    proc.ExitCode


/// Runs the given process and returns the process result.
/// ## Parameters
///
///  - `configProcessStartInfoF` - A function which overwrites the default ProcessStartInfo.
///  - `timeOut` - The timeout for the process.
let ExecProcessAndReturnMessages configProcessStartInfoF timeOut = 
    let errors   = ResizeArray<_>()
    let messages = ResizeArray<_>()
    let exitCode = ExecProcessWithLambdas configProcessStartInfoF timeOut true errors.Add messages.Add
    ProcessResult.New exitCode messages errors

/// Runs the given process and returns the process result.
/// ## Parameters
///
///  - `configProcessStartInfoF` - A function which overwrites the default ProcessStartInfo.
///  - `timeOut` - The timeout for the process.
let ExecProcessRedirected configProcessStartInfoF timeOut = 
    let messages = ref []
    
    let appendMessage isError msg = 
        messages := { IsError = isError
                      Message = msg
                      Timestamp = DateTimeOffset.UtcNow } :: !messages
    
    let exitCode = 
        ExecProcessWithLambdas configProcessStartInfoF timeOut true (appendMessage true) (appendMessage false)
    (exitCode = 0, !messages |> List.rev |> Seq.ofList)


/// Runs the given process and returns the exit code.
/// ## Parameters
///
///  - `configProcessStartInfoF` - A function which overwrites the default ProcessStartInfo.
///  - `timeOut` - The timeout for the process.
/// ## Sample
///
///     let result = ExecProcess (fun info ->  
///                       info.FileName <- "c:/MyProc.exe"
///                       info.WorkingDirectory <- "c:/workingDirectory"
///                       info.Arguments <- "-v") (TimeSpan.FromMinutes 5.0)
///     
///     if result <> 0 then failwithf "MyProc.exe returned with a non-zero exit code"
let ExecProcess configProcessStartInfoF timeOut = 
    ExecProcessWithLambdas configProcessStartInfoF timeOut redirectOutputToTrace traceError trace

/// Runs the given process in an elevated context and returns the exit code.
/// ## Parameters
///
///  - `cmd` - The command which should be run in elavated context.
///  - `args` - The process arguments.
///  - `timeOut` - The timeout for the process.
let ExecProcessElevated cmd args timeOut = 
    timeOut
    |> ExecProcess (fun info -> 
        info.Verb            <- "runas"
        info.Arguments       <- args
        info.FileName        <- cmd
        info.UseShellExecute <- true
    ) 

/// Gets the list of valid directories included in the PATH environment variable.
let pathDirectories =
    splitEnvironVar "PATH"
    |> Seq.map (fun value -> value.Trim())
    |> Seq.filter (fun value -> isNotNullOrEmpty value)
    |> Seq.filter isValidPath

/// Sets the environment Settings for the given startInfo.
/// Existing values will be overriden.
/// [omit]
let setEnvironmentVariables (startInfo : ProcessStartInfo) environmentSettings = 
    for key, value in environmentSettings do
        if startInfo.EnvironmentVariables.ContainsKey key then 
            startInfo.EnvironmentVariables.[key] <- value
        else 
            startInfo.EnvironmentVariables.Add(key, value)

/// Runs the given process and returns true if the exit code was 0.
/// [omit]
let execProcess configProcessStartInfoF timeOut = ExecProcess configProcessStartInfoF timeOut = 0

/// Starts the given process and returns immediatly.
let fireAndForget configProcessStartInfoF = 
    use proc = new Process ()
    proc.StartInfo.UseShellExecute <- false
    configProcessStartInfoF proc.StartInfo
    try 
        start proc
    with exn -> failwithf "Start of process %s failed. %s" proc.StartInfo.FileName exn.Message

/// Runs the given process, waits for its completion and returns if it succeeded.
let directExec configProcessStartInfoF = 
    use proc = new Process ()
    proc.StartInfo.UseShellExecute <- false
    configProcessStartInfoF proc.StartInfo
    try 
        start proc
    with exn -> failwithf "Start of process %s failed. %s" proc.StartInfo.FileName exn.Message
    proc.WaitForExit ()
    proc.ExitCode = 0

/// Starts the given process and forgets about it.
let StartProcess configProcessStartInfoF = 
    use proc = new Process()
    proc.StartInfo.UseShellExecute <- false
    configProcessStartInfoF proc.StartInfo
    start proc


let run cmd args dir =
    if execProcess( fun info ->
        info.FileName <- cmd
        if not ^ isNullOrWhiteSpace dir then
            info.WorkingDirectory <- dir
        info.Arguments <- args
    ) System.TimeSpan.MaxValue = false then
        traceError ^ sprintf "Error while running '%s' with args: %s" cmd args
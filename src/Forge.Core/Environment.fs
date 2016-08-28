[<AutoOpen>]
module Forge.Environment

open System
open System.IO
open System.Diagnostics


// Environment Helpers
//=======================================================

/// Retrieves the environment variable with the given name
let environVar name = Environment.GetEnvironmentVariable name

let environVarOrNone name =
    let var = environVar name
    if String.IsNullOrEmpty var then None
    else Some var

/// Splits the entries of an environment variable and removes the empty ones.
let splitEnvironVar name =
    let var = environVarOrNone name
    if var = None then [ ]
    else var.Value.Split [| Path.PathSeparator |] |> Array.toList

/// The system root environment variable. Typically "C:\Windows"
let SystemRoot = environVar "SystemRoot"

/// Determines if the current system is an Unix system
let isUnix = Environment.OSVersion.Platform = PlatformID.Unix

/// Determines if the current system is a MacOs system
let isMacOS =
    (Environment.OSVersion.Platform = PlatformID.MacOSX) ||
      // osascript is the AppleScript interpreter on OS X
      File.Exists "/usr/bin/osascript"

/// Determines if the current system is a Linux system
let isLinux = int System.Environment.OSVersion.Platform |> fun p -> p=4 || p=6 || p=128

/// Determines if the current system is a mono system
/// Todo: Detect mono on windows
let isMono = isLinux || isUnix || isMacOS

let monoPath =
    if isMacOS && File.Exists "/Library/Frameworks/Mono.framework/Commands/mono" then
        "/Library/Frameworks/Mono.framework/Commands/mono"
    else
        "mono"

/// The path of the "Program Files" folder - might be x64 on x64 machine
let ProgramFiles = Environment.GetFolderPath Environment.SpecialFolder.ProgramFiles

/// The path of Program Files (x86)
/// It seems this covers all cases where PROCESSOR\_ARCHITECTURE may misreport and the case where the other variable
/// PROCESSOR\_ARCHITEW6432 can be null
let ProgramFilesX86 =
    let wow64 = environVar "PROCESSOR_ARCHITEW6432"
    let globalArch = environVar "PROCESSOR_ARCHITECTURE"
    match wow64, globalArch with
    | "AMD64", "AMD64"
    | null, "AMD64"
    | "x86", "AMD64" -> environVar "ProgramFiles(x86)"
    | _ -> environVar "ProgramFiles"
    |> fun detected -> if detected = null then @"C:\Program Files (x86)\" else detected


/// Returns if the build parameter with the given name was set
let inline hasBuildParam name = environVar name <> null

/// Type alias for System.EnvironmentVariableTarget
type EnvironTarget = EnvironmentVariableTarget

/// Retrieves all environment variables from the given target
let environVars target =
    [ for e in Environment.GetEnvironmentVariables target ->
          let e1 = e :?> Collections.DictionaryEntry
          e1.Key, e1.Value ]


// Environment Config
//====================================================

let exeLocation = 
    try
        System.Reflection.Assembly.GetEntryAssembly().Location |> Path.GetDirectoryName
    with 
    | _ -> ""
let templatesLocation = exeLocation </> ".." </> "templates"
let directory         = System.Environment.CurrentDirectory
let packagesDirectory = directory </> "packages"

let paketLocation     = exeLocation </> "Tools" </> "Paket"
let fakeLocation      = exeLocation </> "Tools" </> "FAKE"
let fakeToolLocation  = fakeLocation </> "tools"

let filesLocation = templatesLocation </> ".files"
let templateFile = templatesLocation </> "templates.json"

let relative (path1 : string) (path2 : string) =
    let path1 = if Path.IsPathRooted path1 then path1 else directory </> path1
    let path2 = if Path.IsPathRooted path2 then path2 else directory </> path2

    let p1, p2 = Uri path1, Uri path2
    Uri.UnescapeDataString(
        p2.MakeRelativeUri(p1)
            .ToString()
            .Replace('/', Path.DirectorySeparatorChar)
    )  
    

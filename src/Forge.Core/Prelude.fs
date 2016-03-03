[<AutoOpen>]
module Forge.Prelude

open System
open System.IO
open System.Diagnostics
open System.Xml


// Operators
//========================


let (^) = (<|)



/// Combines two path strings using Path.Combine
let inline combinePaths path1 (path2 : string) = Path.Combine (path1, path2.TrimStart [| '\\'; '/' |])
let inline combinePathsNoTrim path1 path2 = Path.Combine (path1, path2)

/// Combines two path strings using Path.Combine
let inline (@@) path1 path2 = combinePaths path1 path2
let inline (</>) path1 path2 = combinePathsNoTrim path1 path2


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



/// Detects whether the given path does not contains invalid characters.
let isValidPath (path:string) =
    let invalidChars = Path.GetInvalidPathChars()
    (true, path.ToCharArray())
    ||> Array.fold (fun isValid pathChar ->
        if not isValid then false else
        not ^ Array.exists ((=) pathChar) invalidChars
    ) 

/// Returns if the build parameter with the given name was set
let inline hasBuildParam name = environVar name <> null

/// Type alias for System.EnvironmentVariableTarget
type EnvironTarget = EnvironmentVariableTarget

/// Retrieves all environment variables from the given target
let environVars target = 
    [ for e in Environment.GetEnvironmentVariables target -> 
          let e1 = e :?> Collections.DictionaryEntry
          e1.Key, e1.Value ]

// String Helpers
//=====================================================

/// Converts a sequence of strings to a string with delimiters
let inline separated delimiter (items : string seq) = String.Join(delimiter, Array.ofSeq items)

/// Returns if the string is null or empty
let inline isNullOrEmpty value = String.IsNullOrEmpty value

/// Returns if the string is not null or empty
let inline isNotNullOrEmpty value = not ^ String.IsNullOrEmpty value

/// Returns if the string is null or empty or completely whitespace
let inline isNullOrWhiteSpace value = isNullOrEmpty value || value |> Seq.forall Char.IsWhiteSpace

/// Converts a sequence of strings into a string separated with line ends
let inline toLines text = separated Environment.NewLine text

/// Checks whether the given text starts with the given prefix
let startsWith prefix (text : string) = text.StartsWith prefix

/// Checks whether the given text ends with the given suffix
let endsWith suffix (text : string) = text.EndsWith suffix

/// Determines whether the last character of the given <see cref="string" />
/// matches Path.DirectorySeparatorChar.         
let endsWithSlash = endsWith (Path.DirectorySeparatorChar.ToString())
/// Reads a file as one text
let inline readFileAsString file = File.ReadAllText file

/// Replaces the given pattern in the given text with the replacement
let inline replace (pattern : string) replacement (text : string) = text.Replace(pattern, replacement)

/// Replaces the first occurrence of the pattern with the given replacement.
let replaceFirst (pattern : string) replacement (text : string) = 
    let pos = text.IndexOf pattern
    if pos < 0 then text
    else text.Remove(pos, pattern.Length).Insert(pos, replacement)

/// Trims the given string with the DirectorySeparatorChar
let inline trimSeparator (s : string) = s.TrimEnd Path.DirectorySeparatorChar


// Process Helpers
//=====================================================


let prompt text =
    printfn text
    Console.Write "> "
    Console.ReadLine ()

let promptSelect text list =
    printfn text
    list |> Seq.iter (printfn " - %s")
    printfn ""
    Console.Write "> "
    Console.ReadLine ()

/// Loads the given text into a XmlDocument
let XMLDoc text = 
    if isNullOrEmpty text then null else 
    let xmlDocument = XmlDocument ()
    xmlDocument.LoadXml text
    xmlDocument


// Environment Config
//====================================================

let exeLocation       = System.Reflection.Assembly.GetEntryAssembly().Location |> Path.GetDirectoryName
let templatesLocation = exeLocation </> ".." </> "templates"
let directory         = System.Environment.CurrentDirectory
let packagesDirectory = directory </> "packages"

let paketLocation     = exeLocation </> "Tools" </> "Paket"
let fakeLocation      = exeLocation </> "Tools" </> "FAKE"
let fakeToolLocation  = fakeLocation </> "tools"


let inline mapOpt (opt:'a option) mapfn (x:'b) =
    match opt with
    | None -> x
    | Some a -> mapfn a x 

let parseGuid text = 
    let mutable g = Unchecked.defaultof<Guid>
    if Guid.TryParse(text,&g) then Some g else None

let parseBool text =
    let mutable b = Unchecked.defaultof<bool>
    if Boolean.TryParse(text,&b) then Some b else None

[<RequireQualifiedAccess>]
module Option =

    /// Gets the value associated with the option or the supplied default value.
    let inline getOrElse v = function Some x -> x | None -> v

    /// Gets the value associated with the option or the supplied default value.
    let inline mapOrDefault mapfn v =
        function
        | Some x -> mapfn x
        | None -> v

[<RequireQualifiedAccess>]
module Dict = 
    open System.Collections.Generic

    let add key value (dict: Dictionary<_,_>) =
        dict.[key] <- value
        dict

    let remove (key: 'k) (dict: Dictionary<'k,_>) =
        dict.Remove key |> ignore
        dict

    let tryFind key (dict: Dictionary<'k, 'v>) = 
        let mutable value = Unchecked.defaultof<_>
        if dict.TryGetValue (key, &value) then Some value
        else None

    let ofSeq (xs: ('k * 'v) seq) = 
        let dict = Dictionary()
        for k, v in xs do dict.[k] <- v
        dict
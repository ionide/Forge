[<AutoOpen>]
module Forge.FileHelper
open System
open System.IO
open System.Collections.Generic
open Forge.Globbing


// File Helpers
//=====================================================

/// Gets the directory part of a filename.
let inline directoryName fileName = Path.GetDirectoryName fileName

/// Checks if the directory exists on disk.
let directoryExists dir = Directory.Exists dir

/// Ensure that directory chain exists. Create necessary directories if necessary.
let inline ensureDirExists (dir : DirectoryInfo) =
    if not dir.Exists then dir.Create ()

/// Checks if the given directory exists. If not then this functions creates the directory.
let inline ensureDirectory dir = DirectoryInfo dir |> ensureDirExists

/// Creates a file if it does not exist.
let createFile fileName =
    let file = FileInfo fileName
    if file.Exists then logfn "%s already exists." file.FullName else
    logfn "Creating %s" file.FullName
    let newFile = file.Create ()
    newFile.Close ()

/// Deletes a file if it exists.
let deleteFile fileName =
    let file = FileInfo fileName
    if not ^ file.Exists then logfn "%s does not exist." file.FullName else
    logfn "Deleting %s" file.FullName
    file.Delete ()

/// Renames the file to the target file name.
let renameFile fileName target = (FileInfo fileName).MoveTo (getCwd() </> target)

/// Renames the directory to the target directory name.
let renameDir target dirName = (DirectoryInfo dirName).MoveTo target


/// Gets the list of valid directories included in the PATH environment variable.
let pathDirectories =
    splitEnvironVar "PATH"
    |> Seq.map (fun value -> value.Trim())
    |> Seq.filter String.isNotNullOrEmpty
    |> Seq.filter isValidPath


/// Searches the given directories for all occurrences of the given file name
/// [omit]
let tryFindFile dirs file =
    let files =
        dirs
        |> Seq.map (fun (path : string) ->
            let dir =
                path
                |> String.replace "[ProgramFiles]" ProgramFiles
                |> String.replace "[ProgramFilesX86]" ProgramFilesX86
                |> String.replace "[SystemRoot]" SystemRoot
                |> DirectoryInfo
            if not dir.Exists then "" else
            let fi = dir.FullName @@ file |> FileInfo
            if fi.Exists then fi.FullName else ""
        )
        |> Seq.filter ((<>) "")
        |> Seq.cache
    if not ^ Seq.isEmpty files then Some ^ Seq.head files
    else None

/// Searches the given directories for the given file, failing if not found.
/// [omit]
let findFile dirs file =
    match tryFindFile dirs file with
    | Some found -> found
    | None -> failwithf "%s not found in %A." file dirs

/// Searches the current directory and the directories within the PATH
/// environment variable for the given file. If successful returns the full
/// path to the file.
/// ## Parameters
///  - `file` - The file to locate
let tryFindFileOnPath (file : string) : string option =
    pathDirectories
    |> Seq.append [ "." ]
    |> fun path -> tryFindFile path file

/// Returns the AppSettings for the key - Splitted on ;
/// [omit]
let appSettings (key : string) (fallbackValue : string) =
    let value = fallbackValue
        // let setting =
        //     try
        //         System.Configuration.ConfigurationManager.AppSettings.[key]
        //     with exn -> ""
        // if not ^ String.isNullOrWhiteSpace setting then setting
        // else fallbackValue
    value.Split ([| ';' |], StringSplitOptions.RemoveEmptyEntries)


/// Tries to find the tool via AppSettings. If no path has the right tool we are trying the PATH system variable.
/// [omit]
let tryFindPath settingsName fallbackValue tool =
    let paths = appSettings settingsName fallbackValue
    match tryFindFile paths tool with
    | Some path -> Some path
    | None -> tryFindFileOnPath tool

/// Tries to find the tool via AppSettings. If no path has the right tool we are trying the PATH system variable.
/// [omit]
let findPath settingsName fallbackValue tool =
    match tryFindPath settingsName fallbackValue tool with
    | Some file -> file
    | None -> tool

/// Internal representation of a file set.
type FileIncludes =
    { BaseDirectory : string
      Includes : string list
      Excludes : string list }

    /// Adds the given pattern to the file includes
    member this.And pattern = { this with Includes = this.Includes @ [ pattern ] }

    /// Ignores files with the given pattern
    member this.ButNot pattern = { this with Excludes = pattern :: this.Excludes }

    /// Sets a directory as BaseDirectory.
    member this.SetBaseDirectory(dir : string) = { this with BaseDirectory = dir.TrimEnd(Path.DirectorySeparatorChar) }

    /// Checks if a particular file is matched
    member this.IsMatch (path : string) =
        let fullDir pattern =
            if Path.IsPathRooted pattern then pattern  else
            System.IO.Path.Combine (this.BaseDirectory, pattern)

        let included =
            this.Includes
            |> Seq.exists (fun fileInclude ->
                Globbing.isMatch (fullDir fileInclude) path
            )
        let excluded =
            this.Excludes
            |> Seq.exists (fun fileExclude ->
                Globbing.isMatch (fullDir fileExclude) path
            )

        included && not excluded

    interface IEnumerable<string> with

        member this.GetEnumerator() =
            let hashSet = HashSet<_> ()

            let excludes =
                seq { for pattern in this.Excludes do
                        yield! Globbing.search this.BaseDirectory pattern
                } |> Set.ofSeq

            let files =
                seq { for pattern in this.Includes do
                        yield! Globbing.search this.BaseDirectory pattern
                } |> Seq.filter (fun x -> not ^ Set.contains x excludes)
                |> Seq.filter hashSet.Add

            files.GetEnumerator ()

        member this.GetEnumerator () = (this :> seq<string>).GetEnumerator() :> System.Collections.IEnumerator

let private defaultBaseDir = Path.GetFullPath "."


/// Include files
let Include x =
    { BaseDirectory = defaultBaseDir
      Includes = [ x ]
      Excludes = [] }

/// Sets a directory as baseDirectory for fileIncludes.
let SetBaseDir (dir : string) (fileIncludes : FileIncludes) = fileIncludes.SetBaseDirectory dir

/// Add Include operator
let inline (++) (x : FileIncludes) pattern = x.And pattern

/// Exclude operator
let inline (--) (x : FileIncludes) pattern = x.ButNot pattern

/// Includes a single pattern and scans the files - !! x = AllFilesMatching x
let inline (!!) x = Include x

/// Looks for a tool first in its default path, if not found in all subfolders of the root folder - returns the tool file name.
let findToolInSubPath toolname defaultPath =
    try
        let tools = !! (defaultPath @@ "/**/" @@ toolname)
        if  Seq.isEmpty tools then
            let root = !! ("./**/" @@ toolname)
            Seq.head root
        else
            Seq.head tools
    with
    | _ -> defaultPath @@ toolname

/// Looks for a tool in all subfolders - returns the folder where the tool was found.
let findToolFolderInSubPath toolname defaultPath =
    try
        let tools = !! ("./**/" @@ toolname)
        if Seq.isEmpty tools then defaultPath
        else
            let fi = FileInfo (Seq.head tools)
            fi.Directory.FullName
    with
    | _ -> defaultPath

/// Gets all subdirectories of a given directory.
let inline subDirectories (dir : DirectoryInfo) = dir.GetDirectories()

/// Gets all files in the directory.
let inline filesInDir (dir : DirectoryInfo) = dir.GetFiles()

/// Performs the given actions on all files and subdirectories
let rec recursively dirF fileF (dir : DirectoryInfo) =
    dir
    |> subDirectories
    |> Seq.iter (fun dir -> recursively dirF fileF dir; dirF dir)
    dir |> filesInDir |> Seq.iter fileF

/// Finds all the files in the directory matching the search pattern.
let filesInDirMatching pattern (dir : DirectoryInfo) =
    if dir.Exists then dir.GetFiles pattern
    else [||]

/// Sets the directory readonly
let setDirectoryReadOnly readOnly (dir : DirectoryInfo) =
    if dir.Exists then
        let isReadOnly = dir.Attributes &&& FileAttributes.ReadOnly = FileAttributes.ReadOnly
        if readOnly && (not isReadOnly) then dir.Attributes <- dir.Attributes ||| FileAttributes.ReadOnly
        if (not readOnly) && not isReadOnly then dir.Attributes <- dir.Attributes &&& (~~~FileAttributes.ReadOnly)

/// Sets all files in the directory readonly.
let setDirReadOnly readOnly dir =
    recursively (setDirectoryReadOnly readOnly) (fun file -> file.IsReadOnly <- readOnly) dir

/// Sets all given files readonly.
let setReadOnly readOnly (files : string seq) =
    files |> Seq.iter (fun file ->
        let fi = FileInfo file
        if fi.Exists then fi.IsReadOnly <- readOnly else
        file
        |> DirectoryInfo
        |> setDirectoryReadOnly readOnly
    )

/// Deletes a directory if it exists.
let deleteDir path =
    let dir = DirectoryInfo path
    if not dir.Exists then printfn "%s does not exist." dir.FullName else
    // set all files readonly = false
    !!"/**/*.*"
    |> SetBaseDir dir.FullName
    |> setReadOnly false
    printfn "Deleting %s" dir.FullName
    dir.Delete true

/// Creates a directory if it does not exist.
let createDir path =
    let dir = DirectoryInfo path
    if dir.Exists then printfn "%s already exists." dir.FullName else
    printfn "Creating %s" dir.FullName
    dir.Create ()

/// Copies a directory recursivly. If the target directory does not exist, it will be created.
/// ## Parameters
///
///  - `target` - The target directory.
///  - `source` - The source directory.
///  - `filterFile` - A file filter predicate.
let copyDir target source filterFile overwrite =
    createDir target
    Directory.GetFiles(source, "*.*", SearchOption.AllDirectories)
    |> Seq.filter filterFile
    |> Seq.iter (fun file ->
        let fi = file |> String.replaceFirst source "" |> String.trimSeparator
        let newFile = target @@ fi
        printfn "%s => %s" file newFile
        directoryName newFile |> ensureDirectory
        File.Copy (file, newFile, overwrite)
    ) |> ignore

/// Cleans a directory by removing all files and sub-directories.
let cleanDir path =
    let di = DirectoryInfo path
    if not di.Exists then createDir path else
    printfn "Deleting contents of %s" path
    // delete all files
    Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
    |> Seq.iter (fun file ->
        let fi = FileInfo file
        fi.IsReadOnly <- false
        fi.Delete ()
    )
    // deletes all subdirectories
    let rec deleteDirs actDir =
        Directory.GetDirectories actDir |> Seq.iter deleteDirs
        Directory.Delete (actDir, true)
    Directory.GetDirectories path |> Seq.iter deleteDirs

    // set writeable
    File.SetAttributes (path, FileAttributes.Normal)

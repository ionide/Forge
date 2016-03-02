#if INTERACTIVE
#load "Prelude.fs"
#load "Globbing.fs"
#load "TraceListener.fs"
#load "TraceHelper.fs"
#load "FileHelper.fs"
open Forge.Prelude
open Forge.FileHelper
#else
module Forge.GacSearch
#endif

open System
open System.IO

let assemblyRoots = ["assembly"; "Microsoft.NET\\assembly"]
let gacDirs = ["GAC"; "GAC_32"; "GAC_64"; "GAC_MSIL"]

let private subdirs d = Directory.EnumerateDirectories(d)

let private getAssemblyDirs roots =
    roots
    |> Seq.map (combinePathsNoTrim SystemRoot)
    |> Seq.collect (fun dir -> gacDirs |> Seq.map (combinePathsNoTrim dir)) // potential root assembly dirs
    |> Seq.filter directoryExists
    |> Seq.collect subdirs // assembly dirs
    |> Seq.collect subdirs // assembly version dirs

let private getAssemblyFiles dir =
    let strEqual s1 s2 = String.Equals(s1, s2, StringComparison.InvariantCultureIgnoreCase)
    let filterFiles (f : FileInfo) = strEqual f.Extension ".dll" || strEqual f.Extension ".exe"
    let info = DirectoryInfo dir
    info
    |> filesInDir
    |> Seq.where filterFiles

let tryGetAssemblyName (info : FileInfo) =
    try
        Some (System.Reflection.AssemblyName.GetAssemblyName(info.FullName))
    with
    | :? BadImageFormatException -> None

let searchGac () =
    assemblyRoots
    |> getAssemblyDirs
    |> Seq.collect getAssemblyFiles
    |> Seq.choose tryGetAssemblyName

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
open System.Text.RegularExpressions

let private assemblyRoots = ["assembly"; "Microsoft.NET\\assembly"]
let private gacDirs = ["GAC"; "GAC_32"; "GAC_64"; "GAC_MSIL"]
let private policyRegex = Regex(@"^policy\.\d+\..+$", RegexOptions.IgnoreCase ||| RegexOptions.Compiled)

let private subdirs d = Directory.EnumerateDirectories(d)

let private getAssemblyDirs roots =
    roots
    |> Seq.map (combinePathsNoTrim SystemRoot)
    |> Seq.collect (fun dir -> gacDirs |> Seq.map (combinePathsNoTrim dir)) // potential root assembly dirs
    |> Seq.filter directoryExists
    |> Seq.collect subdirs // assembly dirs
    |> Seq.where (fun x -> x.EndsWith(".resources") |> not)
    |> Seq.collect subdirs // assembly version dirs

let private getAssemblyFiles dir =
    let strEqual s1 s2 = String.Equals(s1, s2, StringComparison.InvariantCultureIgnoreCase)
    let isAssemblyFile (f : FileInfo) = strEqual f.Extension ".dll" || strEqual f.Extension ".exe"
    let isNotPolicy (f : FileInfo) = policyRegex.IsMatch f.Name |> not
    DirectoryInfo dir
    |> filesInDir
    |> Seq.where isAssemblyFile
    |> Seq.where isNotPolicy

let private tryGetAssemblyName (info : FileInfo) =
    try
        Some (System.Reflection.AssemblyName.GetAssemblyName(info.FullName))
    with
    | _ -> None //TODO: Add logging

/// Looks for assemblies in GAC
/// Returns AssemblyName instances for all found assemblies except resource and policy assemblies
let searchGac () =
    assemblyRoots
    |> getAssemblyDirs
    |> Seq.collect getAssemblyFiles
    |> Seq.choose tryGetAssemblyName

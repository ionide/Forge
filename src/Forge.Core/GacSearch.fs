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

type AssemblyDetails = {
    Info : FileInfo
    Version : string
    PublicKeyToken : string
    Architecture : Architecture option
    Culture : string
    Location : DirectoryInfo }

and Architecture =
    | X86
    | X64
    | MSIL

let private subdirs d = Directory.EnumerateDirectories(d)

let private getAssemblyDirs roots =
    roots
    |> Seq.map (combinePathsNoTrim SystemRoot)
    |> Seq.collect (fun dir -> gacDirs |> Seq.map (combinePathsNoTrim dir)) // potential root assembly dirs
    |> Seq.filter directoryExists
    |> Seq.collect subdirs // assembly dirs
    |> Seq.collect subdirs // assembly version dirs

let private getDirInfo dir =
    let strEqual s1 s2 = String.Equals(s1, s2, StringComparison.InvariantCultureIgnoreCase)
    let filterFile (f : FileInfo) = strEqual f.Extension ".dll" || strEqual f.Extension ".exe"
    let info = DirectoryInfo dir
    let parts = info.Name.Split '_'
    let files =
        info
        |> filesInDir
        |> Seq.where filterFile
        |> List.ofSeq
    info, parts, files

let private getArch (dir : DirectoryInfo) =
    match dir.Parent.Parent.Name with
    | "GAC_32" -> Some X86
    | "GAC_64" -> Some X64
    | "GAC_MSIL" -> Some MSIL
    | _ -> None

let private getAssemblyDetails dir parts files =
    match files with
    | [] -> None
    | _ ->
        let has3 = (Array.length parts) = 3
        Some {
            Info = files.[0]
            Version = if has3 then parts.[0] else sprintf "%s_%s" parts.[0] parts.[1]
            PublicKeyToken = if has3 then parts.[2] else parts.[3]
            Architecture = getArch dir
            Culture = if has3 then parts.[1] else parts.[2]
            Location = dir }

let searchGac () =
    assemblyRoots
    |> getAssemblyDirs
    |> Seq.map getDirInfo
    |> Seq.choose (fun (info, parts, files)-> getAssemblyDetails info parts files)
    |> Seq.sortBy (fun d -> d.Info.Name)

#if INTERACTIVE
searchGac ()
|> Seq.take 100
|> Seq.iter (printfn "%A");;
#endif

module Forge.GacSearch

open System
open System.IO

let dirExists d =
    match Directory.Exists d with
    | true -> Some d
    | _ -> None
let combine p1 p2 = Path.Combine(p1, p2)
let assemblyDirs = ["assembly"; "Microsoft.NET\\assembly"]
let gacDirs = ["GAC"; "GAC_32"; "GAC_64"; "GAC_MSIL"]
let windowsFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows)
let fullAssemblyDirs =
    assemblyDirs
    |> Seq.map (combine windowsFolderPath)
    |> Seq.collect (fun dir -> gacDirs |> Seq.map (combine dir))
    |> Seq.choose dirExists
    |> Seq.collect (fun dir -> Directory.EnumerateDirectories(dir))
    |> Seq.cache
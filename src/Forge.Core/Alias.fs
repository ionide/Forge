module Forge.Alias

open System
open System.IO
open Nett
open Prelude

let defaultAliases =
    [
        "build", "fake"
        "test", "fake Test"
        "release", "fake Release"
        "install", "paket install"
        "update", "paket update"
    ] |> Map.ofList

let private merge original added =
    added |> Map.fold (fun (state : Map<string,string>) key value ->
        if state.ContainsKey key then
            state.Remove(key).Add(key,value)
        else
            state.Add(key, value) ) original


let private parse path =
    if File.Exists path then
        let toml = Toml.ReadFile path
        let alias = toml.Get<TomlTable>("alias")
        alias.Rows
        |> Seq.map (fun kv -> kv.Key, kv.Value.Get<string>())
        |> Map.ofSeq
    else
        Map.empty

let private loadLocal () =
    let path = directory </> "forge.config"
    parse path

let private loadGlobal () =
    let path = exeLocation </> "forge.config"
    parse path

let load () =
    let globals = loadGlobal ()
    let locals = loadLocal ()

    locals |> merge globals |> merge defaultAliases

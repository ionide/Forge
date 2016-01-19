module Paket

open Fake
open System.IO
open Common

let location = templatesLocation </> ".paket"

let Copy folder =
    folder </> ".paket" |> Directory.CreateDirectory |> ignore
    Directory.GetFiles location
    |> Seq.iter (fun x ->
        let fn = Path.GetFileName x
        File.Copy (x, folder </> ".paket" </> fn) )

let Update () =
    run (paketLocation </> "paket.bootstrapper.exe") "" paketLocation

let Run args =
    let f = paketLocation </> "paket.exe"
    if not ^ File.Exists f then Update ()
    let args' = args |> String.concat " "
    run f args' directory




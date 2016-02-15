module Forge.Paket

open System.IO

let location = templatesLocation </> ".paket"

let getPaketLocation () =
    let local = directory </> ".paket"
    if Directory.Exists local then local else paketLocation

let getPaket () =
    let local = directory </> ".paket" </> "paket.exe"
    if File.Exists local then local else paketLocation </> "paket.exe"


let Copy folder =
    folder </> ".paket" |> Directory.CreateDirectory |> ignore
    Directory.GetFiles location
    |> Seq.iter (fun x ->
        let fn = Path.GetFileName x
        File.Copy (x, folder </> ".paket" </> fn, true) )

let Update () =
    let f = getPaketLocation ()
    run (f </> "paket.bootstrapper.exe") "" f

let Run args =
    let f = getPaket ()
    if not ^ File.Exists f then Update ()
    let args' = args |> String.concat " "
    run f args' directory




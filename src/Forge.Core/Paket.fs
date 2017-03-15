module Forge.Paket

open System.IO

let location = templatesLocation </> ".paket"

let getPaketLocation () =
    let local = directory </> ".paket"
    if Directory.Exists local then local else paketLocation

let getPaket () =
    getPaketLocation () </> "paket.exe"

let copy folder =
    folder </> ".paket" |> Directory.CreateDirectory |> ignore
    Directory.GetFiles location
    |> Seq.iter (fun x ->
        let filename = Path.GetFileName x
        File.Copy (x, folder </> ".paket" </> filename, true) )


let Init folder =
    if Directory.GetFiles folder |> Seq.exists (fun n -> n.EndsWith "paket.dependencies") |> not then
        copy folder
        Update ()
        Run ["init"]

let Update () =
    let f = getPaketLocation ()
    run (f </> "paket.bootstrapper.exe") "" f

let Run args =
    let f = getPaket ()
    if not ^ File.Exists f then Update ()
    let args' = args |> String.concat " "
    run f args' directory




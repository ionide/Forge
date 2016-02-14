module Forge.Fake
open Forge.ZipHelper
open System.IO
open System.Net

let location = templatesLocation </> ".fake"

let Copy folder =
    Directory.GetFiles location
    |> Seq.iter (fun x ->
        let fn = (folder </> Path.GetFileName x)
        if not ^ File.Exists fn then File.Copy (x, fn) )

let getFAKE () =
    match Directory.EnumerateFiles(directory, "FAKE.exe", SearchOption.AllDirectories) |> Seq.tryHead with
    | Some f -> f
    | None -> fakeToolLocation </> "FAKE.exe"


let Update () =
    use wc = new WebClient()
    let zip = fakeLocation </> "fake.zip"
    System.IO.Directory.CreateDirectory(fakeLocation) |> ignore
    printfn "Downloading FAKE..."
    wc.DownloadFile("https://www.nuget.org/api/v2/package/FAKE", zip )
    Unzip fakeLocation zip

let Run args =
    let f = getFAKE ()
    if not ^ File.Exists f then Update ()
    let args' = args |> String.concat " "
    run f args' directory


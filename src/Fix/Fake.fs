module Fake
open Fake
open System.IO
open System.Net
open Common


let Update () =
    use wc = new WebClient()
    let zip = fakeLocation </> "fake.zip"
    System.IO.Directory.CreateDirectory(fakeLocation) |> ignore
    printfn "Downloading FAKE..."
    wc.DownloadFile("https://www.nuget.org/api/v2/package/FAKE", zip )
    Fake.ZipHelper.Unzip fakeLocation zip

let Run args =
    let f = fakeToolLocation </> "FAKE.exe"
    if not ^ File.Exists f then Update ()
    let args' = args |> String.concat " "
    run f args' directory


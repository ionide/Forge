[<AutoOpen>]
module Helpers

open Forge.ProcessHelper
open Forge.Environment
open Expecto

let (</>) = Forge.Prelude.(</>)

let cwd = System.AppDomain.CurrentDomain.BaseDirectory

let initTest dir args =
    let path = cwd </> ".." </> "Forge.exe"
    let dir = cwd </> dir
    Forge.FileHelper.cleanDir dir
    System.Environment.CurrentDirectory <- dir
    args |> List.iter (fun a ->
        let a = a + " --no-prompt"
        Forge.App.main (a.Split ' ') |> ignore)

let runForgeWithOutput args =
    let sw = new System.IO.StringWriter()
    System.Console.SetOut(sw)
    args |> List.iter (fun a ->
        let a = a + " --no-prompt"
        Forge.App.main (a.Split ' ') |> ignore)
    let so = new System.IO.StreamWriter(System.Console.OpenStandardOutput())
    so.AutoFlush <- true
    System.Console.SetOut(so)
    sw.ToString()


let getPath file =
    cwd </> file

let loadProject dir =
    let dir = cwd </> dir
    Forge.ProjectManager.Furnace.loadFsProject dir


let makeAbsolute dir = cwd </> dir

module Expect =
    let reference (ref : string) (proj : Forge.ProjectManager.ActiveState) =
        let res =
            proj.ProjectData.References
            |> Seq.map (fun r -> r.Include)

        Expect.contains res ref "should contain reference"

    let referenceProject (ref : string) (proj : Forge.ProjectManager.ActiveState) =
        let res =
            proj.ProjectData.ProjectReferences
            |> Seq.map (fun r -> r.Include)

        Expect.contains res ref "should contain project reference"


    let hasFile (file : string) (proj : Forge.ProjectManager.ActiveState) =
        let res = proj.ProjectData.SourceFiles.Files

        Expect.contains res file "should contain file"

    let hasName (name : string) (proj : Forge.ProjectManager.ActiveState) =
        let res = proj.ProjectData.Settings.Name.Data.Value

        Expect.equal res name "should have name"

    let notReference (ref : string) (proj : Forge.ProjectManager.ActiveState) =
        let res =
            proj.ProjectData.References
            |> Seq.map (fun r -> r.Include)

        Expect.equal (res |> Seq.tryFind ((=) ref)) None "shouln't contain reference"

    let notReferenceProject (ref : string) (proj : Forge.ProjectManager.ActiveState) =
        let res =
            proj.ProjectData.ProjectReferences
            |> Seq.map (fun r -> r.Include)
        Expect.equal (res |> Seq.tryFind ((=) ref)) None "shouln't contain project reference"

    let hasNotFile (file : string) (proj : Forge.ProjectManager.ActiveState) =
        let res = proj.ProjectData.SourceFiles.Files

        Expect.equal (res |> Seq.tryFind ((=) file)) None "shouln't contain file"

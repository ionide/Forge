[<AutoOpen>]
module Helpers

open Forge.ProcessHelper
open NUnit.Framework.Constraints
open NUnit.Framework

let (</>) = Forge.Prelude.(</>)

let initTest dir args =
    let path = TestContext.CurrentContext.TestDirectory </> ".." </> "bin" </> "Forge.exe"
    let dir = TestContext.CurrentContext.TestDirectory </> dir
    Forge.FileHelper.cleanDir dir
    args |> List.iter (fun a -> run path (a + " --no-prompt") dir)

let loadProject dir =
    let dir = TestContext.CurrentContext.TestDirectory </> dir
    Forge.ProjectManager.Furnace.loadFsProject dir


let makeAbsolute dir = TestContext.CurrentContext.TestDirectory </> dir

module Assertions =
    let reference (ref : string) (proj : Forge.ProjectManager.ActiveState) =
        let res =
            proj.ProjectData.References
            |> Seq.map (fun r -> r.Include)

        Assert.That(res, ContainsConstraint(ref))

    let referenceProject (ref : string) (proj : Forge.ProjectManager.ActiveState) =
        let res =
            proj.ProjectData.ProjectReferences
            |> Seq.map (fun r -> r.Include)

        Assert.That(res, ContainsConstraint(ref))

    let hasFile (file : string) (proj : Forge.ProjectManager.ActiveState) =
        let res = proj.ProjectData.SourceFiles.Files

        Assert.That(res, ContainsConstraint(file))

    let hasName (name : string) (proj : Forge.ProjectManager.ActiveState) =
        let res = proj.ProjectData.Settings.Name.Data.Value

        Assert.That(res, EqualConstraint(name))

    let notReference (ref : string) (proj : Forge.ProjectManager.ActiveState) =
        let res =
            proj.ProjectData.References
            |> Seq.map (fun r -> r.Include)

        Assert.That(res, NotConstraint <| ContainsConstraint(ref))

    let notReferenceProject (ref : string) (proj : Forge.ProjectManager.ActiveState) =
        let res =
            proj.ProjectData.ProjectReferences
            |> Seq.map (fun r -> r.Include)

        Assert.That(res, NotConstraint <| ContainsConstraint(ref))

    let hasNotFile (file : string) (proj : Forge.ProjectManager.ActiveState) =
        let res = proj.ProjectData.SourceFiles.Files

        Assert.That(res, NotConstraint <| ContainsConstraint(file))

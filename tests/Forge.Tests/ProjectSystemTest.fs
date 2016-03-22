[<NUnit.Framework.TestFixture>]
[<NUnit.Framework.Category "ProjectSystem">]
module Forge.Tests.ProjectSystem
open System.Diagnostics
open Forge
open Forge.Tests.Common
open Forge.ProjectSystem
open NUnit.Framework
open FsUnit

[<TestFixture>]
module ``ProjectSystem Tests`` =

    [<Test>]
    let ``ProjectSystem parse - AST gets all project files`` () =
        let projectFile = FsProject.parse astInput
        System.Diagnostics.Debug.WriteLine projectFile
        projectFile.SourceFiles.AllFiles() |> Seq.length |> should be (equal 3)

    [<Test>]
    let ``ProjectSystem parse - AST gets all references`` () =
        let projectFile = FsProject.parse astInput
        projectFile.References |> Seq.length|> should be (equal 5)

    [<Test>]
    let ``ProjectSystem parse - AST gets correct settings`` () =
        let projectFile = FsProject.parse astInput
        let s = projectFile.Settings
        s.Configuration.Data |> should be (equal ^ Some "Debug")
        s.Platform.Data |> should be (equal ^ Some "AnyCPU")
        s.SchemaVersion.Data |> should be (equal ^ Some "2.0")
        s.ProjectGuid.Data |> should be (equal ^ Some ^ System.Guid.Parse "fbaf8c7b-4eda-493a-a7fe-4db25d15736f")
        s.OutputType.Data |> should be (equal ^ Some OutputType.Library)
        s.TargetFrameworkVersion.Data |> should be (equal ^ Some "v4.5")
        s.AssemblyName.Data |> should be (equal ^ Some "Test")

    [<Test>]
    let ``ProjectSystem - add new file``() =
        let pf = FsProject.parse astInput
        let f = {SourceFile.Include = "Test.fsi"; Condition = None; OnBuild = BuildAction.Compile; Link = None; Copy = None}
        let pf' = FsProject.addSourceFile "/" f pf
        let files = pf'.SourceFiles.AllFiles()
        TestContext.WriteLine  (sprintf "%A" files)
        pf'.SourceFiles.AllFiles() |> Seq.length |> should be (equal 4)

    [<Test>]
    let ``ProjectSystem - add duplicate file``() =
        let pf = FsProject.parse astInput
        let f = {SourceFile.Include = "FixProject.fs"; Condition = None; OnBuild = BuildAction.Compile; Link = None; Copy = None}
        let pf' = FsProject.addSourceFile "/" f pf
        let files = pf'.SourceFiles.AllFiles()
        TestContext.WriteLine (sprintf "%A" files)
        pf'.SourceFiles.AllFiles() |> Seq.length |> should be (equal 3)

    [<Test>]
    let ``ProjectSystem - remove file``() =
        let pf = FsProject.parse astInput
        let f = "FixProject.fs"
        let pf' = FsProject.removeSourceFile f pf
        pf'.SourceFiles.AllFiles() |> Seq.length |> should be (equal 2)

    [<Test>]
    let ``ProjectSystem - remove not existing file``() =
        let pf = FsProject.parse astInput
        let f = "FixProject2.fs"
        let pf' = FsProject.removeSourceFile f pf
        pf'.SourceFiles.AllFiles() |> Seq.length |> should be (equal 3)

    [<Test>]
    let ``ProjectSystem - order file``() =
        let pf = FsProject.parse astInput
        let pf' = pf |> FsProject.moveUp "a_file.fs" |> FsProject.moveUp "a_file.fs" 
        let files = pf'.SourceFiles.AllFiles()
        files |> Seq.head |> should be (equal "a_file.fs")
        files |> Seq.length |> should be (equal 3)

    [<Test>]
    let ``ProjectSystem - add reference``() =
        let pf = FsProject.parse astInput
        let r = {Reference.Empty with Include = "System.Xml"}
        let pf' = FsProject.addReference r pf
        pf'.References |> Seq.length |> should be (equal 6)

    [<Test>]
    let ``ProjectSystem - add existing reference``() =
        let pf = FsProject.parse astInput
        let r = {Reference.Empty with Include = "System"}
        let pf' = FsProject.addReference r pf
        pf'.References |> Seq.length |> should be (equal 5)

    [<Test>]
    let ``ProjectSystem - remove reference``() =
        let pf = FsProject.parse astInput
        let r = {Reference.Empty with Include = "System"}
        let pf' = FsProject.removeReference r pf
        pf'.References |> Seq.length |> should be (equal 4)

    [<Test>]
    let ``ProjectSystem - remove not existing reference``() =
        let pf = FsProject.parse astInput
        let r = {Reference.Empty with Include = "System.Xml"}
        let pf' = FsProject.removeReference r pf
        pf'.References |> Seq.length |> should be (equal 5)

    [<Test>]
    let ``ProjectSystem - rename project``() =
        let pf = FsProject.parse astInput
        let pf' = FsProject.renameProject "TestRename" pf
        let s = pf'.Settings
        let df = pf'.BuildConfigs |> List.pick (fun cfg -> cfg.Documentationfile.Data) |> Some
        s.AssemblyName.Data |> should be (equal ^ Some "TestRename")
        s.RootNamespace.Data |> should be (equal ^ Some "TestRename")
        df |> should be (equal ^ Some "TestRename")

    // TODO: complete test code
    [<Test>]
    let ``ProjectSystem - move up``() =
        true |> should equal true

    [<Test>]
    let ``ProjectSystem - move down``() =
        true |> should equal true

    [<Test>]
    let ``ProjectSystem - add above (?)``() =
        true |> should equal true

    [<Test>]
    let ``ProjectSystem - add below (?)``() =
        true |> should equal true

    [<Test>]
    let ``ProjectSystem - add dir``() =
        true |> should equal true

    [<Test>]
    let ``ProjectSystem - remove dir``() =
        true |> should equal true

    [<Test>]
    let ``ProjectSystem - rename file``() =
        true |> should equal true

    [<Test>]
    let ``ProjectSystem - rename dir``() =
        true |> should equal true


module ``PathHelper Tests`` =

    // TODO: complete test code
    [<Test>]
    let ``PathHelper - normalize file name``() =
        true |> should equal true


    let ``PathHelper - get root``() =
        true |> should equal true


    let ``PathHelper - path is directory``() =
        true |> should equal true


    let ``PathHelper - get parent dir from path``() =
        true |> should equal true


    let ``PathHelper - remove parent dir``() =
        true |> should equal true


    let ``PathHelper - remove root``() =
        true |> should equal true


    let ``PathHelper - fixDir: add trailing slash if missing``() =
        true |> should equal true


    let ``PathHelper - dirOrder (?)``() =
        true |> should equal true

    let ``PathHelper - treeOrder (?)``() =
        true |> should equal true


    let ``PathHelper - checkFile (?)``() =
        true |> should equal true

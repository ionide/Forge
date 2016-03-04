[<NUnit.Framework.TestFixture>]
module Forge.Tests.ProjectSystem
open System.Diagnostics
open Forge
open Forge.Tests.Common
open Forge.ProjectSystem
open NUnit.Framework
open FsUnit

[<Test>]
let ``ProjectSystem parse - AST gets all project files`` () =
    let projectFile = FsProject.parse astInput
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
    let r = {Reference.Include = "System.Xml"; Name = None; Condition = None; HintPath = None; SpecificVersion = None; CopyLocal = None}
    let pf' = FsProject.addReference r pf
    pf'.References |> Seq.length |> should be (equal 6)

[<Test>]
let ``ProjectSystem - add existing reference``() =
    let pf = FsProject.parse astInput
    let r = {Reference.Include = "System"; Name = None; Condition = None; HintPath = None; SpecificVersion = None; CopyLocal = None}
    let pf' = FsProject.addReference r pf
    pf'.References |> Seq.length |> should be (equal 5)

[<Test>]
let ``ProjectSystem - remove reference``() =
    let pf = FsProject.parse astInput
    let r = {Reference.Include = "System"; Name = None; Condition = None; HintPath = None; SpecificVersion = None; CopyLocal = None}
    let pf' = FsProject.removeReference r pf
    pf'.References |> Seq.length |> should be (equal 4)

[<Test>]
let ``ProjectSystem - remove not existing reference``() =
    let pf = FsProject.parse astInput
    let r = {Reference.Include = "System.Xml"; Name = None; Condition = None; HintPath = None; SpecificVersion = None; CopyLocal = None}
    let pf' = FsProject.removeReference r pf
    pf'.References |> Seq.length |> should be (equal 5)

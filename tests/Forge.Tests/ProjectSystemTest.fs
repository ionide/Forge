[<NUnit.Framework.TestFixture>]
[<NUnit.Framework.Category "ProjectSystem">]

module Forge.Tests.ProjectSystem

open System.IO
open System.Diagnostics
open Forge
open Forge.Tests.Common
open Forge.ProjectSystem
open NUnit.Framework
open FsUnit


[<Test>]
let ``ProjectSystem parse - AST gets all project files`` () =
    let projectFile = FsProject.parse astInput
    System.Diagnostics.Debug.WriteLine projectFile
    projectFile.SourceFiles.AllFiles() |> Seq.length |> should be (equal 3)



[<Test>]
let ``ProjectSystem parse - parse project files with nested folders and linked files``() =
    let pf = FsProject.parse projectWithLinkedFiles

    pf.SourceFiles.AllFiles() |> Seq.length |> should be (equal 8)
    pf.SourceFiles.Tree.["/"] |> should be (equivalent ["fixproject.fs"; "app.config"; "a_file.fs"; "foo/"; "fldr/"; "linked/"])
    pf.SourceFiles.Tree.["foo/"] |> should be (equivalent ["bar/"; "abc/"])
    pf.SourceFiles.Tree.["linked/"] |> should be (equivalent ["ext/"])
    pf.SourceFiles.Tree.["linked/ext/"] |> should be (equivalent ["external.fs"])
    pf.SourceFiles.Data.["linked/ext/external.fs"].Include |> should be (equal "../foo/external.fs")

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
let ``ProjectSystem - rename physical folder``() =
    let pf = FsProject.parse projectWithLinkedFiles
    let tree = pf.SourceFiles.Tree.["foo/"]
    let pf' = FsProject.renameDir "foo" "foo-renamed" pf
    pf'.SourceFiles.Tree.["foo-renamed/"] |> should be (equal tree)
    
[<Test>]
let ``ProjectSystem - rename folder with linked file``() =
    let pf = FsProject.parse projectWithLinkedFiles
    printfn "%+A" pf.SourceFiles.Tree
    printfn "%+A" pf.SourceFiles.Data
    let originalTree = pf.SourceFiles.Tree.["linked/ext/"]
    let originalFile = pf.SourceFiles.Data.["linked/ext/external.fs"]

    let pf' = FsProject.renameDir "linked/ext" "linked/ext-renamed" pf
    pf'.SourceFiles.Tree.["linked/ext-renamed/"] |> should be (equal originalTree)

    let renamedFile = pf'.SourceFiles.Data.["linked/ext-renamed/external.fs"]
    renamedFile.Include |> should be (equal originalFile.Include)
    renamedFile.Link |> should be (equal ^ Some "linked/ext-renamed/external.fs")


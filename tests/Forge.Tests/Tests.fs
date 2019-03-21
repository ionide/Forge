module Tests

open System
open Forge
open Forge.ProjectSystem
open Forge.SolutionSystem
open Common

open Expecto

[<Tests>]
let tests =
  testList "Unit Tests" [
    testList "Prelude" [
      testCase "relative - file target in different path than directory source" <| fun _ ->
        let relativePath = relative "test/Test/Test.fsproj" "src/Sample/"
        "should be OK" |> Expect.equal relativePath (".." </> ".." </> "test" </> "Test" </> "Test.fsproj")

      testCase "relative - file target in different path than file source" <| fun _ ->
        let relativePath = relative "test/Test/Test.fsproj" "src/Sample/Sample.fsproj"
        "should be OK" |> Expect.equal relativePath (".." </> ".." </> "test" </> "Test" </> "Test.fsproj")

      testCase "relative - directory target in different path than directory source" <| fun _ ->
        let relativePath = relative "test/Test" "src/Sample/"
        "should be OK" |> Expect.equal relativePath (".." </> ".." </> "test" </> "Test")

      testCase "relative - directory target in different path than file source" <| fun _ ->
        let relativePath = relative "test/Test" "src/Sample/Sample.fsproj"
        "should be OK" |> Expect.equal relativePath (".." </> ".." </> "test" </> "Test")

      testCase "relative - file target with shared ancestor as directory source" <| fun _ ->
        let relativePath = relative "test/Test" "src/Sample/Sample.fsproj"
        "should be OK" |> Expect.equal relativePath (".." </> ".." </> "test" </> "Test")

      testCase "relative - file target with shared ancestor as directory source 2" <| fun _ ->
        let relativePath = relative "src/Test/Test.fsproj" "src/Sample/"
        "should be OK" |> Expect.equal relativePath (".." </> "Test" </> "Test.fsproj")

      testCase "relative - file target with shared ancestor as file source" <| fun _ ->
        let relativePath = relative "src/Test/Test.fsproj" "src/Sample/Sample.fsproj"
        "should be OK" |> Expect.equal relativePath (".." </> "Test" </> "Test.fsproj")

      testCase "relative - directory target with shared ancestor as directory source" <| fun _ ->
        let relativePath = relative "src/Test" "src/Sample/"
        "should be OK" |> Expect.equal relativePath (".." </> "Test")

      testCase "relative - directory target with shared ancestor as file source" <| fun _ ->
        let relativePath = relative "src/Test" "src/Sample/Sample.fsproj"
        "should be OK" |> Expect.equal relativePath (".." </> "Test")

      testCase "relative - irectory target equal to directory source" <| fun _ ->
        let relativePath = relative "src/Test" "src/Test"
        "should be OK" |> Expect.equal relativePath ""

      testCase "relative - file target equal to file source" <| fun _ ->
        let relativePath = relative "src/Test/Test.fsproj" "src/Test/Test.fsproj"
        "should be OK" |> Expect.equal relativePath ""
    ]
    testList "Project System" [
      testCase "parse - AST gets all project files" <| fun _ ->
        let projectFile = FsProject.parse astInput
        projectFile.SourceFiles.AllFiles() |> Expect.hasLength 3

      testCase "parse - parse project files with nested folders and linked files" <| fun _ ->
        let pf = FsProject.parse projectWithLinkedFiles

        pf.SourceFiles.AllFiles() |> Expect.hasLength 8
        pf.SourceFiles.Tree.["/"] |> Expect.equivalent ["FixProject.fs"; "App.config"; "a_file.fs"; "foo/"; "fldr/"; "linked/"]
        pf.SourceFiles.Tree.["foo/"] |> Expect.equivalent ["bar/"; "abc/"]
        pf.SourceFiles.Tree.["linked/"] |> Expect.equivalent ["ext/"]
        pf.SourceFiles.Tree.["linked/ext/"] |> Expect.equivalent ["external.fs"]
        Expect.equal pf.SourceFiles.Data.["linked/ext/external.fs"].Include  "../foo/external.fs" "should be same"

      testCase "parse - AST gets all references" <| fun _ ->
        let projectFile = FsProject.parse astInput
        projectFile.References |> Expect.hasLength 5

      testCase "parse - AST gets correct settings" <| fun _ ->
        let projectFile = FsProject.parse astInput
        let s = projectFile.Settings
        Expect.equal s.Configuration.Data (Some "Debug") "should be same"
        Expect.equal s.Platform.Data (Some "AnyCPU") "should be same"
        Expect.equal s.SchemaVersion.Data (Some "2.0") "should be same"
        Expect.equal s.ProjectGuid.Data (Some ^ System.Guid.Parse "fbaf8c7b-4eda-493a-a7fe-4db25d15736f") "should be same"
        Expect.equal s.OutputType.Data (Some OutputType.Library) "should be same"
        Expect.equal s.TargetFrameworkVersion.Data (Some "v4.5") "should be same"
        Expect.equal s.AssemblyName.Data (Some "Test") "should be same"
        Expect.equal s.DocumentationFile.Data (Some "bin\Debug\Test.XML") "should be same"

      testCase "parse - gets all multi target frameworks" <| fun _ ->
        let projectFile = FsProject.parse netCoreProjectMultiTargetsNoFiles
        let s = projectFile.Settings
        Expect.equal s.TargetFrameworks.Data (Some ["net461"; "netstandard2.0"; "netcoreapp2.0"]) "should be same"

      testCase "parse - ToXElem sets all multi target frameworks" <| fun _ ->
        let projectFile = FsProject.parse netCoreProjectMultiTargetsNoFiles
        let s = projectFile.Settings
        let settingsXml = projectFile.Settings.ToXElem()
        let targetFrameworks = settingsXml.Element (Xml.Linq.XName.Get "TargetFrameworks")
        Expect.equal targetFrameworks.Value "net461;netstandard2.0;netcoreapp2.0" "should be same"

      testCase "parse - add new file" <| fun _ ->
        let pf = FsProject.parse astInput
        let f = {SourceFile.Include = "Test.fsi"; Condition = None; OnBuild = BuildAction.Compile; Link = None; Copy = None; Paket = None; CustomAttributes = Seq.empty; CustomElements  = Seq.empty}
        let pf' = FsProject.addSourceFile "/" f pf
        pf'.SourceFiles.AllFiles() |> Expect.hasLength 4

      testCase "parse - add duplicate file" <| fun _ ->
        let pf = FsProject.parse astInput
        let f = {SourceFile.Include = "FixProject.fs"; Condition = None; OnBuild = BuildAction.Compile; Link = None; Copy = None; Paket = None; CustomAttributes = Seq.empty; CustomElements  = Seq.empty}
        let pf' = FsProject.addSourceFile "/" f pf
        pf'.SourceFiles.AllFiles() |> Expect.hasLength 3

      testCase "parse - remove file" <| fun _ ->
        let pf = FsProject.parse astInput
        let f = "FixProject.fs"
        let pf' = FsProject.removeSourceFile f pf
        pf'.SourceFiles.AllFiles() |> Expect.hasLength 2

      testCase "parse - remove not existing file" <| fun _ ->
        let pf = FsProject.parse astInput
        let f = "FixProject2.fs"
        let pf' = FsProject.removeSourceFile f pf
        pf'.SourceFiles.AllFiles() |> Expect.hasLength 3

      testCase "parse - order file" <| fun _ ->
        let pf = FsProject.parse astInput
        let pf' = pf |> FsProject.moveUp "a_file.fs" |> FsProject.moveUp "a_file.fs"
        let files = pf'.SourceFiles.AllFiles()
        Expect.equal (files |> Seq.head) "a_file.fs" "should be equal"
        files |> Expect.hasLength 3

      testCase "parse - add reference" <| fun _ ->
        let pf = FsProject.parse astInput
        let r = {Reference.Empty with Include = "System"}
        let pf' = FsProject.addReference r pf
        pf'.References |> Expect.hasLength 5

      testCase "parse - remove referenc" <| fun _ ->
        let pf = FsProject.parse astInput
        let r = {Reference.Empty with Include = "System"}
        let pf' = FsProject.removeReference r pf
        pf'.References |> Expect.hasLength 4

      testCase "parse - remove not existing reference" <| fun _ ->
        let pf = FsProject.parse astInput
        let r = {Reference.Empty with Include = "System.Xml"}
        let pf' = FsProject.removeReference r pf
        pf'.References |> Expect.hasLength 5

      testCase "parse - rename project" <| fun _ ->
        let pf = FsProject.parse astInput
        let pf' = FsProject.renameProject "TestRename" pf
        let s = pf'.Settings
        Expect.equal s.AssemblyName.Data (Some "TestRename") "should be same"
        Expect.equal s.RootNamespace.Data (Some "TestRename") "should be same"
        Expect.equal s.DocumentationFile.Data (Some "bin\Debug\TestRename.XML") "should be same"

      testCase "parse - rename file" <| fun _ ->
        let pf = FsProject.parse astInput
        let pf' = pf |> FsProject.renameFile "FixProject.fs" "renamed_file.fs"
        let files = pf'.SourceFiles.AllFiles()
        Expect.equal (files |> Seq.head) "renamed_file.fs" "should be same"
        files |> Expect.hasLength 3

      testCase "parse - rename file invalid name" <| fun _ ->
        if System.IO.Path.GetInvalidFileNameChars().Length > 0 then
            let invalid = System.IO.Path.GetInvalidFileNameChars().[0].ToString()
            let pf = FsProject.parse astInput
            let pf' = pf |> FsProject.renameFile "FixProject.fs" ("invalid" + invalid + ".fs")
            let files = pf'.SourceFiles.AllFiles()
            Expect.equal (files |> Seq.head) "FixProject.fs" "should be same"
            files |> Expect.hasLength 3

      testCase "parse - move up" <| fun _ ->
        let pf = FsProject.parse astInput
        let pf' = pf |> FsProject.moveUp "a_file.fs"
        let files = pf'.SourceFiles.AllFiles()
        Expect.equal (files |> Seq.last) "App.config" "should be same"
        files |> Expect.hasLength 3

      testCase "parse - move down" <| fun _ ->
        let pf = FsProject.parse astInput
        let pf' = pf |> FsProject.moveDown "FixProject.fs"
        let files = pf'.SourceFiles.AllFiles()
        Expect.equal (files |> Seq.head) "App.config" "should be same"
        files |> Expect.hasLength 3

      testCase "parse - add above" <| fun _ ->
        let pf = FsProject.parse astInput
        let f = {SourceFile.Include = "above.fs"; Condition = None; OnBuild = BuildAction.Compile; Link = None; Copy = None; Paket = None; CustomAttributes = Seq.empty; CustomElements  = Seq.empty}
        let pf' = FsProject.addAbove "FixProject.fs" f pf
        let files = pf'.SourceFiles.AllFiles()
        Expect.equal (files |> Seq.head) "above.fs" "should be same"
        pf'.SourceFiles.AllFiles() |> Expect.hasLength 4

      testCase "parse - add below" <| fun _ ->
        let pf = FsProject.parse astInput
        let f = {SourceFile.Include = "below.fs"; Condition = None; OnBuild = BuildAction.Compile; Link = None; Copy = None; Paket = None; CustomAttributes = Seq.empty; CustomElements  = Seq.empty}
        let pf' = FsProject.addBelow "FixProject.fs" f pf
        let files = pf'.SourceFiles.AllFiles()
        Expect.equal (files |> Seq.item 1) "below.fs" "should be same"
        pf'.SourceFiles.AllFiles() |> Expect.hasLength 4


      testCase "parse - custom elements and attributes" <| fun _ ->
        let pf = FsProject.parse projectWithCustomProperties
        pf.Settings.CustomProperties |> Expect.hasLength 1
        Expect.exists pf.Settings.CustomProperties (fun n -> n.Name = "ServerGarbageCollection" && n.Data = Some "true") "should contain ServerGarbageCollection"
        pf.Settings.OutputType.CustomAttributes |> Expect.hasLength 1
        Expect.exists pf.Settings.OutputType.CustomAttributes (fun n -> n.Name.LocalName = "Test" && n.Value = "ABC") "should contain custom atrribute"
    ]

    testList "SolutionSystem" [
      testCase "addFolder - add folder to default solution" <| fun _ ->
        let folderName = "newFolder"
        let solution = Solution.Default |> Solution.addFolder folderName
        solution.Folders |> Expect.hasLength 1

      testCase "addFolder - adding duplicated folder fails" <| fun _ ->
        let folderName = "newFolder"
        Expect.throws (fun () ->
          Solution.Default
          |> Solution.addFolder folderName
          |> Solution.addFolder folderName
          |> ignore) "should throw"

      testCase "addFolder - add folder with same name as a project fails" <| fun _ ->
        let projectName = "existingProject"
        let slnProj =
                {
                    ProjectTypeGuid = Guid.NewGuid()
                    Guid = Guid.NewGuid()
                    Name = projectName
                    Path = "projectPath"
                    Dependecies = []
                }
        let solution = { Solution.Default with Projects=[slnProj]}
        Expect.throws (fun () -> solution |> Solution.addFolder projectName |> ignore) "should throw"

      testCase "removeFolder - remove existing folder" <| fun _ ->
        let folderName = "aFolder"
        let solution = Solution.Default |> Solution.addFolder folderName
        let solution' = solution |> Solution.removeFolder folderName
        solution'.Folders |> Expect.hasLength 0

      testCase "removeFolder - remove folder in NestedProjects" <| fun _ ->
        let projectGuid = Guid.NewGuid()
        let folderName = "folderName"
        let slnProj =
                {
                    ProjectTypeGuid = projectGuid
                    Guid = Guid.NewGuid()
                    Name = "existingProject"
                    Path = "projectPath"
                    Dependecies = []
                }

        let folderGuid = Guid.NewGuid()
        let slnFolder =
            {
                ProjectTypeGuid = FolderGuid
                Name  = folderName
                Path  = "folderPath"
                Guid  = folderGuid
                SolutionItems = []
            }
        let slnNestedProject = {Project = projectGuid; Parent = folderGuid}

        let solution = { Solution.Default with Projects=[slnProj]; NestedProjects=[slnNestedProject]; Folders=[slnFolder] }

        let solution' = solution |> Solution.removeFolder folderName
        solution'.NestedProjects  |> Expect.hasLength 0

      testCase "removeFolder - remove unexisting project" <| fun _ ->
        let solution = Solution.Default |> Solution.addFolder "aFolder"
        Expect.throws (fun () -> solution |> Solution.removeFolder "anotherFolder" |> ignore) "should throw"
    ]
  ]
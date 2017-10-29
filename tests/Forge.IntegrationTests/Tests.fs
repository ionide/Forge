module Tests

open Expecto

[<Tests>]
let tests =
  testSequenced <| testList "Integration test" [
    testList "New file" [
      testCase "Create new file giving path to fsproj" <| fun _ ->
        let dir = "new_file - path to fsproj"
        ["new project -n Sample --dir src -t console --no-paket"
         "new file -n src/Sample/Test --project src/Sample/Sample.fsproj --template fs"
        ]
        |> initTest dir
        let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject
        project |> Expect.hasFile "Test.fs"

      testCase "Create new file without giving project name" <| fun _ ->
        let dir = "new_file - no path to project"
        ["new project -n Sample --dir src -t console --no-paket"
         "new file -n src/Sample/Test --template fs"
        ]
        |> initTest dir
        let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject
        project |> Expect.hasFile "Test.fs"

      testCase "Create new file giving wrong project name" <| fun _ ->
        let dir = "new_file - wrong path to project"
        ["new project -n Sample --dir src -t console --no-paket"
         "new file -n src/Sample/Test --project ABC --template fs"
        ]
        |> initTest dir
        let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject
        project |> Expect.hasFile "Test.fs"

      testCase "Create new file with copy-to-output" <| fun _ ->
        let dir = "new_file - copy"
        ["new project -n Sample --dir src -t console --no-paket"
         "new file -n src/Sample/Test --project src/Sample/Sample.fsproj --template fs --copy-to-output never"
        ]
        |> initTest dir
        let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject
        project |> Expect.hasFile "Test.fs"
    ]
    testList "References" [
      testCase "Add Reference" <| fun _ ->
        let dir = "references_add_ref"
        [ "new project -n Sample --dir src -t console --no-paket"
          "add reference -n System.Speech -p src/Sample/Sample.fsproj" ]
        |> initTest dir
        let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject
        project |> Expect.reference "System.Speech"

      testCase "Remove Reference" <| fun _ ->
        let dir = "references_remove_ref"
        [ "new project -n Sample --dir src -t console --no-paket"
          "add reference -n System.Speech -p src/Sample/Sample.fsproj"
          "remove reference -n System.Speech -p src/Sample/Sample.fsproj" ]
        |> initTest dir
        let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject
        project |> Expect.notReference "System.Speech"

      testCase "Add Reference - absolute path" <| fun _ ->
        let dir = "references_add_ref_Absolute_path" |> makeAbsolute
        let projectPath =   dir </> "src" </> "Sample" </> "Sample.fsproj"
        [ "new project -n Sample --dir src -t console --no-paket"
          "add reference -n System.Speech -p " + projectPath]
        |> initTest dir
        let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject
        project |> Expect.reference "System.Speech"

      testCase "Remove Reference - absolute path" <| fun _ ->
        let dir = "references_remove_ref_Absolute_path" |> makeAbsolute
        let projectPath =   dir </> "src" </> "Sample" </> "Sample.fsproj"
        [ "new project -n Sample --dir src -t console --no-paket"
          "add reference -n System.Speech -p " + projectPath
          "remove reference -n System.Speech -p " + projectPath ]
        |> initTest dir
        let project = projectPath |> loadProject
        project |> Expect.notReference "System.Speech"
    ]
    testList "Project Reference" [
      testCase "Add Project Reference" <| fun _ ->
        let dir = "project_references_add_ref"
        [ "new project -n Sample --dir src -t console --no-paket"
          "new project -n Test --dir test -t expecto --no-paket"
          "add project -p test/Test/Test.fsproj -n src/Sample/Sample.fsproj" ]
        |> initTest dir
        let project = dir </> "test" </> "Test" </> "Test.fsproj" |> loadProject
        project |> Expect.referenceProject  (".." </> ".." </> "src" </> "Sample" </> "Sample.fsproj")

      testCase "Remove Project Reference" <| fun _ ->
        let dir = "project_references_remove_ref"
        [ "new project -n Sample --dir src -t console --no-paket"
          "new project -n Test --dir test -t expecto --no-paket"
          "add project -p test/Test/Test.fsproj -n src/Sample/Sample.fsproj"
          "remove project -p test/Test/Test.fsproj -n src/Sample/Sample.fsproj" ]
        |> initTest dir
        let project = dir </> "test" </> "Test" </> "Test.fsproj" |> loadProject
        project |> Expect.notReferenceProject (".." </> ".." </> "src" </> "Sample" </> "Sample.fsproj")

      testCase "Add Project Reference - absolute path" <| fun _ ->
        let dir = "project_references_add_ref_Absolute_path" |> makeAbsolute
        let path1 = dir </> "test" </> "Test" </> "Test.fsproj"
        let path2 = dir </> "src" </> "Sample" </> "Sample.fsproj"
        [ "new project -n Sample --dir src -t console --no-paket"
          "new project -n Test --dir test -t expecto --no-paket"
          sprintf "add project -p %s -n %s" path1 path2 ]
        |> initTest dir
        let project = path1 |> loadProject
        project |> Expect.referenceProject  (".." </> ".." </> "src" </> "Sample" </> "Sample.fsproj")

      testCase "Remove Project Reference - absolute path" <| fun _ ->
        let dir = "project_references_remove_ref_Absolute_path" |> makeAbsolute
        let path1 = dir </> "test" </> "Test" </> "Test.fsproj"
        let path2 = dir </> "src" </> "Sample" </> "Sample.fsproj"
        [ "new project -n Sample --dir src -t console --no-paket"
          "new project -n Test --dir test -t expecto --no-paket"
          sprintf "add project -p %s -n %s" path1 path2
          sprintf "remove project -p %s -n %s" path1 path2  ]
        |> initTest dir
        let project = path1 |> loadProject
        project |> Expect.notReferenceProject  (".." </> ".." </> "src" </> "Sample" </> "Sample.fsproj")
    ]
    testList "Add file" [
      testCase "Add File" <| fun _ ->
        let dir = "file_add_file"
        [ "new project -n Sample --dir src -t console --no-paket"
          "add file -n src/Sample/Test.fs" ]
        |> initTest dir
        let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject
        project |> Expect.hasFile "Test.fs"

      testCase "Add File - with project" <| fun _ ->
        let dir = "file_add_file_project"
        [ "new project -n Sample --dir src -t console --no-paket"
          "add file -p src/Sample/Sample.fsproj -n src/Sample/Test.fs " ]
        |> initTest dir
        let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject
        project |> Expect.hasFile "Test.fs"

      testCase "Add File - absolute path" <| fun _ ->
        let dir = "file_add_file_absolute_path" |> makeAbsolute
        let p = dir </> "src" </> "Sample" </> "Test.fs"
        [ "new project -n Sample --dir src -t console --no-paket"
          sprintf "add file -n %s" p ]
        |> initTest dir
        let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject
        project |> Expect.hasFile "Test.fs"

      testCase "Add File - with project, absolute path" <| fun _ ->
        let dir = "file_add_file_project_absolute_path" |> makeAbsolute
        let p =   dir </> "src" </> "Sample" </> "Test.fs"
        let projectPath = dir </> "src" </> "Sample" </> "Sample.fsproj"
        [ "new project -n Sample --dir src -t console --no-paket"
          sprintf "add file -p %s -n %s " projectPath p ]
        |> initTest dir
        let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject
        project |> Expect.hasFile "Test.fs"

      testCase "Add File - with project, absolute path, above" <| fun _ ->
        let dir = "file_add_file_project_absolute_path_above" |> makeAbsolute
        let p =   dir </> "src" </> "Sample" </> "Test.fs"
        let projectPath = dir </> "src" </> "Sample" </> "Sample.fsproj"
        [ "new project -n Sample --dir src -t console --no-paket"
          sprintf "add file -p %s -n %s --above %s " projectPath p "Sample.fs" ]
        |> initTest dir
        let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject
        project |> Expect.hasFile "Test.fs"

      testCase "Add File - with project, absolute path, below" <| fun _ ->
        let dir = "file_add_file_project_absolute_path_below" |> makeAbsolute
        let p =   dir </> "src" </> "Sample" </> "Test.fs"
        let projectPath = dir </> "src" </> "Sample" </> "Sample.fsproj"
        [ "new project -n Sample --dir src -t console --no-paket"
          sprintf "add file -p %s -n %s --below %s " projectPath p "Sample.fs" ]
        |> initTest dir
        let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject
        project |> Expect.hasFile "Test.fs"

      testCase "Add File - with project, absolute path, copy-to-output" <| fun _ ->
        let dir = "file_add_file_project_absolute_path_copy" |> makeAbsolute
        let p =   dir </> "src" </> "Sample" </> "Test.fs"
        let projectPath = dir </> "src" </> "Sample" </> "Sample.fsproj"
        [ "new project -n Sample --dir src -t console --no-paket"
          sprintf "add file -p %s -n %s --copy-to-output always" projectPath p ]
        |> initTest dir
        let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject
        project |> Expect.hasFile "Test.fs"
    ]
    testList "Remove file" [
      testCase "Remove File" <| fun _ ->
        let dir = "file_remove_file"
        [ "new project -n Sample --dir src -t console --no-paket"
          "remove file -n src/Sample/Sample.fs" ]
        |> initTest dir
        let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject
        project |> Expect.hasNotFile "Sample.fs"

      testCase "Remove File - with project" <| fun _ ->
        let dir = "file_remove_file_project"
        [ "new project -n Sample --dir src -t console --no-paket"
          "remove file -p src/Sample/Sample.fsproj -n src/Sample/Sample.fs " ]
        |> initTest dir
        let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject
        project |> Expect.hasNotFile "Sample.fs"

      testCase "Remove File - absolute path" <| fun _ ->
        let dir = "file_remove_file_absolute_path" |> makeAbsolute
        let p =   dir </> "src" </> "Sample" </> "Sample.fs"
        [ "new project -n Sample --dir src -t console --no-paket"
          sprintf "remove file -n %s" p ]
        |> initTest dir
        let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject
        project |> Expect.hasNotFile "Sample.fs"

      testCase "Remove File - with project, absolute path" <| fun _ ->
        let dir = "file_remove_file_project_absolute_path" |> makeAbsolute
        let p =   dir </> "src" </> "Sample" </> "Sample.fs"
        let projectPath =   dir </> "src" </> "Sample" </> "Sample.fsproj"
        [ "new project -n Sample --dir src -t console --no-paket"
          sprintf "remove file -p %s -n %s " projectPath p ]
        |> initTest dir
        let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject
        project |> Expect.hasNotFile "Sample.fs"
    ]
    testList "New file" [
      testCase "Create new file giving path to fsproj" <| fun _ ->
        let dir = "new_file - path to fsproj"
        ["new project -n Sample --dir src -t console --no-paket"
         "new file -n src/Sample/Test --project src/Sample/Sample.fsproj --template fs"
        ]
        |> initTest dir
        let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject
        project |> Expect.hasFile "Test.fs"

      testCase "Create new file without giving project name" <| fun _ ->
        let dir = "new_file - no path to project"
        ["new project -n Sample --dir src -t console --no-paket"
         "new file -n src/Sample/Test --template fs"
        ]
        |> initTest dir
        let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject
        project |> Expect.hasFile "Test.fs"

      testCase "Create new file giving wrong project name" <| fun _ ->
        let dir = "new_file - wrong path to project"
        ["new project -n Sample --dir src -t console --no-paket"
         "new file -n src/Sample/Test --project ABC --template fs"
        ]
        |> initTest dir
        let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject
        project |> Expect.hasFile "Test.fs"
    ]
    testList "ProjectScaffold" [
      testCase "Create new scaffold" <| fun _ ->
        let dir = "new_scaffold"
        ["new scaffold"]
        |> initTest dir
        let path = getPath (dir </> "FSharp.ProjectScaffold.sln")
        Expect.isTrue (System.IO.File.Exists path) "should exist"

      testCase "Create new scaffold with spaces in folder" <| fun _ ->
        let dir = "new_scaffold with spaces"
        ["new scaffold"]
        |> initTest dir
        let path = getPath (dir </> "FSharp.ProjectScaffold.sln")
        Expect.isTrue (System.IO.File.Exists path) "should exist"
    ]
    testList "Rename file" [
      testCase "Rename file changes name in fsproj" <| fun _ ->
        let dir = "rename_file"
        ["new project -n Sample --dir src -t console --no-paket"
         "new file -n src/Sample/Test --project src/Sample/Sample.fsproj --template fs"
         "rename file -n src/Sample/Test.fs -r src/Sample/Renamed.fs --project src/Sample/Sample.fsproj"
        ]
        |> initTest dir
        let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject
        project |> Expect.hasFile "Renamed.fs"

      testCase "Rename file without project changes name in fsproj" <| fun _ ->
        let dir = "rename_file_no_project"
        ["new project -n Sample --dir src -t console --no-paket"
         "new file -n src/Sample/Test --project src/Sample/Sample.fsproj --template fs"
         "rename file -n src/Sample/Test.fs -r src/Sample/Renamed.fs"
        ]
        |> initTest dir
        let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject
        project |> Expect.hasFile "Renamed.fs"

      testCase "Rename file nonexistent folder" <| fun _ ->
        let dir = "rename_file_nonexistent_folder"
        ["new project -n Sample --dir src -t console --no-paket"
         "new file -n src/Sample/Test --project src/Sample/Sample.fsproj --template fs"
         "rename file -n src/Sample/Test.fs -r src/Sample/Test/Renamed.fs"
        ]
        |> initTest dir
        let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject
        project |> Expect.hasFile "Test.fs"

      testCase "Rename file existing file" <| fun _ ->
        let dir = "rename_file_existing_file"
        ["new project -n Sample --dir src -t console --no-paket"
         "new file -n src/Sample/Test --project src/Sample/Sample.fsproj --template fs"
         "rename file -n src/Sample/Test.fs -r src/Sample/Sample.fs"
        ]
        |> initTest dir
        let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject
        project |> Expect.hasFile "Test.fs"
    ]
  ]

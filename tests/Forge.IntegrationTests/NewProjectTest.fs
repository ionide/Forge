[<NUnit.Framework.TestFixture>]
[<NUnit.Framework.Category "New Project">]
module ``New Project Tests``

open NUnit.Framework
open Assertions
open FsUnit

[<Test>]
let ``Create new solution`` () =
    let dir = "new_project - create_new_solution"

    ["new solution -n Sample"]
    |> initTest dir
    let path = dir </> "Sample.sln"
    System.IO.File.Exists path |> should be True

[<Test>]
let ``Create New Console Application`` () =
    let dir = "new_project - create_new_console_application"

    ["new project -n Sample --dir src -t console"]
    |> initTest dir

    let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject

    project |> reference "mscorlib"
    project |> reference "System"
    project |> reference "System.Core"

    project |> hasFile "Sample.fs"

    project |> hasName "Sample"

[<Test>]
let ``Create New Test Application`` () =
    let dir = "new_project - create_new_test_application"

    ["new project -n Sample --dir src -t fsunit"]
    |> initTest dir

    let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject

    project |> reference "mscorlib"
    project |> reference "System"
    project |> reference "System.Core"

    project |> hasFile "Sample.fs"

    project |> hasName "Sample"
    project.ProjectData.Settings.ProjectGuid.Data.IsSome |> should equal true


[<Test>]
let ``Create Multiple Projects`` () =
    let dir = "new_project - create_multiple_projects"

    [ "new project -n Sample --dir src -t console"
      "new project -n Test --dir test -t fsunit" ]
    |> initTest dir

    let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject
    let project2 = dir </> "test" </> "Test" </> "Test.fsproj" |> loadProject

    project |> reference "mscorlib"
    project |> reference "System"
    project |> reference "System.Core"

    project |> hasFile "Sample.fs"

    project |> hasName "Sample"
    project.ProjectData.Settings.ProjectGuid.Data.IsSome |> should equal true

    project2 |> reference "mscorlib"
    project2 |> reference "System"
    project2 |> reference "System.Core"

    project2 |> hasFile "Test.fs"

    project2 |> hasName "Test"
    project2.ProjectData.Settings.ProjectGuid.Data.IsSome |> should equal true


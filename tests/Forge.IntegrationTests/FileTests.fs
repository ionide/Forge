[<NUnit.Framework.TestFixture>]
[<NUnit.Framework.Category "File">]
module ``File Tests``

open NUnit.Framework
open Assertions

[<Test>]
let ``Add File`` () =
    let dir = "file_add_file"

    [ "new project -n Sample --dir src -t console"
      "add file -n src/Sample/Test.fs" ]
    |> initTest dir

    let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject
    project |> hasFile "Test.fs"

[<Test>]
let ``Add File - with project`` () =
    let dir = "file_add_file_project"

    [ "new project -n Sample --dir src -t console"
      "add file -p src/Sample/Sample.fsproj -n src/Sample/Test.fs " ]
    |> initTest dir

    let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject
    project |> hasFile "Test.fs"

[<Test>]
let ``Add File - absolute path`` () =
    let dir = "file_add_file_absolute_path" |> makeAbsolute
    let p =   dir </> "src" </> "Sample" </> "Test.fs"

    [ "new project -n Sample --dir src -t console"
      sprintf "add file -n %s" p ]
    |> initTest dir

    let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject
    project |> hasFile "Test.fs"

[<Test>]
let ``Add File - with project, absolute path`` () =
    let dir = "file_add_file_project_absolute_path" |> makeAbsolute

    let p =   dir </> "src" </> "Sample" </> "Test.fs"
    let projectPath =   dir </> "src" </> "Sample" </> "Sample.fsproj"

    [ "new project -n Sample --dir src -t console"
      sprintf "add file -p %s -n %s " projectPath p ]
    |> initTest dir

    let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject
    project |> hasFile "Test.fs"



[<Test>]
let ``Remove File`` () =
    let dir = "file_remove_file"

    [ "new project -n Sample --dir src -t console"
      "remove file -n src/Sample/Sample.fs" ]
    |> initTest dir

    let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject
    project |> hasNotFile "Sample.fs"

[<Test>]
let ``Remove File - with project`` () =
    let dir = "file_remove_file_project"

    [ "new project -n Sample --dir src -t console"
      "remove file -p src/Sample/Sample.fsproj -n src/Sample/Sample.fs " ]
    |> initTest dir

    let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject
    project |> hasNotFile "Sample.fs"

[<Test>]
let ``Remove File - absolute path`` () =
    let dir = "file_remove_file_absolute_path" |> makeAbsolute
    let p =   dir </> "src" </> "Sample" </> "Sample.fs"

    [ "new project -n Sample --dir src -t console"
      sprintf "remove file -n %s" p ]
    |> initTest dir

    let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject
    project |> hasNotFile "Sample.fs"

[<Test>]
let ``Remove File - with project, absolute path`` () =
    let dir = "file_remove_file_project_absolute_path" |> makeAbsolute

    let p =   dir </> "src" </> "Sample" </> "Sample.fs"
    let projectPath =   dir </> "src" </> "Sample" </> "Sample.fsproj"

    [ "new project -n Sample --dir src -t console"
      sprintf "remove file -p %s -n %s " projectPath p ]
    |> initTest dir

    let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject
    project |> hasNotFile "Sample.fs"


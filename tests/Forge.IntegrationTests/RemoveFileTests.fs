[<NUnit.Framework.TestFixture>]
[<NUnit.Framework.Category "File">]
module ``Remove File Tests``

open NUnit.Framework
open Assertions



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


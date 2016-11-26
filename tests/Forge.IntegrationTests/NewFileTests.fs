[<NUnit.Framework.TestFixture>]
[<NUnit.Framework.Category "New Project">]
module ``New File Tests``

open NUnit.Framework
open Assertions
open FsUnit

[<Test>]
let ``Create new file giving path to fsproj`` () =
    let dir = "new_file - path to fsproj"

    ["new project -n Sample --dir src -t console"
     "new file -n src/Sample/Test --project src/Sample/Sample.fsproj --template fs"
    ]
    |> initTest dir

    let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject

    project |> hasFile "Test.fs"

[<Test>]
let ``Create new file without giving project name`` () =
    let dir = "new_file - no path to project"

    ["new project -n Sample --dir src -t console"
     "new file -n src/Sample/Test --template fs"
    ]
    |> initTest dir

    let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject

    project |> hasFile "Test.fs"



[<Test>]
let ``Create new file giving wrong project name`` () =
    let dir = "new_file - wrong path to project"

    ["new project -n Sample --dir src -t console"
     "new file -n src/Sample/Test --project ABC --template fs"
    ]
    |> initTest dir

    let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject

    project |> hasFile "Test.fs"
[<NUnit.Framework.TestFixture>]
[<NUnit.Framework.Category "Rename file">]
module ``Rename File Tests``

open NUnit.Framework
open Assertions
open FsUnit

[<Test>]
let ``Renaming file changes name in fsproj`` () =
    let dir = "rename_file"

    ["new project -n Sample --dir src -t console"
     "new file -n src/Sample/Test --project src/Sample/Sample.fsproj --template fs"
     "rename file -n src/Sample/Test.fs -r src/Sample/Renamed.fs --project src/Sample/Sample.fsproj"
    ]
    |> initTest dir

    let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject

    project |> hasFile "Renamed.fs"

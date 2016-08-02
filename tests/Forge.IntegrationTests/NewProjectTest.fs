[<NUnit.Framework.TestFixture>]
[<NUnit.Framework.Category "New Project">]
module ``New Project Tests``

open NUnit.Framework
open Assertions

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
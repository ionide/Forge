[<NUnit.Framework.TestFixture>]
[<NUnit.Framework.Category "Reference">]
module ``Reference Tests``

open NUnit.Framework
open Assertions

[<Test>]
let ``Add Reference`` () =
    let dir = "references_add_ref"

    [ "new project -n Sample --dir src -t console"
      "add reference -n System.Speech -p src/Sample/Sample.fsproj" ]
    |> initTest dir

    let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject
    project |> reference "System.Speech"


[<Test>]
let ``Remove Reference`` () =
    let dir = "references_remove_ref"

    [ "new project -n Sample --dir src -t console"
      "remove reference -n System.Numerics -p src/Sample/Sample.fsproj" ]
    |> initTest dir

    let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject
    project |> notReference "System.Numerics"

[<Test>]
let ``Add Reference - absolute path`` () =
    let dir = "references_add_ref_Absolute_path" |> makeAbsolute
    let projectPath =   dir </> "src" </> "Sample" </> "Sample.fsproj"

    [ "new project -n Sample --dir src -t console"
      "add reference -n System.Speech -p " + projectPath]
    |> initTest dir

    let project = dir </> "src" </> "Sample" </> "Sample.fsproj" |> loadProject
    project |> reference "System.Speech"


[<Test>]
let ``Remove Reference - absolute path`` () =
    let dir = "references_remove_ref_Absolute_path" |> makeAbsolute
    let projectPath =   dir </> "src" </> "Sample" </> "Sample.fsproj"

    [ "new project -n Sample --dir src -t console"
      "remove reference -n System.Numerics -p " + projectPath ]
    |> initTest dir

    let project = projectPath |> loadProject
    project |> notReference "System.Numerics"

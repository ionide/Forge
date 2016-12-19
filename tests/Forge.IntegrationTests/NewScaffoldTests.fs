[<NUnit.Framework.TestFixture>]
[<NUnit.Framework.Category "New Project">]
module ``New ProjectScaffold Tests``

open NUnit.Framework
open Assertions
open FsUnit

[<Test>]
let ``Create new scaffold`` () =
    let dir = "new_scaffold"

    ["new scaffold"]
    |> initTest dir

    let path = getPath (dir </> "FSharp.ProjectScaffold.sln")
    System.IO.File.Exists path |> should be True

[<Test>]
let ``Create new scaffold with spaces in folder`` () =
    let dir = "new_scaffold with spaces"

    ["new scaffold"]
    |> initTest dir

    let path = getPath (dir </> "FSharp.ProjectScaffold.sln")
    System.IO.File.Exists path |> should be True

[<NUnit.Framework.TestFixture>]
[<NUnit.Framework.Category "Project Reference">]
module ``Project References Tests``

open NUnit.Framework
open Assertions

[<Test>]
let ``Add Project Reference`` () =
    let dir = "project_references_add_ref"

    [ "new project -n Sample --dir src -t console"
      "new project -n Test --dir test -t fsunit"
      "add project -p test/Test/Test.fsproj -n src/Sample/Sample.fsproj" ]
    |> initTest dir

    let project = dir </> "test" </> "Test" </> "Test.fsproj" |> loadProject
    project |> referenceProject "..\\src\\Sample\\Sample.fsproj"


[<Test>]
let ``Remove Project Reference`` () =
    let dir = "project_references_remove_ref"

    [ "new project -n Sample --dir src -t console"
      "new project -n Test --dir test -t fsunit"
      "add project -p test/Test/Test.fsproj -n src/Sample/Sample.fsproj"
      "remove project -p test/Test/Test.fsproj -n src/Sample/Sample.fsproj"  ]
    |> initTest dir

    let project = dir </> "test" </> "Test" </> "Test.fsproj" |> loadProject
    project |> notReferenceProject "..\\src\\Sample\\Sample.fsproj"


[<Test>]
let ``Add Project Reference - absolute path`` () =
    let dir = "project_references_add_ref_Absolute_path" |> makeAbsolute
    let path1 = dir </> "test" </> "Test" </> "Test.fsproj"
    let path2 = dir </> "src" </> "Sample" </> "Sample.fsproj"

    [ "new project -n Sample --dir src -t console"
      "new project -n Test --dir test -t fsunit"
      sprintf "add project -p %s -n %s" path1 path2 ]
    |> initTest dir

    let project = path1 |> loadProject
    project |> referenceProject "..\\src\\Sample\\Sample.fsproj"


[<Test>]
let ``Remove Project Reference - absolute path`` () =
    let dir = "project_references_remove_ref_Absolute_path" |> makeAbsolute
    let path1 = dir </> "test" </> "Test" </> "Test.fsproj"
    let path2 = dir </> "src" </> "Sample" </> "Sample.fsproj"

    [ "new project -n Sample --dir src -t console"
      "new project -n Test --dir test -t fsunit"
      sprintf "add project -p %s -n %s" path1 path2
      sprintf "remove project -p %s -n %s" path1 path2  ]
    |> initTest dir

    let project = path1 |> loadProject
    project |> notReferenceProject "..\\src\\Sample\\Sample.fsproj"
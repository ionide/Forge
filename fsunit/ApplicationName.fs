module <%= namespace %>

open NUnit.Framework
open FsUnit

[<Test>]
let ``Example Test`` () =
    1 |> should equal 1

[<NUnit.Framework.TestFixture>]
[<NUnit.Framework.Category "GAC">]
module GacTests

open Forge
open Forge.Tests.Common
open NUnit.Framework
open FsUnit

[<Test>]
[<Ignore("Gives NullRefException")>]
let ``GAC search returns more than zero items`` () =
    let gacItems = GacSearch.searchGac ()
    gacItems |> Seq.length |> should be (greaterThan 0)
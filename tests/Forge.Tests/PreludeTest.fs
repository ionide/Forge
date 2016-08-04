[<NUnit.Framework.TestFixture>]
[<NUnit.Framework.Category "Prelude">]

module Forge.Tests.Prelude

open System.IO
open System.Diagnostics
open Forge
open Forge.Tests.Common
open Forge.ProjectSystem
open NUnit.Framework
open FsUnit

[<TestFixture>]
module ``Prelude Tests`` =

    [<Test>]
    let ``Prelude relative - file target in different path than directory source`` () =
        let relativePath = relative "test/Test/Test.fsproj" "src/Sample/"
        relativePath |> should be (equal (".." </> ".." </> "test" </> "Test" </> "Test.fsproj"))

    [<Test>]
    let ``Prelude relative - file target in different path than file source`` () =
        let relativePath = relative "test/Test/Test.fsproj" "src/Sample/Sample.fsproj"
        relativePath |> should be (equal (".." </> ".." </> "test" </> "Test" </> "Test.fsproj"))

    [<Test>]
    let ``Prelude relative - directory target in different path than directory source`` () =
        let relativePath = relative "test/Test" "src/Sample/"
        relativePath |> should be (equal (".." </> ".." </> "test" </> "Test"))

    [<Test>]
    let ``Prelude relative - directory target in different path than file source`` () =
        let relativePath = relative "test/Test" "src/Sample/Sample.fsproj"
        relativePath |> should be (equal (".." </> ".." </> "test" </> "Test"))
    
    [<Test>]
    let ``Prelude relative - file target with shared ancestor as directory source`` () =
        let relativePath = relative "src/Test/Test.fsproj" "src/Sample/"
        relativePath |> should be (equal (".." </> "Test" </> "Test.fsproj"))

    [<Test>]
    let ``Prelude relative - file target with shared ancestor as file source`` () =
        let relativePath = relative "src/Test/Test.fsproj" "src/Sample/Sample.fsproj"
        relativePath |> should be (equal (".." </> "Test" </> "Test.fsproj"))

    [<Test>]
    let ``Prelude relative - directory target with shared ancestor as directory source`` () =
        let relativePath = relative "src/Test" "src/Sample/"
        relativePath |> should be (equal (".." </> "Test"))

    [<Test>]
    let ``Prelude relative - directory target with shared ancestor as file source`` () =
        let relativePath = relative "src/Test" "src/Sample/Sample.fsproj"
        relativePath |> should be (equal (".." </> "Test"))

    [<Test>]
    let ``Prelude relative - directory target equal to directory source`` () =
        let relativePath = relative "src/Test" "src/Test"
        relativePath |> should be (equal "")

    [<Test>]
    let ``Prelude relative - file target equal to file source`` () =
        let relativePath = relative "src/Test/Test.fsproj" "src/Test/Test.fsproj"
        relativePath |> should be (equal "")
[<NUnit.Framework.TestFixture>]
[<NUnit.Framework.Category "ProjectManager">]
module ProjectManagerTests

open System.Diagnostics
open Forge
open Forge.Tests.Common
open Forge.ProjectSystem
open Forge.ProjectManager
open NUnit.Framework
open FsUnit

//[<Test>]
//let ``ProjectManager subsection - test a specific item`` =
//    true |> should be (equal true)
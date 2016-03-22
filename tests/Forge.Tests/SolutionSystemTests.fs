[<NUnit.Framework.TestFixture>]
[<NUnit.Framework.Category "SolutionSystem">]
module Forge.Tests.SolutionSystem
open System.Diagnostics
open Forge
open Forge.Tests.Common
open Forge.ProjectSystem
open NUnit.Framework
open FsUnit

module SolutionSystemTests = ()

//[<Test>]
//let ``SolutionSystem parse - parse GUID`` () =
//    let guid = Forge.SolutionSystem.Parsers.pGuid 
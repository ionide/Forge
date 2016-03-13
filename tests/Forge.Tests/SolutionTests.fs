[<NUnit.Framework.TestFixture>]
[<NUnit.Framework.Category "SolutionSystem">]
module Forge.Tests.Solution
open System
open Forge
open Forge.Tests.Common
open Forge.ProjectSystem
open Forge.SolutionSystem
open NUnit.Framework
open FsUnit

[<Test>]
let ``SolutionSystem Solution addFolder - add folder to default solution``() =
    let folderName = "newFolder"
    let solution = Solution.Default |> Solution.addFolder folderName
    solution.Folders |> Seq.length |> should be (equal 1)

[<Test>]
let ``SolutionSystem Solution addFolder - add existing folder fails``() =
    let folderName = "newFolder"
    (fun () ->
        Solution.Default 
        |> Solution.addFolder folderName
        |> Solution.addFolder folderName
        |> ignore) |> should throw typeof<Exception>

[<Test>]
let ``SolutionSystem Solution addFolder - add folder with same name as a project fails``() =
    let projectName = "existingProject"
    let slnProj = 
            {   
                ProjectTypeGuid = Guid.NewGuid()
                Guid = Guid.NewGuid()
                Name = projectName
                Path = "projectPath"
                Dependecies = []
            }
    let solution = { Solution.Default with Projects=[slnProj]}

    (fun() -> solution |> Solution.addFolder projectName |> ignore) |> should throw typeof<Exception>

[<Test>]
let ``SolutionSystem Solution removeFolder - remove existing folder``() =
    let folderName = "aFolder"
    let solution = Solution.Default |> Solution.addFolder folderName
    let solution' = solution |> Solution.removeFolder folderName
    
    solution'.Folders |> List.length |> should be (equal 0)

[<Test>]
let ``SolutionSystem Solution removeFolder - remove folder in NestedProjects``() =
    let projectGuid = Guid.NewGuid()
    let folderName = "folderName"
    let slnProj = 
            {   
                ProjectTypeGuid = projectGuid
                Guid = Guid.NewGuid()
                Name = "existingProject"
                Path = "projectPath"
                Dependecies = []
            }

    let folderGuid = Guid.NewGuid()
    let slnFolder = 
        {
            ProjectTypeGuid = FolderGuid
            Name  = folderName
            Path  = "folderPath"
            Guid  = folderGuid
            SolutionItems = []
        }
    let slnNestedProject = {Project = projectGuid; Parent = folderGuid}

    let solution = { Solution.Default with Projects=[slnProj]; NestedProjects=[slnNestedProject]; Folders=[slnFolder] }

    let solution' = solution |> Solution.removeFolder folderName

    solution'.NestedProjects |> List.length |> should be (equal 0)

[<Test>]
let ``SolutionSystem Solution removeFolder - remove unexisting project`` () =
    let solution = Solution.Default |> Solution.addFolder "aFolder"

    (fun() -> solution |> Solution.removeFolder "anotherFolder" |> ignore) |> should throw typeof<Exception>
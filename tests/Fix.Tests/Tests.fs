module Fix.Tests

open Fix.ProjectSystem
open FsUnit
open NUnit.Framework
open ProjectSystem


let projectWithoutFiles = """<?xml version="1.0" encoding="utf-8"?>
    <Project ToolsVersion="12.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
      <ItemGroup>
        <Reference Include="mscorlib" />
        <Reference Include="FSharp.Core, Version=$(TargetFSharpCoreVersion), Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a">
            <Private>True</Private>
        </Reference>
        <Reference Include="System" />
        <Reference Include="System.Core" />
        <Reference Include="System.Numerics" />
      </ItemGroup>
    </Project>
    """

let projectWithFiles = """<?xml version="1.0" encoding="utf-8"?>
    <Project ToolsVersion="12.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
        <ItemGroup>
            <Reference Include="mscorlib" />
            <Reference Include="FSharp.Core, Version=$(TargetFSharpCoreVersion), Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a">
              <Private>True</Private>
            </Reference>
            <Reference Include="System" />
            <Reference Include="System.Core" />
            <Reference Include="System.Numerics" />
        </ItemGroup>
        <ItemGroup>
            <Compile Include="FixProject.fs" />
            <None Include="App.config" />
            <Compile Include="a_file.fs" />
        </ItemGroup>
    </Project>
    """

[<Test>]
let ``ProjectFiles gets all project files - file count`` () =
    let projectFile = new ProjectFile("foo.fsproj", projectWithFiles)
    let files = projectFile.ProjectFiles |> Seq.length
    Assert.AreEqual(3, files)

[<Test>]
let ``Add file works even if ItemGroup not found - file count`` () =
    let projectFile = new ProjectFile("foo.fsproj", projectWithoutFiles)
    let newProject = projectFile.AddFile "file.fs" "Compile"
    let files = newProject.ProjectFiles |> Seq.length
    Assert.AreEqual(1, files)

[<Test>]
let ``Add file works even if ItemGroup not found - file name`` () =
    let projectFile = new ProjectFile("foo.fsproj", projectWithoutFiles)
    let newProject = projectFile.AddFile "file.fs" "Compile"
    let newFile = newProject.ProjectFiles |> Seq.tryHead
    Assert.AreEqual(Some "file.fs", newFile)

[<Test>]
let ``Add file works even if ItemGroup not found - file content`` () =
    let projectFile = new ProjectFile("foo.fsproj", projectWithoutFiles)
    let newProject = projectFile.AddFile "file.fs" "Compile"
    let projectContent = newProject.Content

    let expectedContent = """<?xml version="1.0" encoding="utf-16"?>
<Project ToolsVersion="12.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <ItemGroup>
    <Reference Include="mscorlib" />
    <Reference Include="FSharp.Core, Version=$(TargetFSharpCoreVersion), Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a">
      <Private>True</Private>
    </Reference>
    <Reference Include="System" />
    <Reference Include="System.Core" />
    <Reference Include="System.Numerics" />
  </ItemGroup>
  <ItemGroup>
    <Compile Include="file.fs" />
  </ItemGroup>
</Project>"""
    Assert.AreEqual(expectedContent, projectContent)

[<Test>]
let ``Remove file from Project - file count``() =
    let projectFile = new ProjectFile("foo.fsproj", projectWithFiles)
    let changedProjectFile = projectFile.RemoveFile "FixProject.fs"
    let files = changedProjectFile.ProjectFiles |> Seq.length
    Assert.AreEqual(2, files)

[<Test>]
let ``Remove file from Project - content``() =
    let projectFile = new ProjectFile("foo.fsproj", projectWithFiles)
    let changedProjectFile = projectFile.RemoveFile "FixProject.fs"
    let projectContent = changedProjectFile.Content

    let expectedContent = """<?xml version="1.0" encoding="utf-16"?>
<Project ToolsVersion="12.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <ItemGroup>
    <Reference Include="mscorlib" />
    <Reference Include="FSharp.Core, Version=$(TargetFSharpCoreVersion), Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a">
      <Private>True</Private>
    </Reference>
    <Reference Include="System" />
    <Reference Include="System.Core" />
    <Reference Include="System.Numerics" />
  </ItemGroup>
  <ItemGroup>
    <None Include="App.config" />
    <Compile Include="a_file.fs" />
  </ItemGroup>
</Project>"""
    Assert.AreEqual(expectedContent, projectContent)

[<Test>]
let ``Remove non existing file from Project - file count``() =
    let projectFile = new ProjectFile("foo.fsproj", projectWithoutFiles)
    let changedProjectFile = projectFile.RemoveFile "FixProject.fs"
    let files = changedProjectFile.ProjectFiles |> Seq.length
    Assert.AreEqual(0, files)

[<Test>]
let ``List referenced files - file count``() =
    let projectFile = new ProjectFile("foo.fsproj", projectWithoutFiles)
    let references = projectFile.References
    let referencCount = references |> Seq.length
    Assert.AreEqual(5, referencCount)

[<Test>]
let ``List referenced files - files``() =
    let projectFile = new ProjectFile("foo.fsproj", projectWithoutFiles)
    let references = projectFile.References

    references |> should contain "mscorlib"
    references |> should contain "FSharp.Core, Version=$(TargetFSharpCoreVersion), Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
    references |> should contain "System"
    references |> should contain "System.Core"
    references |> should contain "System.Numerics"

[<Test>]
let ``Add reference to project``() =
    let projectFile = new ProjectFile("foo.fsproj", projectWithoutFiles)
    let newProject = projectFile.AddReference "ref"

    newProject.References |> should contain "ref"

[<Test>]
let ``Add reference to project - only references - content``() =
    let projectFile = new ProjectFile("foo.fsproj", projectWithoutFiles)
    let changedProjectFile = projectFile.AddReference "ref"

    let projectContent = changedProjectFile.Content

    let expectedContent = """<?xml version="1.0" encoding="utf-16"?>
<Project ToolsVersion="12.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <ItemGroup>
    <Reference Include="mscorlib" />
    <Reference Include="FSharp.Core, Version=$(TargetFSharpCoreVersion), Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a">
      <Private>True</Private>
    </Reference>
    <Reference Include="System" />
    <Reference Include="System.Core" />
    <Reference Include="System.Numerics" />
    <Reference Include="ref" />
  </ItemGroup>
</Project>"""

    expectedContent |> should equal projectContent


[<Test>]
let ``Add reference to project - content``() =
    let projectFile = new ProjectFile("foo.fsproj", projectWithFiles)
    let changedProjectFile = projectFile.AddReference "ref"

    let projectContent = changedProjectFile.Content

    let expectedContent = """<?xml version="1.0" encoding="utf-16"?>
<Project ToolsVersion="12.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <ItemGroup>
    <Reference Include="mscorlib" />
    <Reference Include="FSharp.Core, Version=$(TargetFSharpCoreVersion), Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a">
      <Private>True</Private>
    </Reference>
    <Reference Include="System" />
    <Reference Include="System.Core" />
    <Reference Include="System.Numerics" />
    <Reference Include="ref" />
  </ItemGroup>
  <ItemGroup>
    <Compile Include="FixProject.fs" />
    <None Include="App.config" />
    <Compile Include="a_file.fs" />
  </ItemGroup>
</Project>"""

    expectedContent |> should equal projectContent

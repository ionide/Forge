module Fix.Tests.References

open Fix.ProjectSystem
open NUnit.Framework
open FsUnit

open Fix.Tests.Common


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

    let expectedContent = """<?xml version="1.0" encoding="utf-8"?>
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

    projectContent |> shouldbetext expectedContent


[<Test>]
let ``Add reference to project - content``() =
    let projectFile = new ProjectFile("foo.fsproj", projectWithFiles)
    let changedProjectFile = projectFile.AddReference "ref"

    let projectContent = changedProjectFile.Content

    let expectedContent = """<?xml version="1.0" encoding="utf-8"?>
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

    projectContent |> shouldbetext expectedContent

[<Test>]
let ``Remove reference from project``() =
    let projectFile = new ProjectFile("foo.fsproj", projectWithoutFiles)
    let newProject = projectFile.RemoveReference "System"
    newProject.References |> should not' (contain "System")

[<Test>]
let ``Remove reference from project - only references - content``() =
    let projectFile = new ProjectFile("foo.fsproj", projectWithoutFiles)
    let changedProjectFile = projectFile.RemoveReference "System"

    let projectContent = changedProjectFile.Content

    let expectedContent = """<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="12.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <ItemGroup>
    <Reference Include="mscorlib" />
    <Reference Include="FSharp.Core, Version=$(TargetFSharpCoreVersion), Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a">
      <Private>True</Private>
    </Reference>
    <Reference Include="System.Core" />
    <Reference Include="System.Numerics" />
  </ItemGroup>
</Project>"""

    projectContent |> shouldbetext expectedContent


[<Test>]
let ``Remove reference from project - content``() =
    let projectFile = new ProjectFile("foo.fsproj", projectWithFiles)
    let changedProjectFile = projectFile.RemoveReference "System"

    let projectContent = changedProjectFile.Content

    let expectedContent = """<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="12.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <ItemGroup>
    <Reference Include="mscorlib" />
    <Reference Include="FSharp.Core, Version=$(TargetFSharpCoreVersion), Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a">
      <Private>True</Private>
    </Reference>
    <Reference Include="System.Core" />
    <Reference Include="System.Numerics" />
  </ItemGroup>
  <ItemGroup>
    <Compile Include="FixProject.fs" />
    <None Include="App.config" />
    <Compile Include="a_file.fs" />
  </ItemGroup>
</Project>"""

    projectContent |> shouldbetext expectedContent

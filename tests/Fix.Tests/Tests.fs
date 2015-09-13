module Fix.Tests

open Fix
open NUnit.Framework
open ProjectSystem


[<Test>]
let ``ProjectFiles gets all project files`` () =
    let project = """<?xml version="1.0" encoding="utf-8"?>
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
    let projectFile = new ProjectFile("foo.fsproj", project)
    let files = projectFile.ProjectFiles
    Assert.AreEqual(3, files |> Seq.length)
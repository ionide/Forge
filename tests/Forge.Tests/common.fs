module Forge.Tests.Common

open Forge.ProjectSystem
open NUnit.Framework



let cleanup (text : string) =
    let hasCrLf = text.Contains("\r\n")

    match hasCrLf with
    | true ->
        text
    | false ->
        text.Replace("\n", "\r\n")

let shouldbetext expected actual =
    let cleanupExpected = cleanup expected
    let cleanupActual = cleanup actual

    Assert.AreEqual(cleanupExpected, cleanupActual)


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

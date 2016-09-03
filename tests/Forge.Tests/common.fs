module Forge.Tests.Common

open NUnit.Framework
open NUnit.Framework.Constraints



let cleanup (text : string) =
    match text.Contains "\r\n" with
    | true  -> text
    | false -> text.Replace("\n", "\r\n")

let shouldbetext expected actual =
    let cleanupExpected = cleanup expected
    let cleanupActual = cleanup actual

    Assert.AreEqual(cleanupExpected, cleanupActual)

let equivalent (x: seq<_>) = CollectionEquivalentConstraint x

let astInput =
    """<?xml version="1.0" encoding="utf-8"?>
        <Project ToolsVersion="12.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
            <PropertyGroup>
                <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>
                <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>
                <SchemaVersion>2.0</SchemaVersion>
                <ProjectGuid>fbaf8c7b-4eda-493a-a7fe-4db25d15736f</ProjectGuid>
                <OutputType>Library</OutputType>
                <RootNamespace>Test</RootNamespace>
                <AssemblyName>Test</AssemblyName>
                <TargetFrameworkVersion>v4.5</TargetFrameworkVersion>
                <TargetFSharpCoreVersion>4.3.0.0</TargetFSharpCoreVersion>
                <Name>Test</Name>
                <DocumentationFile>bin\Debug\Test.XML</DocumentationFile>
            </PropertyGroup>
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

let projectWithLinkedFiles = """<?xml version="1.0" encoding="utf-8"?>
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
            <Compile Include="foo/bar/some-file.fs" />
            <Compile Include="foo/bar/some-file2.fs" />
            <Compile Include="foo/abc/some-file3.fs" />
            <Compile Include="fldr/some-file2.fs" />
            <Compile Include="../foo/external.fs">
                <Link>linked/ext/external.fs</Link>
            </Compile>
        </ItemGroup>
    </Project>
    """
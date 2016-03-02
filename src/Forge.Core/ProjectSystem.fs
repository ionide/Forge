#if INTERACTIVE
/// Contains project file comparion tools for MSBuild project files.
#r "System.Xml"
#r "System.Xml.Linq"
#load "Prelude.fs"
#load "XLinq.fs"
open Forge.Prelude
open Forge.XLinq
#else
module Forge.ProjectSystem
#endif

open System
open System.Collections.Generic
open System.Xml
open System.Xml.Linq

(*  Project System AST
    ==================

    The project System AST is a strongly representation of the settings relevant to an F# project
    that can be serialized into the xml format of a .fsproj file

    The records and discriminated unions that build up this AST are not a direct mapping to the MSBuild Schema
    There are additional constructs to enforce rules specific to the F# compilation rules
    Constructs that are superfluous for F# compilation are excluded

    This AST is being constructed with the hopes for functioning as an abstraction layer over both
    the MSBuild XML schema and the project.json schema with the possibility of supporting future formats
    that may be constructed to save us from this nonsense.

    Rough Architecture
    ------------------

    [ Project AST ]
    \_ Project Settings
        -



*)


(*  Settings Unions
    ===============

    The following discriminated unions represent settings that will reoccur throughout the AST
    represented in a type safe manner

    These will map to XML elements and attributes in the .fsproj file
*)




/// Sets the platform for a Build Configuration
///     x86,  x64, or AnyCPU.
/// The default is AnyCPU.
type PlatformType =
    | X86 |  X64 | AnyCPU

    override self.ToString () = self |> function
        | X86     -> Constants.X86    
        | X64     -> Constants.X64    
        | AnyCPU  -> Constants.AnyCPU 

    static member Parse text = text |> function
        | Constants.X86     -> X86
        | Constants.X64     -> X64
        | Constants.AnyCPU  -> AnyCPU
        | _ -> 
            failwithf "Could not parse '%s' into a `PlatformType`" text

    static member TryParse text = text |> function
        | Constants.X86     -> Some X86
        | Constants.X64     -> Some X64
        | Constants.AnyCPU  -> Some AnyCPU
        | _                 -> None


[<RequireQualifiedAccess>]
type BuildAction =
    /// Represents the source files for the compiler.
    | Compile
    /// Represents files that are not compiled into the project, but may be embedded or published together with it.
    | Content
    /// Represents an assembly (managed) reference in the project.
    | Reference
    /// Represents files that should have no role in the build process
    | None
    | Resource
    /// Represents resources to be embedded in the generated assembly.
    | EmbeddedResource

    override self.ToString () = self |> function
        | Compile          -> Constants.Compile
        | Content          -> Constants.Content
        | Reference        -> Constants.Reference
        | None             -> Constants.None
        | Resource         -> Constants.Resource
        | EmbeddedResource -> Constants.EmbeddedResource

    static member Parse text = text |> function
        | Constants.Compile          -> Compile
        | Constants.Content          -> Content
        | Constants.Reference        -> Reference
        | Constants.None             -> None
        | Constants.Resource         -> Resource
        | Constants.EmbeddedResource -> EmbeddedResource
        | _ -> 
            failwithf "Could not parse '%s' into a `BuildAction`" text

    static member TryParse text = text |> function
        | Constants.Compile          -> Some Compile
        | Constants.Content          -> Some Content
        | Constants.Reference        -> Some Reference
        | Constants.None             -> Some None
        | Constants.Resource         -> Some Resource
        | Constants.EmbeddedResource -> Some EmbeddedResource
        | _                          -> Option.None


// Under "Compile" in https://msdn.microsoft.com/en-us/library/bb629388.aspx
type CopyToOutputDirectory =
    | Never | Always | PreserveNewest

    override self.ToString () = self |> function
        | Never          -> Constants.Never
        | Always         -> Constants.Always
        | PreserveNewest -> Constants.PreserveNewest

    static member Parse text = text |> function
        | Constants.Never          -> Never
        | Constants.Always         -> Always
        | Constants.PreserveNewest -> PreserveNewest
        | _ -> 
            failwithf "Could not parse '%s' into a `CopyToOutputDirectory`" text

    static member TryParse text = text |> function
        | Constants.Never          -> Some Never
        | Constants.Always         -> Some Always
        | Constants.PreserveNewest -> Some PreserveNewest
        | _                        -> None


[<RequireQualifiedAccess>]
type DebugType =
    | None | PdbOnly | Full

    override self.ToString () = self |> function
        | None    -> Constants.None
        | PdbOnly -> Constants.PdbOnly
        | Full    -> Constants.Full

    static member Parse text = text |> function
        | Constants.None    -> DebugType.None
        | Constants.PdbOnly -> DebugType.PdbOnly
        | Constants.Full    -> DebugType.Full
        | _ -> 
            failwithf "Could not parse '%s' into a `DebugType`" text

    static member TryParse text = text |> function
        | Constants.None    -> Some DebugType.None
        | Constants.PdbOnly -> Some DebugType.PdbOnly
        | Constants.Full    -> Some DebugType.Full
        | _                 -> Option.None


/// Determines the output of compiling the F# Project
type OutputType =
    /// Build a console executable
    | Exe
    ///  Build a Windows executable
    | Winexe
    /// Build a library
    | Library
    /// Build a module that can be added to another assembly (.netmodule)
    | Module

    override self.ToString () = self |> function
        | Exe     -> Constants.Exe     
        | Winexe  -> Constants.Winexe  
        | Library -> Constants.Library 
        | Module  -> Constants.Module  

    static member Parse text = text |> function
        | Constants.Exe     -> Exe
        | Constants.Winexe  -> Winexe
        | Constants.Library -> Library
        | Constants.Module  -> Module
        | _ -> 
            failwithf "Could not parse '%s' into a `OutputType`" text

    static member TryParse text = text |> function
        | Constants.Exe     -> Some Exe
        | Constants.Winexe  -> Some Winexe
        | Constants.Library -> Some Library
        | Constants.Module  -> Some Module
        | _                 -> None



[<Struct>]
type WarningLevel (x:int) =
    member __.Value =
        if x < 0 then 0 elif x > 5 then 5 else x


let inline toXElem x = (^a:(member ToXElem:unit->'b) x)

// Common MSBuild Project Items
// https://msdn.microsoft.com/en-us/library/bb629388.aspx

type Reference =
    {   Include : string
        Condition : string option
        /// Relative or absolute path of the assembly
        HintPath : string option
        /// Optional string. The display name of the assembly, for example, "System.Windows.Forms."
        Name : string option
        /// Optional boolean. Specifies whether only the version in the fusion name should be referenced.
        SpecificVersion : bool option
        /// Optional boolean. Specifies whether the reference should be copied to the output folder.
        /// This attribute matches the Copy Local property of the reference that's in the Visual Studio IDE.
        // if CopyLocal is true shown as "<Private>false</Private>" in XML)
        CopyLocal : bool option
    }

    static member fromXElem (xelem:XElement) =
        let name =  xelem.Name.LocalName
        if name <> "Reference" then
            failwithf "XElement provided was not a `Reference` was `%s` instead" name
        else
        {   Include         = XElem.getAttributeValue  Constants.Include         xelem
            HintPath        = XElem.tryGetElementValue Constants.HintPath        xelem
            Condition       = XElem.tryGetElementValue Constants.Condition       xelem
            Name            = XElem.tryGetElementValue Constants.Name            xelem
            CopyLocal       = XElem.tryGetElementValue Constants.Private         xelem |> Option.bind parseBool
            SpecificVersion = XElem.tryGetElementValue Constants.SpecificVersion xelem |> Option.bind parseBool
        }

    member self.ToXElem () =
        XElem.create Constants.Reference []
        |> XElem.setAttribute Constants.Include self.Include
        |> mapOpt self.Condition        ^ XElem.setAttribute Constants.Condition
        |> mapOpt self.Name             ^ XElem.addElem Constants.Name
        |> mapOpt self.HintPath         ^ XElem.addElem Constants.HintPath
        |> mapOpt self.SpecificVersion  ^ fun b node -> XElem.addElem Constants.SpecificVersion (string b) node
        |> mapOpt self.CopyLocal        ^ fun b node -> XElem.addElem Constants.Private (string b) node


/// Represents a reference to another project
// https://msdn.microsoft.com/en-us/library/bb629388.aspx
type ProjectReference =
    {   /// Path to the project file to include
        /// translates to the `Include` attribute in MSBuild XML
        Include : string
        Condition : string option
        /// Optional string. The display name of the reference.
        Name : string option
        /// Optional Guid of the referenced project
        // will be project in the MSBuild XML
        Guid : Guid option
        /// Should the assemblies of this project be copied Locally
        // if CopyLocal is true shown as "<Private>false</Private>" in XML)
        CopyLocal : bool option
    }
    /// Constructs a ProjectReference from an XElement
    static member fromXElem (xelem:XElement) =
        let name =  xelem.Name.LocalName
        if name <> Constants.ProjectReference then
            failwithf "XElement provided was not a `ProjectReference` was `%s` instead" name
        else
        {   Include     = XElem.getAttributeValue  Constants.Include   xelem
            Condition   = XElem.tryGetElementValue Constants.Condition xelem
            Name        = XElem.tryGetElementValue Constants.Name      xelem
            CopyLocal   = XElem.tryGetElementValue Constants.Private   xelem |> Option.bind parseBool
            Guid        = XElem.tryGetElementValue Constants.Project   xelem |> Option.bind parseGuid
        }

    member self.ToXElem () =
        XElem.create Constants.ProjectReference []
        |> XElem.setAttribute Constants.Include self.Include
        |> mapOpt self.Condition ^ XElem.setAttribute Constants.Condition
        |> mapOpt self.Name      ^ XElem.addElem Constants.Name
        |> mapOpt self.Guid      ^ fun guid node -> 
            XElem.addElem Constants.Private (sprintf "{%s}" ^ string guid) node
        |> mapOpt self.CopyLocal ^ fun b node -> 
            XElem.addElem Constants.Private (string b) node

(*
    <ProjectReference Include="..\some.fsproj">
      <Name>The-Some</Name>
      <Project>{17b0907c-699a-4e40-a2b6-8caf53cbd004}</Project>
      <Private>False</Private>
    </ProjectReference>
*)

/// use to match against the name of an xelement to see if it represents a source file
let isSrcFile = function
    | Constants.Compile
    | Constants.Content
    | Constants.None
    | Constants.Resource
    | Constants.EmbeddedResource -> true
    | _ -> false


type SourceFile =
    {   Include     : string
        Condition   : string option
        OnBuild     : BuildAction
        Link        : string option
        Copy        : CopyToOutputDirectory option
    }

    static member fromXElem (xelem:XElement) =
        let buildtype =  xelem.Name.LocalName
        if not ^ isSrcFile buildtype then
            failwithf "XElement provided was not `Compile|Content|None|Resource|EmbeddedResource` was `%s` instead" buildtype
        else
        {   OnBuild   = BuildAction.Parse buildtype
            Include   = XElem.getAttributeValue  Constants.Include xelem             
            Link      = XElem.tryGetElementValue Constants.Link xelem    
            Condition = XElem.tryGetElementValue Constants.Condition xelem
            Copy      =
                XElem.tryGetElement Constants.CopyToOutputDirectory xelem
                |> Option.bind (XElem.value >> CopyToOutputDirectory.TryParse)
        }

    member self.ToXElem () =
        XElem.create (string self.OnBuild) []
        |> XElem.setAttribute Constants.Include self.Include
        |> mapOpt self.Condition ^ XElem.setAttribute Constants.Condition
        |> mapOpt self.Link      ^ XElem.addElem Constants.Link
        |> mapOpt self.Copy ^ fun copy node ->
            match copy with
            | Never          -> node
            | Always         -> XElem.addElem Constants.CopyToOutputDirectory (string Always) node
            | PreserveNewest -> XElem.addElem Constants.CopyToOutputDirectory (string PreserveNewest) node

(* ^ will produce an xml node like

    <Compile Include="path.fs">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
      <Link>Common/path.fs</Link>
    </Compile>
*)


type SourcePair =
    {   Sig     : SourceFile
        Module  : SourceFile
    }

    member self.ToXElem () = [
        self.Sig.ToXElem ()
        self.Module.ToXElem ()
    ]


type SourceElement =
    | File      of SourceFile
    | Pair      of SourcePair
    | Directory of SourceElement list

    member self.ToXElem () : XElement list =
        match self with
        | File  x       -> [x.ToXElem()]
        | Pair  x       -> x.ToXElem()
        | Directory x   -> x |> List.collect (fun e -> e.ToXElem())


type Property<'a> =
    {   /// The name of the element tag in XML
        Name      : string
        /// The condition attribute
        Condition : string option
        /// Value stored withing tag
        Data     : 'a option
    }

    static member fromXElem (xelem:XElement) =
        {   Name      = xelem.Name.LocalName
            Condition = XElem.tryGetAttributeValue Constants.Condition xelem 
            Data      =
                if String.IsNullOrWhiteSpace xelem.Value then None else
                Some xelem.Value
        }

    static member fromXElem (xelem:XElement, mapString: string -> 'a) =
        {   Name      = xelem.Name.LocalName
            Condition = XElem.tryGetAttributeValue Constants.Condition xelem 
            Data      =
                if String.IsNullOrWhiteSpace xelem.Value then None else
                Some <| mapString xelem.Value
        }

    member self.ToXElem () =
        XElem.create self.Name (if self.Data.IsSome then [self.Data.Value] else [])
        |> mapOpt self.Condition ^ XElem.setAttribute Constants.Condition


type ProjectSettings =
    {   Name                         : Property<string>
        AssemblyName                 : Property<string>
        RootNamespace                : Property<string>
        Configuration                : Property<string>
        Platform                     : Property<string>
        SchemaVersion                : Property<string>
        ProjectGuid                  : Property<Guid>
        ProjectType                  : Property<Guid list>
        OutputType                   : Property<OutputType>
        TargetFrameworkVersion       : Property<string>
        TargetFrameworkProfile       : Property<string>
        AutoGenerateBindingRedirects : Property<bool>
        TargetFSharpCoreVersion      : Property<string>
    }

    static member fromXElem (xelem:XElement) =

        let splitGuids (str:string) =
            if String.IsNullOrWhiteSpace str then [] else
            str.Split ';' |> Array.choose parseGuid |> List.ofArray

        if  not (Constants.PropertyGroup = xelem.Name.LocalName)
         || XElem.hasAttribute Constants.Condition xelem then
            failwithf "XElement provided was not `PropertyGroup` without a condition was `%s` instead" xelem.Name.LocalName
        else

        let elem name =
            match XElem.tryGetElement name xelem with
            | Some x -> Property<string>.fromXElem x
            | None   ->
                {   Name      = name
                    Condition = None
                    Data      = None    }

        let elemmap name (mapfn:string -> 'a) =
            match XElem.tryGetElement name xelem with
            | Some x -> Property<'a>.fromXElem(x,mapfn)
            | None   ->
                {   Name      = name
                    Condition = None
                    Data      = None    }

        {   Name                         = elem    Constants.Name
            AssemblyName                 = elem    Constants.AssemblyName
            RootNamespace                = elem    Constants.RootNamespace
            Configuration                = elem    Constants.Configuration
            Platform                     = elem    Constants.Platform
            SchemaVersion                = elem    Constants.SchemaVersion
            ProjectGuid                  = elemmap Constants.ProjectGuid Guid.Parse
            ProjectType                  = elemmap Constants.ProjectType splitGuids
            OutputType                   = elemmap Constants.OutputType OutputType.Parse
            TargetFrameworkVersion       = elem    Constants.TargetFrameworkVersion
            TargetFrameworkProfile       = elem    Constants.TargetFrameworkProfile
            AutoGenerateBindingRedirects = elemmap Constants.AutoGenerateBindingRedirects Boolean.Parse
            TargetFSharpCoreVersion      = elem    Constants.TargetFSharpCoreVersion
        }

        member self.ToXElem () =
            XElem.create Constants.PropertyGroup []
            |> XElem.addElement ^ toXElem self.Name                        
            |> XElem.addElement ^ toXElem self.AssemblyName                
            |> XElem.addElement ^ toXElem self.RootNamespace               
            |> XElem.addElement ^ toXElem self.Configuration               
            |> XElem.addElement ^ toXElem self.Platform                    
            |> XElem.addElement ^ toXElem self.SchemaVersion               
            |> XElem.addElement ^ toXElem self.ProjectGuid                 
            |> XElem.addElement ^ toXElem self.ProjectType                 
            |> XElem.addElement ^ toXElem self.OutputType                  
            |> XElem.addElement ^ toXElem self.TargetFrameworkVersion      
            |> XElem.addElement ^ toXElem self.TargetFrameworkProfile      
            |> XElem.addElement ^ toXElem self.AutoGenerateBindingRedirects
            |> XElem.addElement ^ toXElem self.TargetFSharpCoreVersion     


// parse the condition strings in property groups to create config settings
// maybe even map those strings across the properties and then sort them to create config sets?

type ConfigSettings =
    {   /// The Condition attribute on the PropertyGroup
        Condition            : string
        DebugSymbols         : Property<bool>
        DebugType            : Property<string>
        Optimize             : Property<bool>
        Tailcalls            : Property<bool>
        OutputPath           : Property<string>
        CompilationConstants : Property<string list>
        WarningLevel         : Property<WarningLevel>
        PlatformTarget       : Property<PlatformType>
        Documentationfile    : Property<string>
        Prefer32Bit          : Property<bool>
        OtherFlags           : Property<string list>
    }

    static member fromXElem (xelem:XElement) =

        if  not (Constants.PropertyGroup = xelem.Name.LocalName)
         || not ^ XElem.hasAttribute Constants.Condition xelem then
            failwithf "XElement provided was not `PropertyGroup` with a condition attribute, was `%s` instead" xelem.Name.LocalName
        else

        let split (str:string) =
            if String.IsNullOrWhiteSpace str then [] else
            str.Split ';' |> List.ofArray

        let elem name =
            match XElem.tryGetElement name xelem with
            | Some x -> Property<string>.fromXElem x
            | None   ->
                {   Name      = name
                    Condition = None
                    Data      = None    }

        let elemmap name (mapfn:string -> 'a) =
            match XElem.tryGetElement name xelem with
            | Some x -> Property<'a>.fromXElem(x,mapfn)
            | None   ->
                {   Name      = name
                    Condition = None
                    Data      = None    }

        {   Condition            = XElem.getAttributeValue Constants.Condition xelem 
            DebugSymbols         = elemmap Constants.DebugSymbols Boolean.Parse
            DebugType            = elem    Constants.DebugType
            Optimize             = elemmap Constants.Optimize Boolean.Parse
            Tailcalls            = elemmap Constants.Tailcalls Boolean.Parse
            OutputPath           = elem    Constants.OutputPath
            CompilationConstants = elemmap Constants.CompilationConstants split
            WarningLevel         = elemmap Constants.WarningLevel (Int32.Parse>>WarningLevel)
            PlatformTarget       = elemmap Constants.PlatformTarget PlatformType.Parse
            Documentationfile    = elem    Constants.Documentationfile
            Prefer32Bit          = elemmap Constants.Prefer32Bit Boolean.Parse
            OtherFlags           = elemmap Constants.OtherFlags split
        }

    member self.ToXElem () =
        XElem.create Constants.PropertyGroup []
        |> XElem.setAttribute Constants.Condition self.Condition
        |> XElem.addElement ^ toXElem self.DebugSymbols
        |> XElem.addElement ^ toXElem self.DebugType
        |> XElem.addElement ^ toXElem self.Optimize
        |> XElem.addElement ^ toXElem self.Tailcalls
        |> XElem.addElement ^ toXElem self.OutputPath
        |> XElem.addElement ^ toXElem self.CompilationConstants
        |> XElem.addElement ^ toXElem self.WarningLevel
        |> XElem.addElement ^ toXElem self.PlatformTarget
        |> XElem.addElement ^ toXElem self.Documentationfile
        |> XElem.addElement ^ toXElem self.Prefer32Bit
        |> XElem.addElement ^ toXElem self.OtherFlags


type FsProject =
    {   ToolsVersion        : string
        DefaultTargets      : string list
        Settings            : ProjectSettings
        BuildConfigs        : ConfigSettings list
        ProjectReferences   : ProjectReference list
        References          : Reference list
        SourceFiles         : SourceElement list
    }

    member __.xmlns = XNamespace.Get @"http://schemas.microsoft.com/developer/msbuild/2003"

    member self.ToXElem () =
        let projxml =
            XElem.create Constants.Project []
            |> XElem.setAttribute Constants.ToolsVersion self.ToolsVersion
            |> XElem.setAttribute Constants.DefaultTargets (self.DefaultTargets |> String.concat " ")
            |> XElem.addElement  ^ toXElem self.Settings
            |> XElem.addElements ^ (self.BuildConfigs |> List.map toXElem)
            |> XElem.addElement  ^
               XElem.create Constants.ItemGroup (self.References |> List.map toXElem)
            |> XElem.addElement  ^
               XElem.create Constants.ItemGroup (self.SourceFiles |> List.map toXElem)

        // add msbuild namespace to XElement representing the project
        projxml.DescendantsAndSelf()
        |> Seq.iter(fun x -> x.Name <- self.xmlns + x.Name.LocalName)

        projxml

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module FsProject =

    let addReference (refr:Reference) (proj:FsProject) =
        if proj.References |> List.exists ((=) refr) then proj else
        { proj with References = refr::proj.References }

    let removeReference (refr:Reference) (proj:FsProject) =
        if not (proj.References |> List.exists ((=) refr)) then proj else
        { proj with References = proj.References |> List.filter ((<>) refr) }

    let addFile (file : SourceElement) (proj:FsProject) =
        if proj.SourceFiles |> List.exists ((=) file) then proj else
        { proj with SourceFiles = file::proj.SourceFiles }

    let removeFile (file : SourceElement) (proj:FsProject) =
        if not (proj.SourceFiles |> List.exists ((=) file)) then proj else
        { proj with SourceFiles = proj.SourceFiles |> List.filter ((<>) file) }

    let orderFile (fileToMove : SourceElement) (putBefore : SourceElement) (proj : FsProject) =
         if not (proj.SourceFiles |> List.exists ((=) fileToMove) ||proj.SourceFiles |> List.exists ((=) putBefore)) then proj else
         let filesBefore = proj.SourceFiles |> List.filter ((<>) fileToMove) |> List.takeWhile ((<>) putBefore)
         let filesAfter = proj.SourceFiles |> List.filter ((<>) fileToMove) |> List.skipWhile ((<>) putBefore)
         let filesNew = seq {yield! filesBefore; yield fileToMove; yield! filesAfter } |> Seq.toList
         { proj with SourceFiles = filesNew }

    let parse content =
        let xdoc = (XDocument.Parse content).Root
        let itemGroups = XElem.descendantsNamed Constants.ItemGroup xdoc

        let projectSettingsSqs =
            XElem.descendantsNamed Constants.PropertyGroup xdoc
            |> Seq.filter (fun pg -> not ^ XElem.hasAttribute Constants.Condition pg)

        let projectSettings = projectSettingsSqs |> Seq.head |> ProjectSettings.fromXElem

    //    let buildConfigs =
    //        XElem.descendantsNamed "PropertyGroup" xdoc
    //        |> Seq.filter (fun pg -> XElem.hasAttribute "Condition" pg)

        let projectReferences =
            XElem.descendantsNamed Constants.ProjectReference xdoc
            |> Seq.map ProjectReference.fromXElem

        let filterItems name =
            itemGroups |> Seq.collect ^ XElem.descendantsNamed name

        let references =
            filterItems Constants.Reference
            |> Seq.filter  (not << XElem.hasElement Constants.Paket) // we only manage references paket isn't already managing
            |> Seq.map Reference.fromXElem

        let srcFiles =
            itemGroups
            |> Seq.collect (fun itemgroup ->
                XElem.descendants itemgroup
                |> Seq.filter (fun x -> isSrcFile x.Name.LocalName))
            |> Seq.map SourceFile.fromXElem

        let proj =
            {   ToolsVersion      = XElem.getAttributeValue Constants.ToolsVersion xdoc 
                DefaultTargets    = [XElem.getAttributeValue Constants.DefaultTargets xdoc ]
                References        = references |> List.ofSeq
                Settings          = projectSettings
                SourceFiles       = srcFiles |> Seq.map File |> List.ofSeq
                ProjectReferences = projectReferences |> List.ofSeq
                BuildConfigs      = []
            }

        proj

    let load path =
        let content = System.IO.File.ReadAllText path
        parse content

// A small abstraction over MSBuild project files.
type ProjectFile (projectFileName:string, documentContent:string) =
    let document = XMLDoc documentContent

    let nsmgr =
        let nsmgr = XmlNamespaceManager document.NameTable
        nsmgr.AddNamespace("default", document.DocumentElement.NamespaceURI)
        nsmgr

    let compileNodesXPath = "/default:Project/default:ItemGroup/default:Compile"

    let projectFilesXPath = "/default:Project/default:ItemGroup/default:Compile|" +
                            "/default:Project/default:ItemGroup/default:Content|" +
                            "/default:Project/default:ItemGroup/default:None"

    let referenceFilesXPath = "/default:Project/default:ItemGroup/default:Reference"

    let nodeListToList (nodeList:XmlNodeList) = [for node in nodeList -> node]
    let getNodes xpath (document:XmlDocument) = document.SelectNodes(xpath, nsmgr) |> nodeListToList
    let getFileAttribute (node:XmlNode) = node.Attributes.["Include"].InnerText

    let newElement (document:XmlDocument) name = document.CreateElement(name, document.DocumentElement.NamespaceURI)

    let addFile fileName nodeType xPath =
        let document = XMLDoc documentContent // we create a copy and work immutable

        let newNode = newElement document nodeType
        newNode.SetAttribute("Include", fileName)

        //get the first ItemGroup node
        let itemGroup = getNodes xPath document |> List.map(fun x -> x.ParentNode) |> List.distinct |> List.tryHead

        match itemGroup with
        | Some n -> n.AppendChild(newNode) |> ignore
        | None ->
            let groupNode = newElement document "ItemGroup"
            groupNode.AppendChild newNode |> ignore
            let project = getNodes "/default:Project" document |> Seq.head
            project.AppendChild groupNode |> ignore

        new ProjectFile(projectFileName,document.OuterXml)

    let getNode document xPath fileName =
        getNodes xPath document
        |> List.filter (fun node -> getFileAttribute node = fileName)
        |> Seq.tryLast

    let removeFile fileName xPath =
        let document = XMLDoc documentContent // we create a copy and work immutable
        let node = getNode document xPath fileName

        match node with
        | Some n -> n.ParentNode.RemoveChild n |> ignore
        | None -> ()

        new ProjectFile(projectFileName,document.OuterXml)

    let orderFiles fileName1 fileName2 xPath =
        let document = XMLDoc documentContent // we create a copy and work immutable
        match getNode document xPath fileName1 with
        | Some n1 ->
            let updated = removeFile fileName1 xPath
            let updatedXml = XMLDoc updated.Content
            match getNode updatedXml xPath fileName2 with
            | Some n2 ->
                let node = newElement updatedXml n1.Name
                node.SetAttribute("Include", fileName1)
                n2.ParentNode.InsertBefore(node, n2) |> ignore

                new ProjectFile(projectFileName, updatedXml.OuterXml)

            | None -> new ProjectFile(projectFileName,document.OuterXml)
        | _ -> new ProjectFile(projectFileName,document.OuterXml)

    /// Read a Project from a FileName
    static member FromFile(projectFileName) = new ProjectFile(projectFileName,readFileAsString projectFileName)

    /// Saves the project file
    member x.Save(?fileName) =
        use writer = new System.IO.StreamWriter(defaultArg fileName projectFileName,
                                                false,
                                                new System.Text.UTF8Encoding(false))
        document.Save(writer)

    member x.Content =
        let utf8 = System.Text.UTF8Encoding false
        let settings = XmlWriterSettings()
        settings.Encoding <- utf8
        settings.Indent <- true
        use ms = new System.IO.MemoryStream()
        use writer = System.Xml.XmlWriter.Create(ms, settings)
        document.Save(writer)
        ms.GetBuffer() |> utf8.GetString



    /// Add a file to the ItemGroup node with node type
    member x.AddFile fileName nodeType =
        addFile fileName nodeType projectFilesXPath

    /// Removes a file from the ItemGroup node with optional node type
    member x.RemoveFile fileName =
        removeFile fileName projectFilesXPath

    member x.AddReference reference =
        addFile reference "Reference" referenceFilesXPath

    member x.RemoveReference reference =
        removeFile reference referenceFilesXPath

    /// All files which are in "Compile" sections
    member x.Files = getNodes compileNodesXPath document |> List.map getFileAttribute


    member x.ProjectFiles = getNodes projectFilesXPath document |> List.map getFileAttribute

    member x.References = getNodes referenceFilesXPath document |> List.map getFileAttribute

    /// Finds duplicate files which are in "Compile" sections
    member this.FindDuplicateFiles() =
        [let dict = Dictionary()
         for file in this.Files do
            match dict.TryGetValue file with
            | false,_    -> dict.[file] <- false            // first observance
            | true,false -> dict.[file] <- true; yield file // second observance
            | true,true  -> ()                              // already seen at least twice
        ]

    member x.RemoveDuplicates() =
        x.FindDuplicateFiles()
        |> List.fold (fun (project:ProjectFile) duplicate -> project.RemoveFile duplicate) x

    /// Places the first file above the second file
    member x.OrderFiles file1 file2 =
        orderFiles file1 file2 projectFilesXPath


    /// The project file name
    member x.ProjectFileName = projectFileName


#if INTERACTIVE
;; readfsproj ^ __SOURCE_DIRECTORY__ + "/../Forge/Forge.fsproj"
#endif

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
        | X86     -> "x86" 
        | X64     -> "x64"
        | AnyCPU  -> "AnyCPU"

    static member Parse text = text |> function
        | "x86"     -> X86     
        | "x64"     -> X64     
        | "AnyCPU"  -> AnyCPU  
        | _         -> failwithf "Could not parse '%s' into a `PlatformType`" text

    static member TryParse text = text |> function
        | "x86"     -> Some X86     
        | "x64"     -> Some X64     
        | "AnyCPU"  -> Some AnyCPU  
        | _         -> None


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
        | Compile          -> "Compile"
        | Content          -> "Content"
        | Reference        -> "Reference"
        | None             -> "None"
        | Resource         -> "Resource"
        | EmbeddedResource -> "EmbeddedResource"

    static member Parse text = text |> function
        | "Compile"          -> Compile          
        | "Content"          -> Content          
        | "Reference"        -> Reference        
        | "None"             -> None             
        | "Resource"         -> Resource         
        | "EmbeddedResource" -> EmbeddedResource 
        | _                  -> failwithf "Could not parse '%s' into a `BuildAction`" text

    static member TryParse text = text |> function
        | "Compile"          -> Some Compile          
        | "Content"          -> Some Content          
        | "Reference"        -> Some Reference        
        | "None"             -> Some None             
        | "Resource"         -> Some Resource         
        | "EmbeddedResource" -> Some EmbeddedResource 
        | _                  -> Option.None


// Under "Compile" in https://msdn.microsoft.com/en-us/library/bb629388.aspx
type CopyToOutputDirectory =
    | Never | Always | PreserveNewest
       
    override self.ToString () = self |> function
        | Never          -> "Never"
        | Always         -> "Always"
        | PreserveNewest -> "PreserveNewest"

    static member Parse text = text |> function
        | "Never"          -> Never
        | "Always"         -> Always
        | "PreserveNewest" -> PreserveNewest
        | _                -> failwithf "Could not parse '%s' into a `CopyToOutputDirectory`" text

    static member TryParse text = text |> function
        | "Never"          -> Some Never
        | "Always"         -> Some Always
        | "PreserveNewest" -> Some PreserveNewest
        | _                -> None


[<RequireQualifiedAccess>]
type DebugType =
    | None | PdbOnly | Full

    override self.ToString () = self |> function
        | None    -> "None"   
        | PdbOnly -> "PdbOnly"
        | Full    -> "Full"   

    static member Parse text = text |> function
        | "None"    -> DebugType.None   
        | "PdbOnly" -> DebugType.PdbOnly
        | "Full"    -> DebugType.Full   
        | _         -> failwithf "Could not parse '%s' into a `DebugType`" text

    static member TryParse text = text |> function
        | "None"    -> Some DebugType.None   
        | "PdbOnly" -> Some DebugType.PdbOnly
        | "Full"    -> Some DebugType.Full   
        | _         -> Option.None


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
        | Exe     -> "Exe"
        | Winexe  -> "Winexe"
        | Library -> "Library"
        | Module  -> "Module"

    static member Parse text = text |> function
        | "Exe"     -> Exe     
        | "Winexe"  -> Winexe  
        | "Library" -> Library 
        | "Module"  -> Module  
        | _         -> failwithf "Could not parse '%s' into a `OutputType`" text

    static member TryParse text = text |> function
        | "Exe"     -> Some Exe     
        | "Winexe"  -> Some Winexe  
        | "Library" -> Some Library 
        | "Module"  -> Some Module  
        | _         -> None 



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
        {   Include         = XElem.getAttribute  "Include"         xelem |> XAttr.value
            HintPath        = XElem.tryGetElement "HintPath"        xelem |> Option.map XElem.value
            Condition       = XElem.tryGetElement "Condition"       xelem |> Option.map XElem.value
            Name            = XElem.tryGetElement "Name"            xelem |> Option.map XElem.value
            CopyLocal       = XElem.tryGetElement "Private"         xelem |> Option.bind (XElem.value >> parseBool)
            SpecificVersion = XElem.tryGetElement "SpecificVersion" xelem |> Option.bind (XElem.value >> parseBool)
        }

    member self.ToXElem () =
        XElem.create "Reference" []
        |> XElem.setAttribute "Include" self.Include
        |> mapOpt self.Condition        ^ XElem.setAttribute "Condition"
        |> mapOpt self.Name             ^ XElem.addElem "Name"
        |> mapOpt self.HintPath         ^ XElem.addElem "HintPath"        
        |> mapOpt self.SpecificVersion  ^ fun b node -> XElem.addElem "SpecificVersion" (string b) node
        |> mapOpt self.CopyLocal        ^ fun b node -> XElem.addElem "Private" (string b) node

        
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
        if name <> "ProjectReference" then 
            failwithf "XElement provided was not a `ProjectReference` was `%s` instead" name 
        else
        {   Include     = XElem.getAttribute  "Include"   xelem |> XAttr.value
            Condition   = XElem.tryGetElement "Condition" xelem |> Option.map XElem.value
            Name        = XElem.tryGetElement "Name"      xelem |> Option.map XElem.value
            CopyLocal   = XElem.tryGetElement "Private"   xelem |> Option.bind (XElem.value >> parseBool)
            Guid        = XElem.tryGetElement "Project"   xelem |> Option.bind (XElem.value >> parseGuid)
        }
        
    member self.ToXElem () =
        XElem.create "ProjectReference" []
        |> XElem.setAttribute "Include" self.Include
        |> mapOpt self.Condition ^ XElem.setAttribute "Condition"
        |> mapOpt self.Name      ^ XElem.addElem "Name"
        |> mapOpt self.Guid      ^ fun guid node -> XElem.addElem "Project" (sprintf "{%s}" ^ string guid) node
        |> mapOpt self.CopyLocal ^ fun b node -> XElem.addElem "Private" (string b) node

(*
    <ProjectReference Include="..\some.fsproj">
      <Name>The-Some</Name>
      <Project>{17b0907c-699a-4e40-a2b6-8caf53cbd004}</Project>
      <Private>False</Private>
    </ProjectReference>
*)

/// use to match against the name of an xelement to see if it represents a source file
let isSrcFile = function
    | "Compile"
    | "Content"
    | "None"
    | "Resource"
    | "EmbeddedResource" -> true
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
        {   Include   = XElem.getAttribute  "Include" xelem |> XAttr.value
            OnBuild   = BuildAction.Parse buildtype
            Link      = XElem.tryGetElement "Link" xelem    |> Option.map XElem.value
            Condition = XElem.tryGetElement "Condition" xelem    |> Option.map XElem.value
            Copy      = 
                XElem.tryGetElement "CopyToOutputDirectory" xelem 
                |> Option.bind (XElem.value >> CopyToOutputDirectory.TryParse)
        }

    member self.ToXElem () =
        XElem.create (string self.OnBuild) []        
        |> XElem.setAttribute "Include" self.Include
        |> mapOpt self.Condition ^ XElem.setAttribute "Condition"
        |> mapOpt self.Link      ^ XElem.addElem "Link"
        |> mapOpt self.Copy ^ fun copy node ->
            match copy with 
            | Never          -> node
            | Always         -> XElem.addElem "CopyToOutputDirectory" (string Always) node
            | PreserveNewest -> XElem.addElem "CopyToOutputDirectory" (string PreserveNewest) node

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
            Condition = XElem.tryGetAttribute "Condition" xelem |> Option.map XAttr.value
            Data      = 
                if String.IsNullOrWhiteSpace xelem.Value then None else 
                Some xelem.Value                
        }

    static member fromXElem (xelem:XElement, mapString:string -> 'a) =
        {   Name      = xelem.Name.LocalName
            Condition = XElem.tryGetAttribute "Condition" xelem |> Option.map XAttr.value
            Data      = 
                if String.IsNullOrWhiteSpace xelem.Value then None else 
                Some <| mapString xelem.Value                
        }

    member self.ToXElem () =
        XElem.create self.Name (if self.Data.IsSome then [self.Data.Value] else [])
        |> mapOpt self.Condition ^ XElem.setAttribute "Condition"


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
    
        if  not ("PropertyGroup" = xelem.Name.LocalName) 
         || XElem.hasAttribute "Condition" xelem then 
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

        {   Name                         = elem    "Name"                                  
            AssemblyName                 = elem    "AssemblyName"  
            RootNamespace                = elem    "RootNamespace" 
            Configuration                = elem    "Configuration" 
            Platform                     = elem    "Platform"      
            SchemaVersion                = elem    "SchemaVersion"            
            ProjectGuid                  = elemmap "ProjectGuid" Guid.Parse
            ProjectType                  = elemmap "ProjectType" splitGuids
            OutputType                   = elemmap "OutputType" OutputType.Parse
            TargetFrameworkVersion       = elem    "TargetFrameworkVersion"      
            TargetFrameworkProfile       = elem    "TargetFrameworkProfile"     
            AutoGenerateBindingRedirects = elemmap "AutoGenerateBindingRedirects" Boolean.Parse
            TargetFSharpCoreVersion      = elem    "TargetFSharpCoreVersion"            
        }

        member self.ToXElem () =
            XElem.create "PropertyGroup" []        
            |> XElem.addElement ^ self.Name                        .ToXElem ()
            |> XElem.addElement ^ self.AssemblyName                .ToXElem ()
            |> XElem.addElement ^ self.RootNamespace               .ToXElem ()
            |> XElem.addElement ^ self.Configuration               .ToXElem ()
            |> XElem.addElement ^ self.Platform                    .ToXElem ()
            |> XElem.addElement ^ self.SchemaVersion               .ToXElem ()
            |> XElem.addElement ^ self.ProjectGuid                 .ToXElem ()
            |> XElem.addElement ^ self.ProjectType                 .ToXElem ()
            |> XElem.addElement ^ self.OutputType                  .ToXElem ()
            |> XElem.addElement ^ self.TargetFrameworkVersion      .ToXElem ()
            |> XElem.addElement ^ self.TargetFrameworkProfile      .ToXElem ()
            |> XElem.addElement ^ self.AutoGenerateBindingRedirects.ToXElem ()
            |> XElem.addElement ^ self.TargetFSharpCoreVersion     .ToXElem ()


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
    
        if  not ("PropertyGroup" = xelem.Name.LocalName)
         || not ^ XElem.hasAttribute "Condition" xelem then 
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

        {   Condition            = XElem.getAttribute "Condition" xelem |> XAttr.value
            DebugSymbols         = elemmap "DebugSymbols" Boolean.Parse
            DebugType            = elem    "DebugType"
            Optimize             = elemmap "Optimize" Boolean.Parse            
            Tailcalls            = elemmap "Tailcalls" Boolean.Parse           
            OutputPath           = elem    "OutputPath"           
            CompilationConstants = elemmap "CompilationConstants" split
            WarningLevel         = elemmap "WarningLevel" (Int32.Parse>>WarningLevel)        
            PlatformTarget       = elemmap "PlatformTarget" PlatformType.Parse
            Documentationfile    = elem    "Documentationfile"    
            Prefer32Bit          = elemmap "Prefer32Bit" Boolean.Parse          
            OtherFlags           = elemmap "OtherFlags" split          
        }

    member self.ToXElem () =
        XElem.create "PropertyGroup" []        
        |> XElem.setAttribute "Condition" self.Condition
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
            XElem.create "Project" []
            |> XElem.setAttribute "ToolsVersion" self.ToolsVersion
            |> XElem.setAttribute "DefaultTargets" (self.DefaultTargets |> String.concat " ")
            |> XElem.addElement  ^ toXElem self.Settings 
            |> XElem.addElements ^ (self.BuildConfigs |> List.map toXElem)
            |> XElem.addElement  ^
               XElem.create "ItemGroup" [self.References |> List.map toXElem]
            |> XElem.addElement  ^
               XElem.create "ItemGroup" [self.SourceFiles |> List.map toXElem]
        
        // add msbuild namespace to XElement representing the project
        projxml.DescendantsAndSelf()
        |> Seq.iter(fun x -> x.Name <- self.xmlns + x.Name.LocalName)

        projxml


let readfsproj path =
    let print sqs = sqs |> Seq.iter ^ printfn "%A"

    printfn "parsing - %s" path
    use reader = XmlReader.Create  path
    let xdoc   = (reader |> XDocument.Load).Root
    
    let itemGroups = XElem.descendantsNamed "ItemGroup" xdoc

    let projectSettingsSqs = 
        XElem.descendantsNamed "PropertyGroup" xdoc
        |> Seq.filter (fun pg -> not ^ XElem.hasAttribute "Condition" pg)
        
    let projectSettings = projectSettingsSqs |> Seq.head |> ProjectSettings.fromXElem

//    let buildConfigs = 
//        XElem.descendantsNamed "PropertyGroup" xdoc
//        |> Seq.filter (fun pg -> XElem.hasAttribute "Condition" pg)

    let projectReferences = 
        XElem.descendantsNamed "ProjectReference" xdoc
        |> Seq.map ProjectReference.fromXElem

    let filterItems name =
        itemGroups |> Seq.collect ^ XElem.descendantsNamed name

    let references = 
        filterItems "Reference"
        |> Seq.filter  (not << XElem.hasElement "Paket") // we only manage references paket isn't already managing
        |> Seq.map Reference.fromXElem
    
    let srcFiles = 
        itemGroups  
        |> Seq.collect (fun itemgroup -> 
            XElem.descendants itemgroup 
            |> Seq.filter (fun x -> isSrcFile x.Name.LocalName)) 
        |> Seq.map SourceFile.fromXElem

//    printfn "\n - PROJECT SETTINGS - \n"
//    print [projectSettings]
//    printfn "\n - PROPERTY GROUPS - \n"    
//    print propertyGroups
//    printfn "\n - PROJECT REFERENCES - \n"
//    print projectReferences
//    printfn "\n - REFERENCES - \n"
//    print references
//    printfn "\n - SOURCE FILES - \n"
//    print srcFiles
    let proj =
        {   ToolsVersion      =  XElem.getAttribute "ToolsVersion" xdoc |> XAttr.value
            DefaultTargets    =  [XElem.getAttribute "DefaultTargets" xdoc |> XAttr.value]
            References        = references |> List.ofSeq
            Settings          = projectSettings
            SourceFiles       = srcFiles |> Seq.map File |> List.ofSeq
            ProjectReferences = projectReferences |> List.ofSeq
            BuildConfigs      = []        
        }

    proj

  



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

/// Result type for project comparisons.
type ProjectComparison =
    { TemplateProjectFileName: string
      ProjectFileName: string
      MissingFiles: string seq
      DuplicateFiles: string seq
      UnorderedFiles: string seq }

      member this.HasErrors =
        not (Seq.isEmpty this.MissingFiles &&
             Seq.isEmpty this.UnorderedFiles &&
             Seq.isEmpty this.DuplicateFiles)

/// Compares the given project files againts the template project and returns which files are missing.
/// For F# projects it is also reporting unordered files.
let findMissingFiles templateProject projects =
    let isFSharpProject file = file |> endsWith ".fsproj"

    let templateFiles = (ProjectFile.FromFile templateProject).Files
    let templateFilesSet = Set.ofSeq templateFiles

    projects
    |> Seq.map (fun fileName -> ProjectFile.FromFile fileName)
    |> Seq.map (fun ps ->
            let missingFiles = Set.difference templateFilesSet (Set.ofSeq ps.Files)

            let unorderedFiles =
                if not <| isFSharpProject templateProject then [] else
                if not <| Seq.isEmpty missingFiles then [] else
                let remainingFiles = ps.Files |> List.filter (fun file -> Set.contains file templateFilesSet)
                if remainingFiles.Length <> templateFiles.Length then [] else

                templateFiles
                |> List.zip remainingFiles
                |> List.filter (fun (a,b) -> a <> b)
                |> List.map fst

            { TemplateProjectFileName = templateProject
              ProjectFileName = ps.ProjectFileName
              MissingFiles = missingFiles
              DuplicateFiles = ps.FindDuplicateFiles()
              UnorderedFiles = unorderedFiles })
    |> Seq.filter (fun pc -> pc.HasErrors)

/// Compares the given projects to the template project and adds all missing files to the projects if needed.
let FixMissingFiles templateProject projects =
    let addMissing (project:ProjectFile) missingFile =
        printfn "Adding %s to %s" missingFile project.ProjectFileName
        project.AddFile missingFile "Compile"

    findMissingFiles templateProject projects
    |> Seq.iter (fun pc ->
            let project = ProjectFile.FromFile pc.ProjectFileName
            if not (Seq.isEmpty pc.MissingFiles) then
                let newProject = Seq.fold addMissing project pc.MissingFiles
                newProject.Save())

/// It removes duplicate files from the project files.
let RemoveDuplicateFiles projects =
    projects
    |> Seq.iter (fun fileName ->
            let project = ProjectFile.FromFile fileName
            if not (project.FindDuplicateFiles().IsEmpty) then
                let newProject = project.RemoveDuplicates()
                newProject.Save())

/// Compares the given projects to the template project and adds all missing files to the projects if needed.
/// It also removes duplicate files from the project files.
let FixProjectFiles templateProject projects =
    FixMissingFiles templateProject projects
    RemoveDuplicateFiles projects

/// Compares the given project files againts the template project and fails if any files are missing.
/// For F# projects it is also reporting unordered files.
let CompareProjectsTo templateProject projects =
    let errors =
        findMissingFiles templateProject projects
        |> Seq.map (fun pc ->
                seq {
                    if Seq.isEmpty pc.MissingFiles |> not then
                        yield sprintf "Missing files in %s:\r\n%s" pc.ProjectFileName (toLines pc.MissingFiles)
                    if Seq.isEmpty pc.UnorderedFiles |> not then
                        yield sprintf "Unordered files in %s:\r\n%s" pc.ProjectFileName (toLines pc.UnorderedFiles)
                    if Seq.isEmpty pc.DuplicateFiles |> not then
                        yield sprintf "Duplicate files in %s:\r\n%s" pc.ProjectFileName (toLines pc.DuplicateFiles)}
                    |> toLines)
        |> toLines

    if isNotNullOrEmpty errors then failwith errors
#if INTERACTIVE
;; readfsproj ^ __SOURCE_DIRECTORY__ + "/../Forge/Forge.fsproj"
#endif
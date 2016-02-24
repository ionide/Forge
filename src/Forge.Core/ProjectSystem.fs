#if INTERACTIVE
/// Contains project file comparion tools for MSBuild project files.
#r "System.Xml"
#r "System.Xml.Linq"
#load "Prelude.fs"
#load "Extensions.fs"
open Forge.Prelude
open Forge.Extensions
#else
module Forge.ProjectSystem
#endif


open System
open System.Collections.Generic
open System.Xml
open System.Xml.Linq

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
    | X86 |  X64 | AnyCpu
    override self.ToString () =
        match self with
        | X86                  -> "x86" 
        | X64                  -> "x64"
        | AnyCpu               -> "AnyCPU"


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
    override self.ToString() = self |> function
        | Compile          -> "Compile"
        | Content          -> "Content"
        | Reference        -> "Reference"
        | None             -> "None"
        | Resource         -> "Resource"
        | EmbeddedResource -> "EmbeddedResource"


// Under "Compile" in https://msdn.microsoft.com/en-us/library/bb629388.aspx
type CopyToOutputDirectory =
    | Never
    | Always
    | PreserveNewest    
    override self.ToString() = self |> function
        | Never -> "Never"
        | Always -> "Always"
        | PreserveNewest -> "PreserveNewest"


[<RequireQualifiedAccess>]
type DebugType =
    | None
    | PdbOnly
    | Full


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
    override self.ToString () =
        match self with
        | Exe     -> "Exe"
        | Winexe  -> "Winexe"
        | Library -> "Library"
        | Module  -> "Module"




// Common MSBuild Project Items
// https://msdn.microsoft.com/en-us/library/bb629388.aspx

type Reference =
    {   Include : string
        /// Relative or absolute path of the assembly
        HintPath : string option
        /// Optional string. The display name of the assembly, for example, "System.Windows.Forms."
        Name : string option
        /// Optional boolean. Specifies whether only the version in the fusion name should be referenced.
        SpecificVesion : bool option
        /// Optional boolean. Specifies whether the reference should be copied to the output folder. 
        /// This attribute matches the Copy Local property of the reference that's in the Visual Studio IDE.                 
        // if CopyLocal is true shown as "<Private>false</Private>" in XML)
        CopyLocal : bool option
    } 

        


/// Represents a reference to another project
// https://msdn.microsoft.com/en-us/library/bb629388.aspx
type ProjectReference =
    {   /// Path to the project file to include
        /// translates to the `Include` attribute in MSBuild XML
        Path : string
        /// Optional string. The display name of the reference.
        Name : string option
        /// Optional Guid of the referenced project
        // will be project in the MSBuild XML
        Guid : Guid option
        /// Should the assemblies of this project be copied Locally 
        // if CopyLocal is true shown as "<Private>false</Private>" in XML)
        CopyLocal : bool option
    }

type SourceFile =
    {   Path    : string
        Link    : string
        Copy    : CopyToOutputDirectory
        OnBuild : BuildAction
    }


type SourcePair =
    {   Module  : SourceFile
        Sig     : SourceFile
    }


type SourceElement =
    | File      of SourceFile
    | Pair      of SourcePair
    | Directory of SourceElement list





type ProjectSettings =
    {   Name : string
        AssemblyName : string
        RootNamespace : string
        Configuration : string
        Platform : string
        SchemaVersion : decimal
        ProjectGuid : Guid
        ProjectType : Guid list option
        OutputType : OutputType
        TargetFrameworkVersion : string
        AutoGenerateBindingRedirects : bool
        TargetFSharpCoreVersion :string
    }


type ConfigurationSettings =
    {   Condition            : string
        DebugSymbols         : bool
        DebugType            : string
        Optimize             : bool
        Tailcalls            : bool
        OutputPath           : string
        CompilationConstants : string list
        WarningLevel         : int
        PlatformTarget       : PlatformType
        Documentationfile    : string
        Prefer32Bit          : bool
    }


    

type FsProject =
    {   References          : Reference list
        ProjectReferences   : ProjectReference list
        SourceFiles         : SourceElement list
        Settings            : ProjectSettings    
    
    }    
/// A small abstraction over MSBuild project files.
type ProjectFile(projectFileName:string,documentContent : string) =
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
        let utf8 = new System.Text.UTF8Encoding(false)
        let settings = new XmlWriterSettings()
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

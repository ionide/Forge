/// Contains project file comparion tools for MSBuild project files.
module Fix.ProjectSystem

open Fake
open System.Collections.Generic
open System.Xml
open System.Xml.Linq
open XMLHelper

/// A small abstraction over MSBuild project files.
type ProjectFile(projectFileName:string,documentContent : string) =
    let document = XMLDoc documentContent

    let nsmgr =
        let nsmgr = new XmlNamespaceManager(document.NameTable)
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
                let node = updatedXml.CreateElement(n1.Name, n1.LocalName, n1.NamespaceURI)
                n2.ParentNode.InsertBefore(node, n2) |> ignore

                new ProjectFile(projectFileName, updatedXml.OuterXml)

            | None -> new ProjectFile(projectFileName,document.OuterXml)
        | _ -> new ProjectFile(projectFileName,document.OuterXml)

    /// Read a Project from a FileName
    static member FromFile(projectFileName) = new ProjectFile(projectFileName,ReadFileAsString projectFileName)

    /// Saves the project file
    member x.Save(?fileName) = document.Save(defaultArg fileName projectFileName)

    member x.Content =
        use stringWriter = new System.IO.StringWriter()
        document.Save(stringWriter)
        stringWriter.ToString()



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
        tracefn "Adding %s to %s" missingFile project.ProjectFileName
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

    if isNotNullOrEmpty errors then
        failwith errors

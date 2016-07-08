module Forge.ProjectManager

open System
open System.Text
open System.IO
open System.Xml
open System.Xml.Linq
open Forge
open Forge.ProjectValidation
open Forge.ProjectSystem
open Forge.ProjectSystem.PathHelpers



type ActiveState =
    {   StoredXml       : XElement seq
        ProjectData     : FsProject
        ProjectPath     : string
        ProjectFileName : string
        ActiveConfig    : ConfigSettings
    }

// Maybe use a persistent vector here to allow timetravel/history & undo?


let saveState (state:ActiveState) =
    File.WriteAllText(
        state.ProjectPath </> state.ProjectFileName + ".fsproj",
        state.ProjectData.ToXmlString state.StoredXml
    )


let updateProj projfn (state:ActiveState) =
    let state = { state with ProjectData = projfn state.ProjectData  }
    saveState state
    state


    // The furnace is the internal workhorse that handles the orchestration of manipulating
// the project and solution files, making changes to the file system, finding the source of
// errors and surfacing them up to the user
[<RequireQualifiedAccess>]
module Furnace =

    let loadFsProject (projectPath: string) =
        use reader = XmlReader.Create projectPath
        let xdoc   = reader |> XDocument.Load
        // hold onto the xml content we're not using so it doesn't get lost
        let detritus =
            xdoc.Root |> XElem.elements
            |> Seq.filter (fun (xelem:XElement) ->
                xelem
                |>( XElem.notNamed Constants.Project
                |&| XElem.notNamed Constants.PropertyGroup
                |&| XElem.notNamed Constants.ItemGroup
                |&| XElem.notNamed Constants.ProjectReference
                )
            )
        let proj = FsProject.fromXDoc xdoc

        let projectPath' =
            match Path.GetDirectoryName projectPath with
            | "" -> Environment.CurrentDirectory
            | p  -> Environment.CurrentDirectory </> p
        // TODO - This is a bad way to deal with loading the configuration settings

        let config = proj.BuildConfigs |> function [] -> ConfigSettings.Debug | hd::_ -> hd
        {   StoredXml       = List.ofSeq detritus
            ProjectData     = proj
            ProjectPath     = projectPath'
            ProjectFileName = Path.GetFileNameWithoutExtension projectPath
            ActiveConfig    = config
        }


    let addReference (includestr: string, condition: string option, hintPath: string option, name: string option, specificVersion: bool option, copy: bool option) (state: ActiveState) =
        let asmName = String.takeUntil ',' includestr
        let project = state.ProjectData
        let r = project.References |> ResizeArray.tryFind (fun refr ->
            (refr.Name.IsSome && refr.Name.Value = asmName) ||
            (String.takeUntil ','  refr.Include = asmName )
        )
        let projectName = defaultArg project.Settings.Name.Data "fsproject"
        match r with
        | Some _ ->
            traceWarning ^ sprintf "'%s' already has a Reference for '%s'" projectName asmName
            state
        | None ->
            let reference = {
                Include         = includestr
                Condition       = condition
                HintPath        = hintPath
                Name            = name
                SpecificVersion = specificVersion
                CopyLocal       = copy
                Paket           = None
            }
            FsProject.addReference  reference project |> ignore
            updateProj (FsProject.addReference reference) state


    let removeReference (refname:string) (state: ActiveState)  =
        let project = state.ProjectData
        let r = project.References |> ResizeArray.tryFind (fun refr ->
            (refr.Name.IsSome && refr.Name.Value = refname) ||
            (String.takeUntil ','  refr.Include = refname )
        )
        let projectName = defaultArg project.Settings.Name.Data "fsproject"
        match r with
        | None ->
            traceWarning ^ sprintf "'%s' does not contain a Reference for '%s'" projectName refname
            state
        | Some reference ->
            FsProject.removeReference reference project |> ignore
            updateProj (FsProject.removeReference reference) state

    let addProjectReference (path : string, name : string option, condition : string option, guid : Guid option, copyLocal : bool option) (state: ActiveState) =
        let path = if path.StartsWith "." then path else relative path (state.ProjectPath + Path.DirectorySeparatorChar.ToString())
        let projRef = {
            Include = path
            Condition = condition
            Name = name
            CopyLocal = copyLocal
            Guid = guid
        }
        FsProject.addProjectReference projRef state.ProjectData |> ignore
        updateProj (FsProject.addProjectReference projRef) state

    let removeProjectReference (path : string) (state: ActiveState) =
        let project = state.ProjectData
        let r = project.ProjectReferences |> ResizeArray.tryFind (fun refr -> refr.Include = path)
        let path = if path.StartsWith "." then path else relative path (state.ProjectPath + Path.DirectorySeparatorChar.ToString())

        let projectName = defaultArg project.Settings.Name.Data "fsproject"
        match r with
        | None ->
            traceWarning ^ sprintf "'%s' does not contain a project Reference for '%s'" projectName path
            state
        | Some reference ->
            FsProject.removeProjectReference reference project |> ignore
            updateProj (FsProject.removeProjectReference reference) state


    let moveUp (target: string) (state: ActiveState) =
        updateProj (FsProject.moveUp target)  state


    let moveDown (target:string) (state: ActiveState) =
        updateProj (FsProject.moveDown target)  state


    let addAbove (target: string, file: string, onBuild: BuildAction option, link: string option, copy: CopyToOutputDirectory option, condition: string option) (state: ActiveState) =
        let dir = getParentDir target
        let onBuild = defaultArg onBuild BuildAction.Compile
        let srcFile =
            {   Include     = dir </> file
                Condition   = condition
                OnBuild     = onBuild
                Link        = link
                Copy        = copy
                Paket       = None
            }
        updateProj (FsProject.addAbove target srcFile)  state


    let addBelow (target: string, file: string, onBuild: BuildAction option, link: string option, copy: CopyToOutputDirectory option, condition: string option) (state: ActiveState) =
        let dir = getParentDir target
        let onBuild = defaultArg onBuild BuildAction.Compile
        let srcFile =
            {   Include     = dir </> file
                Condition   = condition
                OnBuild     = onBuild
                Link        = link
                Copy        = copy
                Paket       = None
            }
        updateProj (FsProject.addBelow target srcFile)  state


    let addSourceFile (file: string, dir :string option, onBuild: BuildAction option, linkPath: string option, copy: CopyToOutputDirectory option, condition: string option) (state: ActiveState)=
        let dir = defaultArg dir ""
        let onBuild = defaultArg onBuild BuildAction.Compile
        let srcFile =
            {   Include     = dir </> file
                Condition   = condition
                OnBuild     = onBuild
                Link        = linkPath
                Copy        = copy
                Paket       = None
            }
        updateProj (FsProject.addSourceFile dir srcFile)  state


    let removeSourceFile  (path:string) (state: ActiveState) =
        updateProj (FsProject.removeSourceFile path)  state


    let deleteSourceFile (path:string) (state: ActiveState) =
        if not ^ File.Exists path then
            traceError ^ sprintf "Cannot Delete File - '%s' does not exist" path
            state
        else
            deleteFile path
            removeSourceFile path state


    let removeDirectory (path:string) (state: ActiveState) =
        updateProj (FsProject.removeDirectory path)  state


    let deleteDirectory (path:string) (state: ActiveState) =
        if not ^ directoryExists path then
            traceError ^ sprintf "Cannot Delete Directory - '%s' does not exist" path
            state
        else
            deleteDir path
            removeDirectory path  state


    let renameDirectory (path:string, newName:string) (state: ActiveState) =
        let fullOldPath = state.ProjectPath </> path
        let fullNewPath = state.ProjectPath </> newName

        let (|Physical|Virtual|None|) directory =
            let sourceFiles =
                state.ProjectData.SourceFiles.DirContents ^ normalizeFileName directory
                |> Seq.map (fun file -> state.ProjectData.SourceFiles.Data.[file])
                |> List.ofSeq

            if sourceFiles.IsEmpty then None
            elif sourceFiles |> List.exists (fun f -> f.Link.IsNone) then Physical
            else Virtual

        let rename() = updateProj (FsProject.renameDir path newName) state

        match path with
        | Physical ->
            if directoryExists fullOldPath then
                renameDir fullNewPath fullOldPath
                rename()
            else
                traceError ^ sprintf "Cannot Rename Directory - directory '%s' does not exist on disk" fullOldPath
                state
        | Virtual -> rename()
        | None ->
            traceError ^ sprintf "Cannot Rename Directory - directory '%s' does not exist in the project" path
            state


    let renameSourceFile (path:string, newName:string) (state: ActiveState) =
        if not ^ File.Exists path then
            traceError ^ sprintf "Cannot Rename File - '%s' does not exist" path
            state
        else
            renameFile path newName
            updateProj (FsProject.renameFile path newName)  state


    let listSourceFiles (filter: string option) (state: ActiveState) =
        let filterFn =
            match filter with
            | Some s -> (fun fileName -> (String.editDistance fileName s) < 5)
            | None   -> (fun _ -> true)
        FsProject.listSourceFiles state.ProjectData
        |> List.filter filterFn
        |> List.iter trace
        state


    let listReferences (filter: string option) (state: ActiveState) =
        let filterFn =
            match filter with
            | Some s -> (fun fileName -> (String.editDistance fileName s) < 5)
            | None   -> (fun _ -> true)
        FsProject.listReferences state.ProjectData
        |> List.filter filterFn
        |> List.iter trace
        state

    let listProjectReferences (filter: string option) (state: ActiveState) =
        let filterFn =
            match filter with
            | Some s -> (fun fileName -> (String.editDistance fileName s) < 5)
            | None   -> (fun _ -> true)
        FsProject.listProjectReferences state.ProjectData
        |> List.filter filterFn
        |> List.iter trace
        state

    let rec tryFindProject dir =
        try
            let dir =
                try
                    if (File.GetAttributes(dir).HasFlag FileAttributes.Directory |> not) then System.IO.Path.GetDirectoryName dir else dir
                with
                | _ -> System.IO.Path.GetDirectoryName dir

            match Globbing.search dir "*.fsproj" |> List.tryHead with
            | Some f -> Some f
            | None ->
                if dir = directory then None
                else dir |> System.IO.Directory.GetParent |> fun n -> n.FullName |> tryFindProject
        with
        | _ -> None

    let renameProject (name:string, newName:string) (state: ActiveState) =
        let proj = tryFindProject name
        if proj.IsNone then
            traceError ^ sprintf "Cannot Rename Project - '%s' does not exist" name
            state
        else
            updateProj (FsProject.renameProject newName) state




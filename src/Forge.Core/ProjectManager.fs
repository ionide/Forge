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
        state.ProjectPath </> state.ProjectFileName, 
        state.ProjectData.ToXmlString state.StoredXml
    )


let updateProj projfn (state:ActiveState) =
    let state = { state with ProjectData = projfn state.ProjectData  }
    saveState state
    state


let readFsProject path =
    use reader = XmlReader.Create (path:string)
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
    
    let projectPath = 
        match Path.GetDirectoryName path with
        | "" -> Environment.CurrentDirectory
        | p  -> Environment.CurrentDirectory </> p
    // TODO - This is a bad way to deal with loading the configuration settings

    let config = proj.BuildConfigs |> function [] -> ConfigSettings.Debug | hd::_ -> hd
    {   StoredXml       = List.ofSeq detritus
        ProjectData     = proj
        ProjectPath     = projectPath
        ProjectFileName = Path.GetFileName path
        ActiveConfig    = config
    }

// The furnace is the internal workhorse that handles the orchestration of manipulating 
// the project and solution files, making changes to the file system, finding the source of
// errors and surfacing them up to the user
type Furnace =

    static member init (projectPath:string) =
        readFsProject projectPath


    static member addReference 
        (state: ActiveState, includestr:string,?condition:string,?hintPath:string,?name:string,?specificVersion:bool,?copy:bool) =
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
            }
            FsProject.addReference  reference project |> ignore
            updateProj (FsProject.addReference reference) state 
            

    static member removeReference (refname:string) (state: ActiveState)  =
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



    static member moveUp (target: string) (state: ActiveState) =
        updateProj (FsProject.moveUp target)  state



    static member moveDown (target:string) (state: ActiveState) =
        updateProj (FsProject.moveDown target)  state


    static member addAbove
       ( state: ActiveState, target: string, file: string,
            ?onBuild: BuildAction, ?link: string, ?copy: CopyToOutputDirectory, ?condition: string) =
        let dir = getParentDir target
        let onBuild = defaultArg onBuild BuildAction.Compile
        let srcFile =
            {   Include     = dir </> file
                Condition   = condition
                OnBuild     = onBuild
                Link        = link
                Copy        = copy
            }
        updateProj (FsProject.addAbove target srcFile)  state


    static member addBelow
       ( state: ActiveState, target: string, file: string,
            ?onBuild: BuildAction, ?link: string, ?copy: CopyToOutputDirectory, ?condition: string) =
        let dir = getParentDir target
        let onBuild = defaultArg onBuild BuildAction.Compile
        let srcFile =
            {   Include     = dir </> file
                Condition   = condition
                OnBuild     = onBuild
                Link        = link
                Copy        = copy
            }
        updateProj (FsProject.addBelow target srcFile)  state


    static member addSourceFile
       ( state: ActiveState, file: string, ?dir:string,
            ?onBuild: BuildAction, ?linkPath: string, ?copy: CopyToOutputDirectory, ?condition: string) =
        let dir = defaultArg dir ""
        let onBuild = defaultArg onBuild BuildAction.Compile
        let srcFile =
            {   Include     = dir </> file
                Condition   = condition
                OnBuild     = onBuild
                Link        = linkPath
                Copy        = copy
            }
        updateProj (FsProject.addSourceFile dir srcFile)  state



    static member removeSourceFile  (path:string) (state: ActiveState) =
        updateProj (FsProject.removeSourceFile path)  state



    static member deleteSourceFile (path:string) (state: ActiveState) =
        if not ^ File.Exists path then
            traceError ^ sprintf "Cannot Delete File - '%s' does not exist" path
            state
        else
            deleteFile path
            Furnace.removeSourceFile path state
        

    static member removeDirectory (path:string) (state: ActiveState) =
        updateProj (FsProject.removeDirectory path)  state



    static member deleteDirectory (path:string) (state: ActiveState) =
        if not ^ directoryExists path then
            traceError ^ sprintf "Cannot Delete Directory - '%s' does not exist" path
            state
        else
            deleteDir path
            Furnace.removeDirectory path  state


    static member renameDirectory (path:string)  (newName:string) (state: ActiveState) =
        if not ^ directoryExists path then
            traceError ^ sprintf "Cannot Rename Directory - '%s' does not exist" path
            state
        else
            renameDir path newName
            updateProj (FsProject.renameDir path newName)  state
            

    static member renameSourceFile (path:string) (newName:string) (state: ActiveState) =
        if not ^ File.Exists path then
            traceError ^ sprintf "Cannot Rename File - '%s' does not exist" path
            state
        else
            renameFile path newName
            updateProj (FsProject.renameFile path newName)  state



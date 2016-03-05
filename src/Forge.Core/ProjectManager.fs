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
        Project         : FsProject
        ProjectPath     : string
        ActiveConfig    : ConfigSettings
    }

// Maybe use a persistent vector here to allow timetravel/history & undo?


let saveState (state:ActiveState) =
    File.WriteAllText(state.ProjectPath, state.Project.ToXmlString state.StoredXml)


let updateProj projfn (state:ActiveState) =
    { state with Project = projfn state.Project  }


let readFsProject path =
    use reader = XmlReader.Create (path:string)
    let xdoc   = reader |> XDocument.Load
    let detritus =
        xdoc.Root |> XElem.elements
        |> Seq.filter (fun (xelem:XElement) ->
            xelem
            |>( XElem.isNamed Constants.Project
            |?| XElem.isNamed Constants.PropertyGroup
            |?| XElem.isNamed Constants.ItemGroup
            |?| XElem.isNamed Constants.ProjectReference
            )
        )
    let proj = FsProject.fromXDoc xdoc
    
    // TODO - This is a bad way to deal with loading the configuration settings

    let config = proj.BuildConfigs |> function [] -> ConfigSettings.Debug | hd::_ -> hd
    {   StoredXml       = detritus
        Project         = proj
        ProjectPath     = path
        ActiveConfig    = config
    }


type Furnace =

    static member init (projectPath:string) =
        readFsProject projectPath

    static member addReference 
        (state: ActiveState, includestr:string,?condition:string,?hintPath:string,?name:string,?specificVersion:bool,?copy:bool) =
        let asmName = String.takeUntil ',' includestr
        let project = state.Project
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
            let state = updateProj (FsProject.addReference reference) state 
            saveState state
            state


    static member removeReference (refname:string) (state: ActiveState)  =
        let project = state.Project
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
            let state = updateProj (FsProject.removeReference reference) state 
            saveState state
            state


    static member moveUp (target: string) (state: ActiveState) =
        Environment.CurrentDirectory <- Path.GetDirectoryName state.ProjectPath
        let state = updateProj (FsProject.moveUp target)  state
        saveState state
        state


    static member moveDown (target:string) (state: ActiveState) =
        Environment.CurrentDirectory <- Path.GetDirectoryName state.ProjectPath
        let state = updateProj (FsProject.moveDown target)  state
        saveState state
        state


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
        Environment.CurrentDirectory <- Path.GetDirectoryName state.ProjectPath
        createFile (dir </> file)
        let state = updateProj (FsProject.addAbove target srcFile)  state
        saveState state
        state


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
        Environment.CurrentDirectory <- Path.GetDirectoryName state.ProjectPath
        createFile (dir </> file)
        let state = updateProj (FsProject.addBelow target srcFile)  state
        saveState state
        state


    static member addSourceFile
       ( state: ActiveState, dir: string, file: string,
            ?onBuild: BuildAction, ?link: string, ?copy: CopyToOutputDirectory, ?condition: string) =

        let onBuild = defaultArg onBuild BuildAction.Compile
        let srcFile =
            {   Include     = dir </> file
                Condition   = condition
                OnBuild     = onBuild
                Link        = link
                Copy        = copy
            }
        Environment.CurrentDirectory <- Path.GetDirectoryName state.ProjectPath
        createFile (dir </> file)
        let state = updateProj (FsProject.addSourceFile dir srcFile)  state
        saveState state
        state


    static member removeSourceFile  (path:string) (state: ActiveState) =
        Environment.CurrentDirectory <- Path.GetDirectoryName state.ProjectPath
        let state = updateProj (FsProject.removeSourceFile path)  state
        saveState state
        state


    static member deleteSourceFile (path:string) (state: ActiveState) =
        Environment.CurrentDirectory <- Path.GetDirectoryName state.ProjectPath
        deleteFile path
        let state = updateProj (FsProject.removeSourceFile path)  state
        saveState state
        state


    static member removeDirectory (path:string) (state: ActiveState) =
        Environment.CurrentDirectory <- Path.GetDirectoryName state.ProjectPath
        let state = updateProj (FsProject.removeDirectory path)  state
        saveState state
        state


    static member deleteDirectory (path:string) (state: ActiveState) =
        Environment.CurrentDirectory <- Path.GetDirectoryName state.ProjectPath
        deleteDir path
        let state = updateProj (FsProject.removeDirectory path)  state
        saveState state
        state


    static member renameDirectory (target:string)  (newName:string) (state: ActiveState) =
        Environment.CurrentDirectory <- Path.GetDirectoryName state.ProjectPath
        renameDir target newName
        let state = updateProj (FsProject.renameDir target newName)  state
        saveState state
        state


    static member renameSourceFile (target:string) (newName:string) (state: ActiveState) =
        Environment.CurrentDirectory <- Path.GetDirectoryName state.ProjectPath
        renameFile target newName
        let state = updateProj (FsProject.renameFile target newName)  state
        saveState state
        state


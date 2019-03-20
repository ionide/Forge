// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------
#r "paket: groupref build //"
#load ".fake/build.fsx/intellisense.fsx"
#if !FAKE
  #r "netstandard"
  #r "Facades/netstandard.dll"
#endif

open Fake.Core
open Fake.DotNet
open Fake.Tools
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators
open Fake.Api

// --------------------------------------------------------------------------------------
// Information about the project to be used at NuGet and in AssemblyInfo files
// --------------------------------------------------------------------------------------

let project = "Forge"
let summary = "Forge is a build tool that provides tasks for creating, compiling, and testing F# projects"

let gitOwner = "ionide"
let gitHome = "https://github.com/" + gitOwner
let gitName = "Forge"
let gitRaw = Environment.environVarOrDefault "gitRaw" ("https://raw.github.com/" + gitOwner)

// --------------------------------------------------------------------------------------
// Build variables
// --------------------------------------------------------------------------------------

let dotnetcliVersion = DotNet.getSDKVersionFromGlobalJson()
System.Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
let release = ReleaseNotes.parse (System.IO.File.ReadAllLines "RELEASE_NOTES.md")

let packageDir = __SOURCE_DIRECTORY__ </> "out"
let buildDir = __SOURCE_DIRECTORY__ </> "temp"
let forgeSh = "./forge.sh"


// --------------------------------------------------------------------------------------
// Helpers
// --------------------------------------------------------------------------------------
let isNullOrWhiteSpace = System.String.IsNullOrWhiteSpace
let exec cmd args dir =
    if Process.execSimple( fun info ->

        { info with
            FileName = cmd
            WorkingDirectory =
                if (isNullOrWhiteSpace dir) then info.WorkingDirectory
                else dir
            Arguments = args
            }
    ) System.TimeSpan.MaxValue <> 0 then
        failwithf "Error while running '%s' with args: %s" cmd args
let getBuildParam = Environment.environVar

let DoNothing = ignore
// --------------------------------------------------------------------------------------
// Build Targets
// --------------------------------------------------------------------------------------

Target.create "Clean" (fun _ ->
    Shell.cleanDirs [buildDir; packageDir]
)

Target.create "AssemblyInfo" (fun _ ->
    let getAssemblyInfoAttributes projectName =
        [ AssemblyInfo.Title projectName
          AssemblyInfo.Product project
          AssemblyInfo.Description summary
          AssemblyInfo.Version release.AssemblyVersion
          AssemblyInfo.FileVersion release.AssemblyVersion ]

    let getProjectDetails projectPath =
        let projectName = System.IO.Path.GetFileNameWithoutExtension(projectPath)
        ( projectPath,
          projectName,
          System.IO.Path.GetDirectoryName(projectPath),
          (getAssemblyInfoAttributes projectName)
        )

    !! "src/**/*.??proj"
    |> Seq.map getProjectDetails
    |> Seq.iter (fun (projFileName, _, folderName, attributes) ->
        match projFileName with
        | proj when proj.EndsWith("fsproj") -> AssemblyInfoFile.createFSharp (folderName </> "AssemblyInfo.fs") attributes
        | proj when proj.EndsWith("csproj") -> AssemblyInfoFile.createCSharp ((folderName </> "Properties") </> "AssemblyInfo.cs") attributes
        | proj when proj.EndsWith("vbproj") -> AssemblyInfoFile.createVisualBasic ((folderName </> "My Project") </> "AssemblyInfo.vb") attributes
        | _ -> ()
        )
)

Target.create "InstallDotNetCLI" (fun _ ->
    let version = DotNet.CliVersion.Version dotnetcliVersion
    let options = DotNet.Options.Create()
    DotNet.install (fun opts -> { opts with Version = version }) options |> ignore
    )

Target.create "Restore" (fun _ ->
    DotNet.restore id ""
)

Target.create "Build" (fun _ ->
    DotNet.build id ""
)

Target.create "Publish" (fun _ ->
    DotNet.publish (fun p -> {p with OutputPath = Some buildDir}) "src/Forge"

)

Target.create "Test" (fun _ ->
    exec "dotnet"  @"run --project .\tests\Forge.Tests\Forge.Tests.fsproj" "."
    //exec "dotnet"  @"run --project .\tests\Forge.IntegrationTests\Forge.IntegrationTests.fsproj" "."
)

// --------------------------------------------------------------------------------------
// Release Targets
// --------------------------------------------------------------------------------------

Target.create "Pack" (fun _ ->

    //Pack Forge.Core
    Paket.pack (fun p ->
        { p with
            BuildConfig = "Release";
            OutputPath = packageDir;
            Version = release.NugetVersion
            ReleaseNotes = String.concat "\n" release.Notes
            MinimumFromLockFile = false
            ToolPath = ".paket/paket.exe"
        }
    )

    //Pack Forge global tool
    Environment.setEnvironVar "PackageVersion" release.NugetVersion
    Environment.setEnvironVar "Version" release.NugetVersion
    Environment.setEnvironVar "Authors" "Krzysztof Cieslak"
    Environment.setEnvironVar "Description" summary
    Environment.setEnvironVar "PackageReleaseNotes" (release.Notes |> String.toLines)
    Environment.setEnvironVar "PackageTags" "build;fake;f#"
    Environment.setEnvironVar "PackageProjectUrl" "https://github.com/ionide/Forge"
    Environment.setEnvironVar "PackageLicenseUrl" "https://raw.githubusercontent.com/ionide/Forge/master/LICENSE.txt"

    DotNet.pack (fun p ->
        { p with
            OutputPath = Some packageDir
            Configuration = DotNet.BuildConfiguration.Release
        }) "src/Forge"
)

Target.create "ReleaseGitHub" (fun _ ->
    let remote =
        Git.CommandHelper.getGitResult "" "remote -v"
        |> Seq.filter (fun (s: string) -> s.EndsWith("(push)"))
        |> Seq.tryFind (fun (s: string) -> s.Contains(gitOwner + "/" + gitName))
        |> function None -> gitHome + "/" + gitName | Some (s: string) -> s.Split().[0]

    Git.Staging.stageAll ""
    Git.Commit.exec "" (sprintf "Bump version to %s" release.NugetVersion)
    Git.Branches.pushBranch "" remote (Git.Information.getBranchName "")


    Git.Branches.tag "" release.NugetVersion
    Git.Branches.pushTag "" remote release.NugetVersion

    let client =
        let user =
            match getBuildParam "github-user" with
            | s when not (isNullOrWhiteSpace s) -> s
            | _ -> UserInput.getUserInput "Username: "
        let pw =
            match getBuildParam "github-pw" with
            | s when not (isNullOrWhiteSpace s) -> s
            | _ -> UserInput.getUserPassword "Password: "

        // Git.createClient user pw
        GitHub.createClient user pw
    let files = !! (packageDir </> "*.nupkg")

    // release on github
    let cl =
        client
        |> GitHub.draftNewRelease gitOwner gitName release.NugetVersion (release.SemVer.PreRelease <> None) release.Notes
    (cl,files)
    ||> Seq.fold (fun acc e -> acc |> GitHub.uploadFile e)
    |> GitHub.publishDraft//releaseDraft
    |> Async.RunSynchronously
)

Target.create "Push" (fun _ ->
    let key =
        match getBuildParam "nuget-key" with
        | s when not (isNullOrWhiteSpace s) -> s
        | _ -> UserInput.getUserPassword "NuGet Key: "
    Paket.push (fun p -> { p with WorkingDir = buildDir; ApiKey = key }))

// --------------------------------------------------------------------------------------
// Build order
// --------------------------------------------------------------------------------------
Target.create "Default" DoNothing
Target.create "Release" DoNothing

"Clean"
  ==> "InstallDotNetCLI"
  ==> "AssemblyInfo"
  ==> "Restore"
  ==> "Build"
  ==> "Publish"
  ==> "Test"
  ==> "Default"

"Default"
  ==> "Pack"
  ==> "ReleaseGitHub"
  ==> "Push"
  ==> "Release"

Target.runOrDefault "Default"

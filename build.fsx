// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------

#r @"packages/FAKE/tools/FakeLib.dll"
#load "paket-files/fsharp/FAKE/modules/Octokit/Octokit.fsx"
open Fake
open Fake.Git
open Fake.AssemblyInfoFile
open Fake.ReleaseNotesHelper
open Fake.UserInputHelper
open Fake.ZipHelper
open Fake.Testing
open System
open System.IO
open Octokit

// The name of the project
// (used by attributes in AssemblyInfo, name of a NuGet package and directory in 'src')
let project = "Forge"

// Short summary of the project
// (used as description in AssemblyInfo and as a short summary for NuGet package)
let summary = "Forge is a build tool that provides tasks for creating, compiling, and testing F# projects"

// Longer description of the project
// (used as a description for NuGet package; line breaks are automatically cleaned up)
let description = "Forge is a build tool that provides tasks for creating, compiling, and testing F# projects"

// List of author names (for NuGet package)
let authors = [ "Reid Evans"; "Krzysztof Cieslak"; "Jared Hester" ]

// Tags for your project (for NuGet package)
let tags = ""

// File system information
let projectFile  = "src/Forge/Forge.fsproj"
let testProjectFiles = "tests/**/*Tests.fsproj"

// Pattern specifying assemblies to be tested using NUnit
let testAssemblies = "temp/test/*Tests*.dll"

// Git configuration (used for publishing documentation in gh-pages branch)
// The profile where the project is posted
let gitOwner = "fsharp-editing"
let gitHome = "https://github.com/" + gitOwner

// The name of the project on GitHub
let gitName = "Forge"

// The url for the raw files hosted
let gitRaw = environVarOrDefault "gitRaw" ("https://raw.github.com/" + gitOwner)

// Read additional information from the release notes document
let release = LoadReleaseNotes "RELEASE_NOTES.md"

let tempDir = "temp"
let testBuildDir = "temp/test"
let buildDir = "temp/bin"

// Helper active pattern for project types
let (|Fsproj|Csproj|Vbproj|) (projFileName:string) =
    match projFileName with
    | f when f.EndsWith("fsproj") -> Fsproj
    | f when f.EndsWith("csproj") -> Csproj
    | f when f.EndsWith("vbproj") -> Vbproj
    | _                           -> failwith (sprintf "Project file %s not supported. Unknown project type." projFileName)

// Generate assembly info files with the right version & up-to-date information
Target "AssemblyInfo" (fun _ ->
    let getAssemblyInfoAttributes projectName =
        [ Attribute.Title (projectName)
          Attribute.Product project
          Attribute.Description summary
          Attribute.Version release.AssemblyVersion
          Attribute.FileVersion release.AssemblyVersion ]

    let getProjectDetails projectPath =
        let projectName = System.IO.Path.GetFileNameWithoutExtension(projectPath)
        ( projectPath,
          projectName,
          System.IO.Path.GetDirectoryName(projectPath),
          (getAssemblyInfoAttributes projectName)
        )

    !! "src/**/*.??proj" -- "src/**/templates/**/*.*"
    |> Seq.map getProjectDetails
    |> Seq.iter (fun (projFileName, projectName, folderName, attributes) ->
        match projFileName with
        | Fsproj -> CreateFSharpAssemblyInfo (folderName @@ "AssemblyInfo.fs") attributes
        | Csproj -> CreateCSharpAssemblyInfo ((folderName @@ "Properties") @@ "AssemblyInfo.cs") attributes
        | Vbproj -> CreateVisualBasicAssemblyInfo ((folderName @@ "My Project") @@ "AssemblyInfo.vb") attributes
        )
)

// --------------------------------------------------------------------------------------
// Clean build results

Target "Clean" (fun _ ->
    CleanDirs ["temp"; ]
)

// --------------------------------------------------------------------------------------
// Build library & test project

Target "Build" (fun _ ->
    !! projectFile
    |> MSBuildRelease buildDir "Rebuild"
    |> ignore

    !! (buildDir </> "*.dll")
    ++ (buildDir </> "*.pdb")
    ++ (buildDir </> "*.xml")
    -- (buildDir </> "Mono.Posix.dll")
    |> DeleteFiles
)

Target "BuildProjectSystem" (fun _ ->
    !! "src/Forge.ProjectSystem/Forge.ProjectSystem.fsproj"
    |> MSBuildRelease "temp/bin_projectSystem" "Rebuild"
    |> ignore

)

Target "CopyRunners" (fun _ ->
    CopyFiles "temp" ["runners/forge.cmd"; "runners/forge.sh"]
)

// --------------------------------------------------------------------------------------
// Run the unit tests using test runner

Target "BuildTests" (fun _ ->
    !! testProjectFiles
    |> MSBuildRelease testBuildDir "Rebuild"
    |> ignore)

Target "RunTests" (fun _ ->
    !! testAssemblies
    |> NUnit3 (fun p ->
        { p with
            ShadowCopy = true
            TimeOut = TimeSpan.FromMinutes 20.
            OutputDir = "TestResults.xml" })
)
// --------------------------------------------------------------------------------------
// Release Scripts

Target "ZipRelease" (fun _ ->
    !! (tempDir  </> "forge.sh")
    ++ (tempDir  </> "forge.cmd")
    ++ (buildDir </> "*.exe")
    ++ (buildDir </> "*.config")
    ++ (buildDir </> "Mono.Posix.dll")
    -- (buildDir </> "Forge.Core.dll.config")
    -- (buildDir </> "*templates*")
    -- (buildDir </> "*Tests*")
    ++ (buildDir </> "Tools" </> "Paket" </> "paket.bootstrapper.exe")
    |> Zip "temp" ("temp" </> "forge.zip")

)

Target "Release" (fun _ ->
    let user =
        match getBuildParam "github-user" with
        | s when not (String.IsNullOrWhiteSpace s) -> s
        | _ -> getUserInput "Username: "
    let pw =
        match getBuildParam "github-pw" with
        | s when not (String.IsNullOrWhiteSpace s) -> s
        | _ -> getUserPassword "Password: "
    let remote =
        Git.CommandHelper.getGitResult "" "remote -v"
        |> Seq.filter (fun (s: string) -> s.EndsWith("(push)"))
        |> Seq.tryFind (fun (s: string) -> s.Contains(gitOwner + "/" + gitName))
        |> function None -> gitHome + "/" + gitName | Some (s: string) -> s.Split().[0]

    StageAll ""
    Git.Commit.Commit "" (sprintf "Bump version to %s" release.NugetVersion)
    Branches.pushBranch "" remote (Information.getBranchName "")

    Branches.tag "" release.NugetVersion
    Branches.pushTag "" remote release.NugetVersion

    // release on github
    createClient user pw
    |> createDraft gitOwner gitName release.NugetVersion (release.SemVer.PreRelease <> None) release.Notes
    |> uploadFile "temp/forge.zip"
    |> releaseDraft
    |> Async.RunSynchronously
)


// --------------------------------------------------------------------------------------
// Run all targets by default. Invoke 'build <Target>' to override

Target "Default" DoNothing
Target "PaketBuild" DoNothing


"Clean"
  ==> "AssemblyInfo"
  ==> "BuildProjectSystem"

"Clean"
  ==> "AssemblyInfo"
  ==> "Build"
  ==> "CopyRunners"
  ==> "BuildTests"
  ==> "RunTests"
  ==> "Default"

"BuildTests"
  ==> "ZipRelease"
  ==> "Release"

"Build"
  ==> "ZipRelease"
  ==> "PaketBuild"


RunTargetOrDefault "Default"

module Forge.Git


open System
open System.Diagnostics
open System.IO
open System.Threading  
open System.Text
open System.Collections.Generic
open Fake

/// Specifies a global timeout for git.exe - default is *no timeout*
let mutable gitTimeOut = TimeSpan.MaxValue

let private GitPath = @"[ProgramFiles]\Git\cmd\;[ProgramFilesX86]\Git\cmd\;[ProgramFiles]\Git\bin\;[ProgramFilesX86]\Git\bin\;"

/// Tries to locate the git.exe via the eviroment variable "GIT".
let gitPath = 
    if isUnix then
        "git"
    else
        let ev = environVar "GIT"
        if not (isNullOrEmpty ev) then ev else findPath "GitPath" GitPath "git.exe"   

/// Runs git.exe with the given command in the given repository directory.
let runGitCommand repositoryDir command = 
    run gitPath repositoryDir command
//    let processResult = 
//        ExecProcessAndReturnMessages (fun info ->  
//          info.FileName <- gitPath
//          info.WorkingDirectory <- repositoryDir
//          info.Arguments <- command) gitTimeOut
//
//    processResult.OK,processResult.Messages,toLines processResult.Errors

/// [omit]
let runGitCommandf fmt = Printf.ksprintf runGitCommand fmt

/// Runs the git command and returns the first line of the result.
let runSimpleGitCommand repositoryDir command =
    try
        runGitCommand repositoryDir command
//               
//        let errorText = toLines msg + Environment.NewLine + errors
//        if errorText.Contains "fatal: " then
//            failwith errorText
//
//        if msg.Count = 0 then "" else
//        msg |> Seq.iter (logfn "%s")
//        msg.[0]
    with 
    | exn -> failwithf "Could not run \"git %s\".\r\nError: %s" command exn.Message

/// Clones a single branch of a git repository.
/// ## Parameters
///
///  - `workingDir` - The working directory.
///  - `repoUrl` - The URL to the origin.
///  - `branchname` - Specifes the target branch.
///  - `toPath` - Specifes the new target subfolder.
let cloneSingleBranch workingDir repoUrl branchName toPath =
    sprintf "clone -b %s --single-branch %s %s" branchName repoUrl toPath
    |> runSimpleGitCommand workingDir



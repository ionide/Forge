module FixLib
open System

let Help () =
    printfn"Fix (Mix for F#)\n\
            Available Commands:\n\n\
            new [projectName] [projectDir] [templateName] [--no-paket] - Creates a new project\
          \n                      with the given name, in given directory\
          \n                      (relative to working directory) and given template.\
          \n                      If parameters are not provided, program prompts user for them\n\
            file add [fileName] - Adds a file to the current folder and project.\
          \n                      If more than one project is in the current\
          \n                      directory you will be prompted which to use.\n\
            file remove [fileName]\
          \n                    - Removes the filename from disk and the project.\
          \n                      If more than one project is in the current\
          \n                      directory you will be prompted which to use.\n\
            file list           - List all files\n\
            file order [file1] [file2]\
          \n                    - Moves file1 immediately before file2 in the project.
          \n                      If more than one project is in the current\
          \n                      directory you will be prompted which to use.\n\
            reference add [reference]\
          \n                    - Add reference to the current project.\
          \n                      If more than one project is in the current\
          \n                      directory you will be prompted which to use.\n\
            reference remove [reference]\
                                - Remove reference from the current project.\
          \n                      If more than one project is in the current\
          \n                      directory you will be prompted which to use.\n\
            reference list      - list all references\n\
            update paket        - Updates Paket to latest version\n\
            update fake         - Updates FAKE to latest version\n\
            paket [args]        - Runs Paket with given arguments\n\
            fake [args]         - Runs FAKE with given arguments\n\
            refresh             - Refreshes the template cache\n\
            help                - Displays this help\n\
            exit                - Exit interactive mode\n"


let rec consoleLoop f =
    Console.Write("> ")
    let input = Console.ReadLine()
    let result = input.Split(' ') |> Array.toList |> f
    if result > 0
    then result
    else consoleLoop f

//TODO: Better input handling, maybe Argu ?
let handleInput = function
    | [ "new" ] -> Project.New "" "" "" true; 1
    | [ "new"; "--no-paket" ] -> Project.New "" "" "" false; 1
    | [ "new"; projectName ] -> Project.New projectName "" "" true; 1
    | [ "new"; projectName; "--no-paket"] -> Project.New projectName "" "" false; 1
    | [ "new"; projectName; projectDir ] -> Project.New projectName projectDir "" true; 1
    | [ "new"; projectName; projectDir; "--no-paket" ] -> Project.New projectName projectDir "" false; 1
    | [ "new"; projectName; projectDir; templateName ] -> Project.New projectName projectDir templateName true; 1
    | [ "new"; projectName; projectDir; templateName; "--no-paket" ] -> Project.New projectName projectDir templateName false; 1

    | [ "file"; "add"; fileName ] -> Files.Add fileName; 0
    | [ "file"; "remove"; fileName ] -> Files.Remove fileName; 0
    | [ "file"; "list"] -> Files.List(); 0
    | [ "file"; "order"; file1; file2 ] -> Files.Order file1 file2; 0

    | [ "reference"; "add"; fileName ] -> References.Add fileName; 0
    | [ "reference"; "remove"; fileName ] -> References.Remove fileName; 0
    | [ "reference"; "list"] -> References.List(); 0

    | [ "update"; "paket"] -> Paket.Update (); 0
    | [ "update"; "fake"] -> Fake.Update (); 0
    | "paket"::xs -> Paket.Run xs; 0
    | "fake"::xs -> Fake.Run xs; 0
    | [ "refresh" ] -> Templates.Refresh(); 0
    | [ "exit" ] -> 1
    | _ -> Help(); 0

[<EntryPoint>]
let main argv =
    if argv |> Array.isEmpty
    then
        Help ()
        consoleLoop handleInput
    else handleInput (argv |> Array.toList)

module Forge.References
open Forge.ProjectSystem

let Add reference =
    Project.execOnProject(fun x -> x.AddReference reference)

let Remove reference = 
    Project.execOnProject(fun x -> x.RemoveReference reference)
    
let List () =
    let listReferencesOfProject (project:ProjectFile) =
        project.References
        |> List.iter (printfn "%s")
        project

    Project.execOnProject listReferencesOfProject
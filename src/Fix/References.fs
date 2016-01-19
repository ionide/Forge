module References
open Fix.ProjectSystem

let Add reference =
    Project.execOnProject(fun x -> x.AddReference reference)

let Remove reference = 
    Project.execOnProject(fun x -> x.RemoveReference reference)
    
let List () =
    let listReferencesOfProject (project:ProjectFile) =
        project.References
        |> List.iter (fun i -> printfn "%s" i)
        project

    Project.execOnProject listReferencesOfProject
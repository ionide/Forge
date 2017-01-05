namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Forge.ProjectSystem")>]
[<assembly: AssemblyProductAttribute("Forge")>]
[<assembly: AssemblyDescriptionAttribute("Forge is a build tool that provides tasks for creating, compiling, and testing F# projects")>]
[<assembly: AssemblyVersionAttribute("1.3.3")>]
[<assembly: AssemblyFileVersionAttribute("1.3.3")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.3.3"
    let [<Literal>] InformationalVersion = "1.3.3"

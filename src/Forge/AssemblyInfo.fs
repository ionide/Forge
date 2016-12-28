namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Forge")>]
[<assembly: AssemblyProductAttribute("Forge")>]
[<assembly: AssemblyDescriptionAttribute("Forge is a build tool that provides tasks for creating, compiling, and testing F# projects")>]
[<assembly: AssemblyVersionAttribute("1.3.2")>]
[<assembly: AssemblyFileVersionAttribute("1.3.2")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.3.2"
    let [<Literal>] InformationalVersion = "1.3.2"

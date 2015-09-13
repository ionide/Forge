namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Fix")>]
[<assembly: AssemblyProductAttribute("Fix")>]
[<assembly: AssemblyDescriptionAttribute("Fix is a build tool that provides tasks for creating, compiling, and testing F# projects")>]
[<assembly: AssemblyVersionAttribute("1.0")>]
[<assembly: AssemblyFileVersionAttribute("1.0")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.0"

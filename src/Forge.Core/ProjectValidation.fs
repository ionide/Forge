
#if INTERACTIVE
/// Contains project file comparion tools for MSBuild project files.
#r "System.Xml"
#r "System.Xml.Linq"
#load "Prelude.fs"
#load "XLinq.fs"
open Forge.Prelude
open Forge.XLinq
#else
module Forge.ProjectValidation
#endif


open System
open System.Xml
open System.Xml.Linq
open System.Xml.Schema
open System.Reflection

let [<Literal>] MSBuildNamespace = "http://schemas.microsoft.com/developer/msbuild/2003"

let xsdPaths = [
    #if INTERACTIVE
    __SOURCE_DIRECTORY__ + "/XmlSchemas/Microsoft.Build.Commontypes.xsd"
    __SOURCE_DIRECTORY__ + "/XmlSchemas/Microsoft.Build.Core.xsd"
    __SOURCE_DIRECTORY__ + "/XmlSchemas/Microsoft.Build.xsd"
    #else
    "Forge.Microsoft.Build.Commontypes.xsd"
    "Forge.Microsoft.Build.Core.xsd"
    "Forge.Microsoft.Build.xsd"
    #endif
]


let buildSchemas =
    let asm = Assembly.GetExecutingAssembly()

    let addxsd (schemas:XmlSchemaSet) (path:string) =
        use reader =
        #if INTERACTIVE
            XmlReader.Create path
        #else
            XmlReader.Create(asm.GetManifestResourceStream path)
        #endif
        schemas.Add (MSBuildNamespace, reader) |> ignore
        schemas
    let schema = List.fold addxsd (XmlSchemaSet()) xsdPaths
    schema.Compile()
    schema




let ignoreFSharpElements (o:Object) (e:ValidationEventArgs) =

    let xelem = o :?> XElement

    match xelem.Name.LocalName with
    | "Paket"
    | "TargetFSharpCoreVersion"
    | "Tailcalls"
    | "FSharpTargetsPath" -> ()
    | _ ->
        let info = xelem :> IXmlLineInfo
        printfn "-- %s --\n"  (string e.Severity)
        printfn "The element '%s' has an invalid child element\n    %s"  xelem.Parent.Name.LocalName xelem.Name.LocalName
        printfn "Located at Line - %i Pos %i\n" info.LineNumber info.LinePosition

//        printfn "%s" e.Message

#if INTERACTIVE

let brokenproj =
    try
        use reader = XmlReader.Create (__SOURCE_DIRECTORY__ + "/broken.fsproj")
        XDocument.Load(reader,LoadOptions.SetLineInfo)
    with
    | e -> failwithf "Couldn't load fproj due to errors \n%s" e.Message
;;
printfn "%A" buildSchemas
;;
brokenproj.Validate(buildSchemas,
    ValidationEventHandler(fun o e -> ignoreFSharpElements o e), true)
#endif

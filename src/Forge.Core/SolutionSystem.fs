module Forge.SolutionSystem

open System
open System.IO
open FParsec



type UserState = unit
type Parser<'t> = Parser<'t, UserState>

/// Sets the platform for a Build Configuration
///     x86,  x64, or AnyCPU.
/// The default is AnyCPU.
type PlatformType =
    | X86 |  X64 | AnyCPU

    override self.ToString () = self |> function
        | X86     -> Constants.X86
        | X64     -> Constants.X64
        | AnyCPU  -> Constants.AnyCPU

    static member Parse text = text |> function
        | InvariantEqual Constants.X86     -> X86
        | InvariantEqual Constants.X64     -> X64
        | InvariantEqual "Any CPU"
        | InvariantEqual Constants.AnyCPU  -> AnyCPU
        | _ ->
            failwithf "Could not parse '%s' into a `PlatformType`" text

    static member TryParse text = text |> function
        | InvariantEqual Constants.X86     -> Some X86
        | InvariantEqual Constants.X64     -> Some X64
        | InvariantEqual "Any CPU"
        | InvariantEqual Constants.AnyCPU  -> Some AnyCPU
        | _ -> None


type PreProjectAttribute    () = inherit Attribute() 
type PostProjectAttribute   () = inherit Attribute() 
type PreSolutionAttribute   () = inherit Attribute() 
type PostSolutionAttribute  () = inherit Attribute() 

type SolutionItem = { Name:string; Path:string }

type SolutionFolder = 
    {   ProjectTypeGuid     : Guid  // {2150E333-8FDC-42A3-9474-1A3956D46DE8}
        Name                : string
        Path                : string
        Guid                : Guid
        [<PreProject>]
        SolutionItems       : SolutionItem list
    }

type Project =
    {   ProjectTypeGuid     : Guid
        Name                : string
        Path                : string
        Guid                : Guid
        [<PostProject>]
        Dependecies         : Guid list
    }

type SolutionConfiguration = { Name:string; Platform:PlatformType }

type BuildProperty = 
    | ActiveCfg | Build0
    static member Parse text = text |> function
        | InvariantEqual "ActiveCfg" -> ActiveCfg
        | InvariantEqual "Build.0" -> Build0
        | _ ->
            failwithf "Could not parse '%s' into a `PlatformType`" text


type ProjectConfiguration =
    {   ProjectGuid   : Guid
        ConfigName    : string
        BuildProperty : BuildProperty
        Platform      : PlatformType
    }

type SolutionProperty = { Name:string; Value:string }

type NestedProject = { Project : Guid; Parent : Guid }

type Solution = 
    {   Header : string
        Folders : SolutionFolder list
        Projects : Project List
        [<PreSolution>]  SolutionConfigurationPlatforms : SolutionConfiguration list
        [<PostSolution>] ProjectConfigurationPlatforms : ProjectConfiguration list
        [<PreSolution>]  SolutionProperties : SolutionProperty list
        [<PreSolution>]  NestedProjects : NestedProject list
    }
    static member Empty = 
        {   Header  = ""
            Folders = []
            Projects = []
            SolutionConfigurationPlatforms = []
            ProjectConfigurationPlatforms = []
            SolutionProperties = []
            NestedProjects = []
        }


[<AutoOpen>]
module internal Parsers =

    let solutionFolderGuid = Guid "2150E333-8FDC-42A3-9474-1A3956D46DE8"

    type UserState = unit
    type Parser<'t> = Parser<'t, UserState>

    let ``{`` : Parser<_> = pchar '{'
    let ``}`` : Parser<_> = pchar '}'
    let ``"`` : Parser<_> = pchar '"'
    let ``(`` : Parser<_> = pchar '('
    let ``)`` : Parser<_> = pchar ')'
    let ``|`` : Parser<_> = skipChar '|'
    let ``.`` : Parser<_> = pchar '.'

    let isGuid c = isHex c || c = '-'

    let pEq     : Parser<_> = pchar '='
    let skipEqs : Parser<_> =  spaces >>. pchar '=' >>. spaces
    let skipCom : Parser<_> =  spaces >>. pchar ',' >>. spaces

    let notspace: Parser<_> = many1Satisfy ^ isNoneOf [ '\t'; ' '; '\n'; '\r'; '\u0085';'\u2028';'\u2029' ]

    let pSection : Parser<_> = pstring "Section"
    let pProject : Parser<_> = pstring "Project"
    let pEndProject : Parser<_> = pstring "EndProject" .>> notFollowedBy pSection
    let pGlobal : Parser<_> = pstring "Global"
    let pEndGlobal : Parser<_> = pstring "EndGlobal" .>> notFollowedBy pSection
    let pProjectSection : Parser<_> = pstring "ProjectSection"
    let pEndProjectSection : Parser<_> = pstring "EndProjectSection" 
    let pGlobalSection : Parser<_> = pstring "GlobalSection"
    let pEndGlobalSection : Parser<_> = pstring "EndGlobalSection"

    let pheader = manyCharsTill anyChar ^ lookAhead pProject

    let pGuid: Parser<Guid> = between ``{`` ``}`` ^ manySatisfy isGuid |>> Guid.Parse

    let quoteGuid = ``"`` >>. pGuid .>> ``"``

    let projGuid: Parser<Guid> = ``(`` >>. quoteGuid .>> ``)``

    let quoted: Parser<_> = between ``"`` ``"`` ^ manyCharsTill anyChar ^ lookAhead ``"``

    let projectHeading =  skipEqs >>. (quoted .>> skipCom) .>>. (quoted .>> skipCom) .>>. quoteGuid .>> spaces


    let internal parseProjectBase itemPsr = 
        let projectSection itempsr = 
            spaces >>. between (pProjectSection .>> skipRestOfLine true) pEndProjectSection
                (manyTill itempsr ^ lookAhead pEndProjectSection)
        let solutionItems: Parser<_> = 
            let customContentParser = projectSection itemPsr
            choice [
                customContentParser
                preturn [] // when we encounter folders without soultion items return an empty list
            ]
        projectHeading .>>. solutionItems .>> spaces


    let pitem = spaces >>. notspace .>> skipEqs .>>. notspace .>> spaces

    let parseFolder = parseProjectBase pitem


    let parseProject =
        let dependency = spaces >>. pGuid .>> skipRestOfLine true .>> spaces
        parseProjectBase dependency


    let projectSections : Parser<_> = 
        let rawProject = 
            spaces >>. between pProject pEndProject 
                (projGuid .>>. many1CharsTill  anyChar ^ lookAhead pEndProject) .>> spaces
        manyTill rawProject ^ lookAhead pGlobal
        |>> fun pls ->
        pls |> List.partition ^ fun (guid,_) -> guid = solutionFolderGuid 
        |> fun (folders, projects) ->

            let runSubParser psr mapfn (xs:(Guid * string) list) =
                xs |> List.map (fun (projGuid,txt) -> 
                match run psr txt with
                | Failure (errormsg, err, _)  -> 
                    failwithf  "Could not parse -\n%s\n\n%s" txt errormsg
                | Success (result, _, _) -> mapfn projGuid result
                )

            let makeFolder projGuid (((name,path),guid),solutionItems) =
                {   SolutionFolder.ProjectTypeGuid = projGuid
                    Name = name
                    Path = path
                    Guid = guid
                    SolutionItems = 
                        solutionItems |> List.map (fun (n,p) -> { Name = n; Path = p}) 
                }

            let makeProject projGuid (((name,path),guid),guids) =
                {   Project.ProjectTypeGuid = projGuid
                    Name = name
                    Path = path
                    Guid = guid
                    Dependecies = guids
                }
            let solutionFolders  = folders  |> runSubParser parseFolder makeFolder
            let solutionProjects = projects |> runSubParser parseProject makeProject
            solutionFolders, solutionProjects


    let pSolutionConfigLine =
        spaces >>. manyCharsTill anyChar ``|`` .>>. (manyCharsTill anyChar  pEq)  .>> skipRestOfLine true
        |>> fun (name, plat) -> 
        {   SolutionConfiguration.Name = name
            Platform = plat.Trim() |> PlatformType.Parse
        }


    let pProjectConfigLine =
        (spaces >>. pGuid .>> ``.``)
        .>>. (many1CharsTill anyChar ``|``)
        .>>. (many1CharsTill anyChar ``.``)
        .>>. (many1CharsTill anyChar ^ (spaces .>> pEq))
        |>> fun (((guid,name),plat),prop) ->
            {   ProjectGuid = guid
                ConfigName = name
                BuildProperty = BuildProperty.Parse ^ prop.Trim()
                Platform = PlatformType.Parse  ^ plat.Trim()
            }


    let pNestedProjectLine : Parser<_> =
        (spaces >>. pGuid .>> skipEqs) .>>. pGuid  .>> skipRestOfLine true
        |>> fun (proj, parent) -> { Project = proj ; Parent = parent }


    let pPropertyLine = pitem |>> fun (n,v) -> { Name = n ; Value = v }


    let spwork =  many (attempt (spaces >>. pPropertyLine       .>> spaces ))
    let scwork =  many (attempt (spaces >>. pSolutionConfigLine .>> spaces ))
    let pcwork =  many (attempt (spaces >>. pProjectConfigLine  .>> spaces ))
    let npwork =  many (attempt (spaces >>. pNestedProjectLine  .>> spaces ))


    let pull_results (target:string) psr (data:(string*string)list) =
        ([],data) ||> List.fold (fun  acc (key,text) ->
            if target = key then
                match run psr text with
                | Failure (errormsg, err, _)  -> acc // failwithf  "Could not parse -\n%s\n\n%s" text errormsg
                | Success (result, _, _) -> List.append acc result            
            else acc
        )


    let parseSolution = 
        let globalsection =
            spaces >>.  between pGlobalSection pEndGlobalSection
                ((``(`` >>. many1CharsTill  anyChar  ``)`` .>> skipRestOfLine true)
                    .>>. many1CharsTill  anyChar ^ lookAhead pEndGlobalSection ) .>> spaces
        let parseGlobal = between pGlobal pEndGlobal  (manyTill globalsection ^ lookAhead pEndGlobal)
        pheader .>>.  projectSections .>>. parseGlobal
        |>> fun ((header,(folders,projects)),data) -> 
            {   Header  = header
                Folders = folders
                Projects = projects
                SolutionConfigurationPlatforms = pull_results "SolutionConfigurationPlatforms" scwork data
                ProjectConfigurationPlatforms = pull_results "ProjectConfigurationPlatforms" pcwork data
                SolutionProperties = pull_results "SolutionProperties" spwork data
                NestedProjects = pull_results "NestedProjects" npwork data
            }
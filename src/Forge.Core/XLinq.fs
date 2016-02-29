[<AutoOpen>]
module Forge.XLinq
open System.Runtime.CompilerServices
open System.Xml.Linq


let xelem (name:string) (content:seq<'a>)  = XElement (XName.Get name, Seq.toArray content)
let xattr (name:string) value    = XAttribute (XName.Get name, value)


let inline private matchName (name:string) (xelem:#XElement) = name = xelem.Name.LocalName

/// Helper function to filter a seq of XElements by matching their local name against the provided string
let private nameFilter name sqs = sqs |> Seq.filter ^ matchName name

let private firstNamed name sqs = sqs |> Seq.find ^ matchName name

let private hasNamed name sqs = sqs |> Seq.exists ^ matchName name


[<RequireQualifiedAccess>]
/// Functions for operating on XNodes
module XNode =

    /// Returns a seq of XElements that precede the XNode 
    let elementsBefore  (node:#XNode) = node.ElementsBeforeSelf()

    /// Returns a seq of XElements that follow the XNode
    let elementsAfter   (node:#XNode) = node.ElementsAfterSelf()
  
    /// Returns a seq of XElements that follow the XNode with a local name matching `name`
    let elementsAfterNamed (node:#XNode) name =
        elementsAfter node |> nameFilter name

    /// Returns a seq of XElements that precede the XNode with a local name matching `name`
    let elementsBeforeNamed (node:#XNode) name =
        elementsBefore node |> nameFilter name

    /// Returns seq of the XNode's ancestor XElements
    let ancestors (node:#XNode) = node.Ancestors()

    /// Returns the first ancestor of the XNode with a local name matching `name`
    let ancestorNamed (name:string) (node:#XNode) =
        ancestors node |> firstNamed name

    /// Returns all ancestors of the XNode with a local name matching `name`
    let ancestorsNamed (name:string) (node:#XNode) =
        ancestors node |> nameFilter name 

    /// Insert a sibling XNode before `node`
    let addBefore (insert:#XNode) (node:#XNode) =
        node.AddBeforeSelf insert
        node
    
    /// Insert a sibling XNode after `node`
    let addAfter (insert:#XNode) (node:#XNode) =
        node.AddAfterSelf insert
        node

        
    let next (node:#XNode) = node.NextNode        
    let previous (node:#XNode) = node.PreviousNode
    let parent (node:#XNode) = node.Parent

    let isBefore (target:#XNode) (node:#XNode) = node.IsBefore target
    let isAfter (target:#XNode)  (node:#XNode) = node.IsAfter target


[<RequireQualifiedAccess>]
/// Functions for operating on XContainers
module XCont =

    let descendants (cont:#XContainer) = cont.Descendants()
    
    let descendantNamed name (cont:#XContainer) =
        descendants cont |> firstNamed name

    let descendantsNamed name (cont:#XContainer) = 
        descendants cont |> nameFilter name
    
    let elements (cont:#XContainer) = cont.Elements()

    let hasElement name (cont:#XContainer) =
        elements cont |> hasNamed name

    let elementNamed name (cont:#XContainer) =
        elements cont |> firstNamed name

    let elementsNamed name (cont:#XContainer) =
        elements cont |> nameFilter name

    let nodes (cont:#XContainer) = cont.Nodes()


[<RequireQualifiedAccess>]
/// Functions for operating on XElements
module XElem =

    let descendants (xelem:#XElement) = xelem.Descendants()
    
    let descendantNamed name (xelem:#XElement) =
        descendants xelem |> firstNamed name

    let descendantsNamed name (xelem:#XElement) = 
        descendants xelem |> nameFilter name
    
    let elements (xelem:#XElement) = xelem.Elements()

    let hasElement name (xelem:#XElement) =
        elements xelem |> hasNamed name

    let elementNamed name (xelem:#XElement) =
        elements xelem |> firstNamed name

    let elementsNamed name (xelem:#XElement) =
        elements xelem |> nameFilter name

    let nodes (xelem:#XElement) = xelem.Nodes()

    let setAttribute name value (xelem:#XElement) =
        xelem.SetAttributeValue(XName.Get name, value)
        xelem
    
    let setElement name value (xelem:#XElement) =
        xelem.SetElementValue(XName.Get name, value)
        xelem

    let addElement (child:XElement) (parent:XElement) =
        parent.Add child
        parent

    
[<Extension>]
type XLinqSeqExtensions =
    [<Extension>] 
    static member Ancestors (source:seq<#XNode>) name = source.Ancestors ^ XName.Get name

    [<Extension>] 
    static member AncestorsAndSelf (source:seq<XElement>)  name =  source.AncestorsAndSelf ^ XName.Get name

    [<Extension>] 
    static member Attributes (source:seq<XElement>)  name =  source.Attributes ^ XName.Get name

    [<Extension>] 
    static member Descendants (source:seq<#XContainer>) name = source.Descendants ^ XName.Get name

    [<Extension>] 
    static member DescendantsAndSelf (source:seq<XElement>) name =  source.DescendantsAndSelf ^ XName.Get name

    [<Extension>] 
    static member Elements (source:seq<#XContainer>) name =  source.Elements ^ XName.Get name





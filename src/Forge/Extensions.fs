[<AutoOpen>]
module Forge.Extensions
open System.Runtime.CompilerServices
open System.Xml.Linq
   
type XNode with
        
    member this.Ancestors ?name : seq<XElement> = 
        match name.IsSome with
        | true  -> this.Ancestors (XName.Get name.Value)  
        | false -> this.Ancestors ()

    member this.ElementsAfterSelf name : seq<XElement> = 
        this.ElementsAfterSelf(XName.Get name)

    member this.ElementsBeforeSelf name : seq<XElement> =
        this.ElementsBeforeSelf(XName.Get name)    

    static member ancestors (xnode:XNode) : seq<XElement> =
        xnode.Ancestors()

    static member elementsBefore (xnode:XNode) : seq<XElement> =
        xnode.ElementsBeforeSelf()



type XContainer with
    member this.Descendants name = 
        this.Descendants(XName.Get name)
    member this.Element  name = 
        this.Element(XName.Get name)
    member this.Elements name = 
        this.Elements(XName.Get name)

    static member nodes (xcon:XContainer) : seq<XNode>  =
        xcon.Nodes()


type XElement with
    member this.AncestorsAndSelf name = 
        this.AncestorsAndSelf(XName.Get name)
    member this.Attribute name = 
        this.Attribute(XName.Get name)
    member this.Attributes name = 
        this.Attributes(XName.Get name)
    member this.DescendantsAndSelf name = 
        this.DescendantsAndSelf(XName.Get name)
    member this.SetAttributeValue (name, value) = 
        this.SetAttributeValue(XName.Get name, value)
    member this.SetElementValue (name, value) = 
        this.SetElementValue(XName.Get name, value)

    



[<Extension>]
type XLinqSeqExtensions =
    [<Extension>] 
    static member  Ancestors (source:seq<#XNode>, name) = 
        source.Ancestors (XName.Get name)

    [<Extension>] 
    static member AncestorsAndSelf (source:seq<XElement>, name) = 
        source.AncestorsAndSelf(XName.Get name)

    [<Extension>] 
    static member Attributes (source:seq<XElement>, name) = 
        source.Attributes (XName.Get name)

    [<Extension>] 
    static member Descendants (source:seq<#XContainer>, name) = 
        source.Descendants (XName.Get name)

    [<Extension>] 
    static member DescendantsAndSelf (source:seq<XElement>, name) = 
        source.DescendantsAndSelf (XName.Get name)

    [<Extension>] 
    static member Elements (source:seq<#XContainer>, name) = 
        source.Elements (XName.Get name)


let xelem name content  = new XElement (XName.Get name, Seq.toArray content)
let xattr name value    = new XAttribute (XName.Get name, value)
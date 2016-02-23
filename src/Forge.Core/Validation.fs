/// This module contains function which validate fsproj file
module Forge.Validation

type MsBuildElement =
    | Choose of ChildElements
    | Import of Attributes
    | ImportGroup of ChildElements * Attributes
    | Item of ChildElements * Attributes
    | ItemDefinitionGroup of ChildElements * Attributes
    | ItemGroup of ChildElements * Attributes
    | ItemMetadata of ChildElements * Attributes
    | OnError of Attributes
    | Otherwise of ChildElements
    | Output of Attributes
    | Parameter of Attributes
    | ParameterGroup of ChildElements
    | Project of ChildElements * Attributes
    | ProjectExtensions
    | Property of Attributes
    | PropertyGroup of ChildElements * Attributes
    | Target of ChildElements * Attributes
    | Task of ChildElements * Attributes
    | TaskBody of ChildElements * Attributes
    | UsingTask of ChildElements * Attributes
    | When of ChildElements * Attributes
    | Unrecognized of ChildElements * Attributes

and ChildElements = MsBuildElement list

and Attribute =
    | Condition of AttributeValue
    | Exclude of AttributeValue
    | ExecuteTargets of AttributeValue
    | Output of AttributeValue
    | Project of AttributeValue
    | Remove of AttributeValue
    | TaskParameter of AttributeValue
    | XmlNs of AttributeValue

and AttributeValue = string
and Attributes = Attribute list

let validate file =
    ()

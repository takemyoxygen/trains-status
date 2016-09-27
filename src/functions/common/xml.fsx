[<AutoOpen>]
module Xml

#r "System.Xml.Linq"
open System.Xml.Linq

let private xn name = XName.Get name

/// Gets single nested element with the given name from the container.
let (-!>) (container: #XContainer) name = container.Element <| xn name

/// Gets all children with the given name from the container.
let (-*>) (container: #XContainer) name = container.Elements <| xn name

/// Gets the value of the XML element.
let xval (element: XElement) = element.Value

/// Parses given XML string
let xparse xml = XDocument.Parse(xml)
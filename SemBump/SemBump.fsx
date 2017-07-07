#r "System.Xml"
#r "System.Xml.Linq"

open System
open System.IO
open System.Linq
open System.Xml.Linq
open System.Collections.Generic

let contents = """<?xml version="1.0" encoding="utf-8"?>
<!-- Do not remove this test for UTF-8: if “Ω” doesn’t appear as greek uppercase omega letter enclosed in quotation marks, you should use an editor that supports UTF-8, not this one. -->
<package xmlns="http://schemas.microsoft.com/packaging/2015/06/nuspec.xsd">
  <metadata>
    <id>choconuspec</id>
    <version>1.3.3.7</version>
  </metadata>
</package>
"""

let doc = XDocument.Parse(contents)
let a = doc.Root.Element(XName.Get "package")
let b = doc.Root.Element(XName.Get "metadata")
XName.Get "package"
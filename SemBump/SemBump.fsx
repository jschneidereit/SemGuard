﻿#r "System.Xml"
#r "System.Xml.Linq"

open System
open System.IO
open System.Linq
open System.Xml.Linq
open System.Collections.Generic

let contents = """<?xml version="1.0" encoding="utf - 8"?>
<!--Do not remove this test for UTF - 8: if “Ω” doesn’t appear as greek uppercase omega letter enclosed in quotation marks, you should use an editor that supports UTF - 8, not this one. -->
      <package xmlns = "http://schemas.microsoft.com/packaging/2015/06/nuspec.xsd">
         <metadata>
           <id> choconuspec </id>
           <version> 1.3.3.7 </version>
              <title> choconuspec(Install) </title>
              <authors> __REPLACE_AUTHORS_OF_SOFTWARE_COMMA_SEPARATED__ </authors>
              <projectUrl> https://_Software_Location_REMOVE_OR_FILL_OUT_</projectUrl>
    <tags> choconuspec admin SPACE_SEPARATED</tags>
       <summary> __REPLACE__ </summary>
       <description> __REPLACE__MarkDown_Okay </description>
     </metadata>
     <files>
       <file src = "tools\**" target = "tools" />
        </files>
      </package>
"""

let doc = XDocument.Parse(contents)
let a = doc.Root.Element(XName.Get "package")
let b = doc.Root.Element(XName.Get "metadata")
XName.Get "package"
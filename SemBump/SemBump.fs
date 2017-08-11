namespace SemBump

module Bumper =
    open System.IO
    open FSharp.Data
    open System
    open SemVer
    open System.Xml.Linq
        
    type UnionOperation = Major | Minor | Patch | Build
    
    let parseOperation (operation : string) : UnionOperation option =
        match operation.ToLower() with
        | "major" -> Some Major
        | "minor" -> Some Minor
        | "patch" -> Some Patch
        | "build" -> Some Build
        | _       -> None

    type Nuget = XmlProvider<"""<?xml version="1.0"?>
<package >
  <metadata>
        <id></id>
        <version></version>
    </metadata>
</package>""">

    type Choco = XmlProvider<"""<?xml version="1.0" encoding="utf-8"?>
<package xmlns="http://schemas.microsoft.com/packaging/2015/06/nuspec.xsd">
    <metadata>
        <id></id>
        <version></version>
    </metadata>
</package>""">

    let tryParseNuget (contents : string) =
        try
            let _ = Nuget.Parse contents
            (true, "success")
        with e -> (false, e.ToString())

    let tryParseChoco (contents : string) = 
        try 
            let _ = Choco.Parse contents
            (true, "success")
        with e -> (false, e.ToString())

    type VersionType = 
        | Sem of SemanticVersion
        | Sys of Version
        | Error of string

    let IsSemVer (s : string) = SemanticVersion.TryParse(s) |> fun (b, _) -> b
    
    let IsSysVer (s : string) = Version.TryParse(s) |> fun (b, _) -> b

    let GetVersionType (v : string) =
        match (IsSysVer v, IsSemVer v) with
        | (true, false) -> Sys (Version v)
        | (_,     true) -> Sem (SemanticVersion v)
        | _ -> Error "Couldn't parse version and therefore can't bump it!"
        
    let BumpSemVer (v : SemanticVersion) (o : UnionOperation) =
        match o with
        | Major -> (v.Major + 1u, 0u, 0u)
        | Minor -> (v.Major, v.Minor + 1u, 0u)
        | Patch -> (v.Major, v.Minor, v.Patch + 1u)
        | _       -> (v.Major, v.Minor, v.Patch)
        |> fun (x, y, z) -> sprintf "%i.%i.%i" x y z

    //Support bumping non-semver for my own selfish reasons
    let BumpSysVer (v : Version) (o : UnionOperation) =
        match o with
        | Major -> (v.Major + 1, 0, 0, 0)
        | Minor -> (v.Major, v.Minor + 1, 0, 0)
        | Patch -> (v.Major, v.Minor, v.Build, 0)
        | Build -> (v.Major, v.Minor, v.Build, v.Revision + 1)
        |> fun (x, y, z, w) -> sprintf "%i.%i.%i.%i" x y z w

    let Bump (v : string) (o : UnionOperation) =
        match GetVersionType v with
        | Sem v -> BumpSemVer v o
        | Sys v -> BumpSysVer v o
        | _   -> v

    let BumpNuspecContents (contents : string) (o : UnionOperation) = 
        let doc = XDocument.Parse(contents)
        
        doc.Descendants().Attributes() |> Seq.filter (fun a -> a.IsNamespaceDeclaration) |> Seq.map (fun a -> a.Remove()) |> ignore
        let versionNode =
            (doc.Descendants())
                |> Seq.filter (fun d -> d.Name.LocalName.ToLower() = "version")
                |> Seq.head

        (Bump (versionNode.Value.Trim()) o).ToString() |> versionNode.SetValue
        
        doc.ToString()
            
    //let BumpNuspecContents (contents : string) (o : string) = 
    //    match (tryParseNuget contents, tryParseChoco contents) with
    //    | ((true, _), _) -> 
    //        let xml = Nuget.Parse(contents)
    //        xml.Metadata.Version.XElement.Value <- (Bump xml.Metadata.Version.XElement.Value o).ToString()
    //        xml.ToString()
    //    | (_, (true, _)) -> 
    //        let xml = Choco.Parse(contents)
    //        xml.Metadata.Version.XElement.Value <- (Bump xml.Metadata.Version.XElement.Value o).ToString()
    //        xml.ToString()
    //    | ((_, n), (_, c)) -> sprintf "Error. Could not parse the nuspec contents. \nNuget error: %s. \nChoco error: %s" n c
    
    let BumpNuspecFile (fi : FileInfo) (operation : string) =
        let op = operation |> parseOperation
        match op with
        | Some o -> ((fi.FullName |> File.ReadAllText |> BumpNuspecContents) o) |> fun x -> File.WriteAllText(fi.FullName, x)
        | None   -> printfn "Operation is not valid"

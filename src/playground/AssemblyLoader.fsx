open System
open System.IO

let script = "load.fsx"
let ignore = ["FSharp.Core"]
let home = __SOURCE_DIRECTORY__
let packages = Directory.GetDirectories(Path.Combine(home, "packages"))

let getAssemblies libFolder =
    let allFrom folder = Directory.EnumerateFiles(folder, "*.dll") |> List.ofSeq
    let firstOf folders supported =
        folders
        |> Seq.map (fun s -> s, supported |> List.tryFindIndex ((=) ((Path.GetFileName s).ToLower())))
        |> Seq.filter (fun (_, i) -> i.IsSome)
        |> Seq.sortBy snd
        |> Seq.map fst
        |> Seq.tryHead

    let supported = ["net45"; "net40"]
    let frameworks =
        Directory.GetDirectories libFolder
        |> List.ofSeq

    match firstOf frameworks supported with
    | Some(f) -> allFrom f
    | None -> []

printfn "Getting assemblies to load"

let assemblies =
    packages
    |> Seq.map (fun p ->
        Path.GetFileName p, Path.Combine(p, "lib"))
    |> Seq.filter (fun (name, path) -> Directory.Exists path)
    |> Seq.map (fun (name, path) -> name, getAssemblies path)
    |> Seq.filter (fun (name, _) -> ignore |> List.contains name |> not)
    |> List.ofSeq

assemblies
|> Seq.iter (fun (name, assemblies) ->
    printfn "%s:" name
    assemblies |> Seq.map ((+) "\t") |> Seq.iter (printfn "%s"))

printfn "Generating script..."

let lines =
    assemblies
    |> Seq.collect snd
    |> Seq.collect (fun path ->
        [ sprintf "#I @\"%s\"" (Path.GetDirectoryName path);
          sprintf "#r @\"%s\"" path;
          String.Empty])
    |> Array.ofSeq

File.WriteAllLines(Path.Combine(home, script), lines)

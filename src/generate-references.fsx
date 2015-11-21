open System
open System.IO

let script = "load.fsx"
let exclude = ["FSharp.Core"]
let home = __SOURCE_DIRECTORY__
let packagesFolder = Path.Combine(home, "packages")
let packages = Directory.GetDirectories(packagesFolder)
let targetFile = Path.Combine(home, script)

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

let shouldRegenerate = 
    (not <| File.Exists targetFile) ||
    (Directory.GetLastWriteTime packagesFolder >= File.GetLastWriteTime targetFile)

if shouldRegenerate then

    printfn "Getting assemblies to load"

    let assemblies =
        packages
        |> Seq.map (fun p -> Path.GetFileName p, Path.Combine(p, "lib"))
        |> Seq.filter (snd >> Directory.Exists)
        |> Seq.map (fun (name, path) -> name, getAssemblies path)
        |> Seq.filter (fun (name, _) -> exclude |> List.contains name |> not)
        |> List.ofSeq

    printfn "Generating script..."

    let lines =
        assemblies
        |> Seq.collect snd
        |> Seq.collect (fun path ->
            [ sprintf "#I @\"%s\"" (Path.GetDirectoryName path);
              sprintf "#r @\"%s\"" path;
              String.Empty])
        |> Array.ofSeq


    File.WriteAllLines(targetFile, lines)

else
    printfn "Load script generation skipped, nothing has changed since last generation"

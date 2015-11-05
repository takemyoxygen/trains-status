#r "packages/FAKE/tools/FakeLib.dll"

open System
open System.IO
open Fake
open Fake.FileUtils
open System.Diagnostics
open System.Threading

let normalize = toLower >> trimEndChars [|'\\'|]
let sourceDir = normalize __SOURCE_DIRECTORY__
let outputDir = 
    (match environVarOrNone "DEPLOYMENT_TARGET" with
    | Some(dir) -> dir
    | None -> getBuildParamOrDefault "output-dir" __SOURCE_DIRECTORY__)
    |> normalize

logfn "Source directory: %s" sourceDir
logfn "Output directory: %s" outputDir

[<Literal>] 
let Azure = "azure"

[<Literal>] 
let Local = "local"

let environment = getBuildParamOrDefault "env" Local |> toLower

Target "Start" ignore
Target "Clean" (fun _ -> CleanDir outputDir)

Target "PatchConfig" (fun _ ->
    printfn "Patching web.config"
    let config = sourceDir @@ "web.azure.config"
    if (TestFile config) then
        let target = sourceDir @@ "web.config"
        if TestFile target then DeleteFile target
        Rename target config
)

let exec filename args = 
    let info = new ProcessStartInfo(
                FileName = filename,
                WorkingDirectory = sourceDir,
                Arguments = args)

    let proc = Process.Start info
    
    if not <| proc.WaitForExit(1000 * 60 * 5) then
        failwithf "Process \"%s %s\" didn't exit after 5 minutes" filename args

let execHere filename args =
    let absoluteFilename = 
        if Path.GetFileName filename = filename then 
            match tryFindFileOnPath filename with
            | Some(f) -> f
            | None -> failwithf "Failed to find file \"%s\"" filename
        else filename

    let info = new ProcessStartInfo(
                FileName = absoluteFilename,
                WorkingDirectory = sourceDir,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true)

    let proc = Process.Start info

    let rec read() = async {
        if not proc.HasExited then
            let! line = proc.StandardOutput.ReadLineAsync() |> Async.AwaitTask
            printfn "%s" line
            do! read()
    }

    read() |> Async.Start

    proc.WaitForExit()


/// Starts a process that won't be terminated when FAKE build completes
let startDetached filename args = 
    let info = new ProcessStartInfo(
                    FileName = filename,
                    WorkingDirectory = sourceDir,
                    Arguments = args)
    Process.Start info |> ignore

Target "RestoreNodePackages" (fun _ -> 
    printfn "Restoring NPM packages"
    execHere "npm.cmd" "install --production")

let nodeBin = sourceDir @@ "node_modules\\.bin"
let bower = nodeBin @@ "bower.cmd"
let babel = nodeBin @@ "babel.cmd"
let autoless = nodeBin @@ "autoless.cmd"

Target "RestoreBowerPackages" (fun _ ->
    printfn "Restoring Bower packages"
    execHere bower "install --production"
)

Target "CompileJs" (fun _ ->
    printfn "Compiling ES6 JavaScript files"
    execHere babel "js/src --out-dir js/build --modules amd --stage 0"
)

Target "CompileLess" (fun _ ->
    printfn "Compiling LESS files"
    execHere autoless "--no-watch styles styles"
)

Target "Copy" (fun _ ->
    let files = 
        !! "packages/**/*.*"
        ++ "bower_components/**/*.*"
        ++ "*.fsx"
        ++ "fs/*.fs"
        ++ "js/build/**/*.js"
        ++ "styles/*.css"
        ++ "samples/*.xml"
        ++ "credentials.txt"
        ++ "index.html"
        ++ "web.config"
        |> SetBaseDir sourceDir
    CopyWithSubfoldersTo outputDir [files]
)

Target "Build" ignore

Target "Watch" (fun _ ->
    if TestDir nodeBin then
        startDetached babel "js/src --watch --out-dir js/build --modules amd --stage 0"
        startDetached autoless "styles styles"
    else failwith "\"Watch\" can only be executed from the folder with the source code"
)

Target "Run" (fun _ ->
    let username, password, connectionString = 
        if environment = Azure then
            environVar "APPSETTING_NS_USERNAME",
            environVar "APPSETTING_NS_PASSWORD",
            environVar "APPSETTING_CONNECTION_STRING"
        else
            let content = File.ReadAllLines(sourceDir @@ "credentials.txt")
            content.[0], content.[1], content.[2]

    let app = sourceDir @@ "app.fsx"

    let arguments = sprintf "%s username=%s password=%s connection-string=%s" app username password connectionString
    execHere fsiPath arguments
//    execProcess
//        (fun info -> 
//            info.FileName <- fsiPath
//            info.WorkingDirectory <- sourceDir
//            info.Arguments <- arguments)
//        (Timeout.InfiniteTimeSpan)
//    |> ignore
)

"Start"
    =?> ("Clean", sourceDir <> outputDir)
    =?> ("PatchConfig", environment = Azure)
    ==> "RestoreNodePackages"
    ==> "RestoreBowerPackages"
    ==> "CompileJs"
    ==> "CompileLess"
    =?> ("Copy", sourceDir <> outputDir)
    ==> "Build"

RunTargetOrDefault "Run"
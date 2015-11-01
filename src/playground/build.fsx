#r "packages/FAKE/tools/FakeLib.dll"

open System
open Fake
open Fake.FileUtils
open System.Diagnostics

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
        mv config (sourceDir @@ "web.config")
)

let execAndWait filename args = 
    let successful = 
        execProcess 
            (fun info ->
                info.WorkingDirectory <- sourceDir
                info.FileName <- filename
                info.Arguments <- args
                info.UseShellExecute <- true)
            (TimeSpan.FromMinutes 1.0)
    
    if not successful then
        failwithf "Failed to execute \"%s %s\"" filename args

/// Starts a process that won't be terminated when FAKE build completes
let startDetachedProcess filename args = 
    let info = new ProcessStartInfo(
                    FileName = filename,
                    WorkingDirectory = sourceDir,
                    Arguments = args)
    Process.Start info |> ignore

Target "RestoreNodePackages" (fun _ -> 
    printfn "Restoring NPM packages"
    execAndWait "npm" "install --production")

let nodeBin = sourceDir @@ "node_modules\\.bin"
let bower = nodeBin @@ "bower.cmd"
let babel = nodeBin @@ "babel.cmd"
let autoless = nodeBin @@ "autoless.cmd"

Target "RestoreBowerPackages" (fun _ ->
    printfn "Restoring Bower packages"
    execAndWait bower "install --production"
)

Target "CompileJs" (fun _ ->
    printfn "Compiling ES6 JavaScript files"
    execAndWait babel "js/src --out-dir js/build --modules amd --stage 0"
)

Target "CompileLess" (fun _ ->
    printfn "Compiling LESS files"
    execAndWait autoless "--no-watch styles styles"
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
        |> SetBaseDir sourceDir
    CopyWithSubfoldersTo outputDir [files]
)

Target "Build" ignore

Target "Watch" (fun _ ->
    if TestDir nodeBin then
        startDetachedProcess babel "js/src --watch --out-dir js/build --modules amd --stage 0"
        startDetachedProcess autoless "styles styles"
    else failwith "\"Watch\" can only be executed from the folder with the source code"
)

#load "app.fsx"

Target "Run" (fun _ ->
    printfn "Starting Suave server..."
    App.start() |> ignore

    printfn "Server has been started. Press <Enter> to stop the server."
    Console.ReadLine() |> ignore

    printfn "Stopping the server..."
    App.stop()
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
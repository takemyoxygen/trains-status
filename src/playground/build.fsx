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

let azure = "azure"
let environment = getBuildParamOrDefault "env" "local" |> toLower

Target "Start" ignore
Target "Clean" (fun _ -> CleanDir outputDir)

Target "PatchConfig" (fun _ ->
    printfn "Patching web.config"
    let config = sourceDir @@ "web.azure.config"
    if (TestFile config) then
        mv config (sourceDir @@ "web.config")
)

let execAndWait filename args = 
    let info = new ProcessStartInfo(
                    WorkingDirectory = sourceDir,
                    FileName = filename,
                    Arguments = args)
    let p =  Process.Start(info)
    let successful = p.WaitForExit(60000)
    
    if not successful then
        failwithf "Failed to execute \"%s %s\"" filename args


Target "RestoreNodePackages" (fun _ -> 
    printfn "Restoring NPM packages"
    execAndWait "npm" "install --production")

let nodeBin = sourceDir @@ "node_modules\\.bin"

Target "RestoreBowerPackages" (fun _ ->
    printfn "Restoring Bower packages"
    let bower = nodeBin @@ "bower.cmd"
    execAndWait bower "install --production"
)

Target "CompileJs" (fun _ ->
    printfn "Compiling ES6 JavaScript files"
    let babel = nodeBin @@ "babel.cmd"
    execAndWait babel "js/src --out-dir js/build --modules amd --stage 0"
)

Target "CompileLess" (fun _ ->
    printfn "Compiling LESS files"
    let autoless = nodeBin @@ "autoless.cmd"
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

Target "Default" ignore

Target "Run" (fun _ ->
    let loglevel = getBuildParamOrDefault "loglevel" "verbose"
    let info = new ProcessStartInfo(
                    Arguments = "app.fsx loglevel=" + loglevel,
                    FileName = "fsi",
                    WorkingDirectory = sourceDir)
    Process.Start info |> ignore
)

"Start"
    =?> ("Clean", sourceDir <> outputDir)
    =?> ("PatchConfig", environment = azure)
    ==> "RestoreNodePackages"
    ==> "RestoreBowerPackages"
    ==> "CompileJs"
    ==> "CompileLess"
    =?> ("Copy", sourceDir <> outputDir)
    ==> "Default"

RunTargetOrDefault "Default"
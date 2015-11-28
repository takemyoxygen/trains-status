#r "packages/FAKE/tools/FakeLib.dll"
#r "System.Management"

open System
open System.IO
open Fake
open Fake.FileUtils
open System.Diagnostics
open System.Threading
open System.Management

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

let startProcess filename args embed =
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
                UseShellExecute = not embed,
                RedirectStandardOutput = embed,
                RedirectStandardError = embed)

    let proc = new Process(StartInfo = info)
    start proc

    if embed then
        let rec read() = async {
            if not proc.HasExited then
                let! line = proc.StandardOutput.ReadLineAsync() |> Async.AwaitTask
                printfn "%s" line
                do! read()
        }

        let rec readError() = async {
            if not proc.HasExited then
                let! line = proc.StandardError.ReadLineAsync() |> Async.AwaitTask
                Console.Error.WriteLine line
                do! readError()
        }

        read() |> Async.Start
        readError() |> Async.Start

    proc

let waitForExit (p: Process) = p.WaitForExit()

let exec filename args =
    startProcess filename args true
    |> waitForExit


/// Starts a process that won't be terminated when FAKE build completes
let startDetached filename args =
    let info = new ProcessStartInfo(
                    FileName = filename,
                    WorkingDirectory = sourceDir,
                    Arguments = args)
    let proc = Process.Start info

    startedProcesses.Add(proc.Id, proc.StartTime) |> ignore

    printfn "Process %i has been started" proc.Id


Target "RestoreNodePackages" (fun _ ->
    printfn "Restoring NPM packages"
    exec "npm.cmd" "install --production")

let nodeBin = sourceDir @@ "node_modules\\.bin"
let bower = nodeBin @@ "bower.cmd"
let babel = nodeBin @@ "babel.cmd"
let autoless = nodeBin @@ "autoless.cmd"

Target "RestoreBowerPackages" (fun _ ->
    printfn "Restoring Bower packages"
    exec bower "install --production"
)

Target "CompileJs" (fun _ ->
    printfn "Compiling ES6 JavaScript files"
    exec babel "js/src --out-dir js/build --modules amd --stage 0"
)

Target "CompileLess" (fun _ ->
    printfn "Compiling LESS files"
    exec autoless "--no-watch styles styles"
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
        ++ "favicon.ico"
        ++ "img/*.*"
        |> SetBaseDir sourceDir
    CopyWithSubfoldersTo outputDir [files]
)

Target "Build" ignore

let findNodeJsProcesses (folder: string) =
    let folderLowercase = folder.ToLower()

    let wmiQueryString = "SELECT ProcessId, ExecutablePath, CommandLine FROM Win32_Process";
    use searcher = new ManagementObjectSearcher(wmiQueryString)
    let pids = 
        searcher.Get()
        |> Seq.cast<ManagementObject>
        |> Seq.filter (fun wmi -> 
            Path.GetFileName(string wmi.["ExecutablePath"]).ToLower() = "node.exe")
        |> Seq.filter (fun wmi ->
            (string wmi.["CommandLine"]).ToLower().Contains folderLowercase)
        |> Seq.map (fun wmi -> wmi.["ProcessId"] :?> uint32 |> int)
        |> List.ofSeq

    if pids.IsEmpty then []
    else
        Process.GetProcesses()
        |> Seq.filter (fun p -> pids |> List.contains p.Id)
        |> List.ofSeq

Target "Watch" (fun _ ->
    if TestDir nodeBin then
        startProcess babel "js/src --watch --out-dir js/build --modules amd --stage 0" false |> ignore
        startProcess autoless "styles styles" false |> ignore
        ActivateFinalTarget "TerminateWatchers"
    else failwith "\"Watch\" can only be executed from the folder with the source code")

FinalTarget "TerminateWatchers" (fun _ -> 
    findNodeJsProcesses sourceDir
    |> Seq.iter (fun p -> 
        logfn "Trying to kill process %i" p.Id
        p.Kill()))

let startServer username password connectionString port =
    let script = sourceDir @@ "app.fsx"
    let arguments = sprintf "%s username=%s password=%s connection-string=%s port=%s" script username password connectionString port
    startProcess fsiPath arguments true

Target "RunOnAzure" (fun _ ->
    let username, password, connectionString =
        environVar "APPSETTING_NS_USERNAME",
        environVar "APPSETTING_NS_PASSWORD",
        environVar "APPSETTING_CONNECTION_STRING"
    let port = getBuildParam "port"

    if isNullOrEmpty port then failwith "No port configured"
    else startServer username password connectionString port |> waitForExit
)

Target "RunLocally" (fun _ ->
    let username, password, connectionString =
        let content = File.ReadAllLines(sourceDir @@ "credentials.txt")
        content.[0], content.[1], content.[2]

    let port = getBuildParamOrDefault "port" "8081"
    
    let rec loop() = 
        printfn "Starting the server, type \"r\" to restart or anything else to stop."
        let proc = startServer username password connectionString port
        let input = Console.ReadKey(true)
        proc.Kill()
        if input.KeyChar = 'r' then loop()

    loop()
)

Target "Run" DoNothing

"Start"
    =?> ("Clean", sourceDir <> outputDir)
    =?> ("PatchConfig", environment = Azure)
    ==> "RestoreNodePackages"
    ==> "RestoreBowerPackages"
    ==> "CompileJs"
    ==> "CompileLess"
    =?> ("Copy", sourceDir <> outputDir)
    ==> "Build"

"Start"
    =?> ("RunOnAzure", environment = Azure)
    =?> ("Watch", environment = Local)
    =?> ("RunLocally", environment = Local)
    ==> "Run"

RunTargetOrDefault "Run"

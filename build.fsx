#r "packages/FAKE/tools/FakeLib.dll"
#r "System.Management"

open System
open System.IO
open Fake
open Fake.FileUtils
open System.Diagnostics
open System.Threading
open System.Management

type Environment =
    | Local
    | Azure

let normalize = toLower >> trimEndChars [|'\\'|]
let homeDir = normalize __SOURCE_DIRECTORY__
let outputDir =
    (match environVarOrNone "DEPLOYMENT_TARGET" with
    | Some(dir) -> dir
    | None -> getBuildParamOrDefault "output-dir" __SOURCE_DIRECTORY__)
    |> normalize

logfn "Source directory: %s" homeDir
logfn "Output directory: %s" outputDir

let environment = 
    match getBuildParam "env" |> toLower with
    | "" | "local" -> Local
    | "azure" -> Azure
    | x -> failwithf "Environment \"%s\" is not supported." x

/// Starts external process.
let startProcess filename args embed workingFolder =
    let absoluteFilename =
        if Path.GetFileName filename = filename then
            match tryFindFileOnPath filename with
            | Some(f) -> f
            | None -> failwithf "Failed to find file \"%s\"" filename
        else filename

    logfn "Starting process \"%s\" with args \"%s\"" absoluteFilename args

    let info = new ProcessStartInfo(
                FileName = absoluteFilename,
                WorkingDirectory = workingFolder,
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
                logfn "%s" line
                do! read()
        }

        let rec readError() = async {
            if not proc.HasExited then
                let! line = proc.StandardError.ReadLineAsync() |> Async.AwaitTask
                traceError line
                do! readError()
        }

        read() |> Async.Start
        readError() |> Async.Start

    proc

let waitForExit (p: Process) = p.WaitForExit()

let exec filename args =
    startProcess filename args true homeDir
    |> waitForExit

let execIn folder filename args =
    startProcess filename args true folder
    |> waitForExit

/// Starts a process that won't be terminated when FAKE build completes
let startDetached filename args =
    let info = new ProcessStartInfo(
                    FileName = filename,
                    WorkingDirectory = homeDir,
                    Arguments = args)
    let proc = Process.Start info

    startedProcesses.Add(proc.Id, proc.StartTime) |> ignore

    logfn "Process %i has been started" proc.Id

let nodePath path = 
    if isMono then path else path + ".cmd"

let npm = nodePath "npm"
let nodeBin = homeDir @@ "node_modules" @@ ".bin"
let bower = nodeBin @@ "bower" |> nodePath
let babel = nodeBin @@ "babel" |> nodePath
let autoless = nodeBin @@ "autoless" |> nodePath

Target "Start" ignore

Target "Clean" (fun _ -> CleanDir outputDir)

Target "PatchConfig" (fun _ ->
    logfn "Patching web.config"
    let config = homeDir @@ "src" @@ "web.azure.config"
    if (TestFile config) then
        let target = homeDir @@ "web.config"
        if TestFile target then DeleteFile target
        CopyFile target config
)

Target "RestoreNodePackages" (fun _ ->
    logfn "Restoring NPM packages"
    exec npm "install --production")

Target "RestoreBowerPackages" (fun _ ->
    logfn "Restoring Bower packages"
    execIn (homeDir @@ "src") bower "install --production"
)

Target "CompileFs" (fun _ ->
    ["Trains Status.sln"] |> MSBuildRelease "" "Rebuild"
    |> Log "MsBuild output: "
)

Target "CompileJs" (fun _ ->
    logfn "Compiling ES6 JavaScript files"
    exec babel "src/js/src --out-dir src/js/build --modules amd --stage 0"
)

Target "CompileLess" (fun _ ->
    logfn "Compiling LESS files"
    exec autoless "--no-watch src/styles src/styles"
)

Target "GenerateIncludeScript" (fun _ ->
    let args = "generate-include-scripts framework net45 type fsx"
    let paket = ".paket" @@ "paket.exe"
    let executable, args = 
        if isMono then monoPath, paket + " " + args
        else paket, args

    exec executable args
)

Target "Copy" (fun _ ->
    let files =
        !! "packages/**/*.*"
        ++ "paket-files/**/*.*"
        ++ "src/bower_components/**/*.*"
        ++ "src/*.fsx"
        ++ "src/fs/*.fs"
        ++ "src/js/build/**/*.js"
        ++ "src/styles/*.css"
        ++ "src/samples/*.xml"
        ++ "src/Index.html"
        ++ "src/favicon.ico"
        ++ "src/img/*.*"
        ++ "build.fsx"
        ++ "web.config"
        |> SetBaseDir homeDir
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
        startProcess babel "src/js/src --watch --out-dir src/js/build --modules amd --stage 0" false homeDir|> ignore
        startProcess autoless "src/styles src/styles" false homeDir |> ignore
        ActivateFinalTarget "TerminateWatchers"
    else failwith "\"Watch\" can only be executed from the folder with the source code")

FinalTarget "TerminateWatchers" (fun _ -> 
    findNodeJsProcesses homeDir
    |> Seq.iter (fun p -> 
        logfn "Trying to kill process %i" p.Id
        p.Kill()))

let startServer username password connectionString port =
    let script = homeDir @@ "src" @@ "app.fsx"
    let arguments = sprintf "%s username=%s password=%s connection-string=%s port=%s" script username password connectionString port
    startProcess fsiPath arguments true homeDir

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
        let content = File.ReadAllLines(homeDir @@ "credentials.txt")
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
    =?> ("Clean", homeDir <> outputDir)
    =?> ("PatchConfig", environment = Azure)
    ==> "RestoreNodePackages"
    ==> "RestoreBowerPackages"
    ==> "CompileFs"
    ==> "CompileJs"
    ==> "CompileLess"
    =?> ("Copy", homeDir <> outputDir)
    ==> "Build"

"Start"
    ==> "RestoreNodePackages"
    ==> "RestoreBowerPackages"
    ==> "GenerateIncludeScript"
    ==> "CompileFs"
    =?> ("Watch", not isMono)
    =?> ("CompileJs", isMono)
    =?> ("CompileLess", isMono)
    ==> "RunLocally"

"Start"
    =?> ("RunOnAzure", environment = Azure)
    =?> ("RunLocally", environment = Local)
    ==> "Run"

RunTargetOrDefault "Run"

#r "System.Management"

open System.Management
open System.Linq
open System.Diagnostics
open System.IO

let processes = Process.GetProcesses()

let findNodeJsProcesses (folder: string) =
    let folderLowercase = folder.ToLower()

    let wmiQueryString = "SELECT ProcessId, ExecutablePath, CommandLine FROM Win32_Process";
    use searcher = new ManagementObjectSearcher(wmiQueryString)

    let pids = 
        searcher.Get().Cast<ManagementObject>()
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



findNodeJsProcesses __SOURCE_DIRECTORY__

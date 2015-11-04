#r "System.Management"

open System.Management
open System.Linq
open System.Diagnostics

let processes = Process.GetProcesses()

let nodes = processes
            |> Seq.filter (fun p -> p.Id = 7756 || p.Id = 6088)
            |> List.ofSeq


let wmiQueryString = "SELECT ProcessId, ExecutablePath, CommandLine FROM Win32_Process";
let searcher = new ManagementObjectSearcher(wmiQueryString)
let results = searcher.Get().Cast<ManagementObject>()
              |> Seq.filter(fun o ->
                    let pid = int (o.["ProcessId"] :?> System.UInt32)
                    pid = 7756 || pid = 6088)
              |> List.ofSeq


results.[0].Properties


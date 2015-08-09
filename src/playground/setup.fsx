open System
open System.IO

Environment.CurrentDirectory <- __SOURCE_DIRECTORY__

if not (File.Exists "paket.exe") then
    let url = "https://github.com/fsprojects/Paket/releases/download/1.18.5/paket.exe"
    use wc = new Net.WebClient()
    let tmp = Path.GetTempFileName()
    printfn "Downloading Paket"
    wc.DownloadFile(url, tmp); 
    File.Move(tmp,Path.GetFileName url)
    printfn "Paket is ready"

#r "paket.exe"

Paket.Dependencies.Install """
    source https://nuget.org/api/v2
    nuget FSharp.Data 
    nuget FSharp.Compiler.Tools
    nuget Suave
    nuget Json.Net
""";;
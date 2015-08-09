#r "packages/Suave/lib/net40/Suave.dll"

#load "fs/Status.fsx"

open System
open Suave
open Suave.Http.Successful
open Suave.Web
open Suave.Types
open System.Net
open Suave.Http
open Suave.Http.Applicatives
open Suave.Http.RequestErrors
open Suave.Http.Files

Environment.CurrentDirectory <- __SOURCE_DIRECTORY__

let configuredPort = Environment.GetCommandLineArgs()
                     |> Seq.tryFind(fun s -> s.StartsWith("port="))
                     |> Option.map(fun s -> s.Replace("port=", String.Empty))
                     |> Option.map(Sockets.Port.Parse)

let port = defaultArg configuredPort (uint16 8081)
printfn "Starting Suave server on port %i" port

let serverConfig = 
    { defaultConfig with
       bindings = [ HttpBinding.mk HTTP IPAddress.Loopback port ]
    }

let app = 
    choose
        [GET >>= pathScan "/api/%s/%s" (fun (origin, destination) -> 
            let origin, destination = Uri.UnescapeDataString origin, Uri.UnescapeDataString destination
            match Status.check origin destination with
            | Status.Status.Ok-> OK "OK"
            | Status.Status.Delayed -> OK "Delayed"
            | Status.Status.NotFound -> OK <| sprintf "No travel options found between %s and %s" origin destination)
         GET >>= path "/" >>= file "Index.html"
         OK "Nothing here"]

startWebServer serverConfig app
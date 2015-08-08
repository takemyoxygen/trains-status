#r "packages/Suave/lib/net40/Suave.dll"

open System
open Suave
open Suave.Http.Successful
open Suave.Web
open Suave.Types
open System.Net

let configuredPort = Environment.GetCommandLineArgs()
                     |> Seq.tryFind(fun s -> s.StartsWith("port="))
                     |> Option.map(fun s -> s.Replace("port=", String.Empty))
                     |> Option.map(Sockets.Port.Parse)

let port = defaultArg configuredPort (uint16 8080)
printfn "Starting Suave server on port %i" port

let serverConfig = 
    { defaultConfig with
       bindings = [ HttpBinding.mk HTTP IPAddress.Loopback port ]
    }

startWebServer serverConfig (OK "Hello World!")
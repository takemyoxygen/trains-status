#r "packages/Suave/lib/net40/Suave.dll"
#r "packages/FSharp.Data/lib/net40/FSharp.Data.dll"

#load "fs/Common.fs"
#load "fs/Config.fs"
#load "fs/Http.fs"
#load "fs/TravelOptions.fs"
#load "fs/Status.fs"
#load "fs/Controller.fs"

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

let config = Config.current

printfn "Starting Suave server on port %i" config.Port

let serverConfig = 
    { defaultConfig with bindings = [ HttpBinding.mk HTTP IPAddress.Loopback config.Port ] }

let app = 
    choose
        [GET >>= pathScan "/api/%s/%s" (fun (origin, destination) -> 
            OK <| Controller.checkStatus 
                        config.Credentials 
                        (Uri.UnescapeDataString origin)
                        (Uri.UnescapeDataString destination))
         GET >>= path "/" >>= file "Index.html"
         OK "Nothing here"]

startWebServer serverConfig app
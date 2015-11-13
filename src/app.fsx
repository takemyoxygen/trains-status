#load "AssemblyLoader.fsx"
#load "load.fsx"

#r "System.Xml.Linq"

#load "fs/Json.fs"
#load "fs/Common.fs"
#load "fs/Config.fs"
#load "fs/Http.fs"
#load "fs/TravelOptions.fs"
#load "fs/Status.fs"
#load "fs/Storage.fs"
#load "fs/Stations.fs"
#load "fs/Auth.fs"
#load "fs/Controller.fs"

open System
open Suave
open Suave.Web
open Suave.Types
open System.Net
open Suave.Http
open Suave.Http.Writers

open Common
open System.Threading

let private cancellationTokenSource = new CancellationTokenSource()

let private setup() =

    let home = __SOURCE_DIRECTORY__

    printfn "Current folder: %s" home
    Environment.CurrentDirectory <- home
    let config = Config.current home

    printfn "Starting Suave server on port %i with log level %O" config.Port config.LogLevel

    let mimeTypes =
      defaultMimeTypesMap
        >=> (function
                | ".jsx" -> mkMimeType "text/jsx" true
                | ".woff" -> mkMimeType "application/x-font-woff" false
                | ".woff2" -> mkMimeType "application/font-woff2" false
                | ".ttf" -> mkMimeType " application/font-sfnt" false
                | _ -> None)

    let noCache =
      setHeader "Cache-Control" "no-cache, no-store, must-revalidate"
      >>= setHeader "Pragma" "no-cache"
      >>= setHeader "Expires" "0"

    let serverConfig =
        { defaultConfig with
            logger = Logging.Loggers.saneDefaultsFor config.LogLevel
            bindings = [ HttpBinding.mk HTTP IPAddress.Loopback config.Port ]
            mimeTypesMap = mimeTypes
            cancellationToken = cancellationTokenSource.Token
            homeFolder = Some home }

    serverConfig, Controller.webParts config >>= noCache

let start () =
    let config, app = setup()
    printfn "Starting Suave server..."
    startWebServer config app

start()

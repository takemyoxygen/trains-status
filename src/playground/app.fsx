#r "packages/Suave/lib/net40/Suave.dll"
#r "packages/FSharp.Data/lib/net40/FSharp.Data.dll"
#r "packages/Newtonsoft.Json/lib/net40/Newtonsoft.Json.dll"

#load "fs/Common.fs"
#load "fs/Config.fs"
#load "fs/Http.fs"
#load "fs/TravelOptions.fs"
#load "fs/Status.fs"
#load "fs/Stations.fs"
#load "fs/Controller.fs"
#load "fs/Json.fs"

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
open Suave.Http.Writers
open Newtonsoft.Json
open Newtonsoft.Json.Serialization

open Common

Environment.CurrentDirectory <- __SOURCE_DIRECTORY__

let config = Config.current

printfn "Starting Suave server on port %i" config.Port

let mimeTypes =
  defaultMimeTypesMap
    >=> (function | ".jsx" -> mkMimeType "text/jsx" true | _ -> None)

let serverConfig =
    { defaultConfig with
        logger = Logging.Loggers.saneDefaultsFor Logging.LogLevel.Verbose
        bindings = [ HttpBinding.mk HTTP IPAddress.Loopback config.Port ]
        mimeTypesMap = mimeTypes }

let json response =
    Json.toJsonString response
    |> OK
    >>= setMimeType "application/json"

let staticContent  =
    [".js"; ".jsx"; ".css"; ".html"]
    |> Seq.map (fun s -> s.Replace(".", "\."))
    |> String.concat "|"
    |> sprintf "(%s)$"

printfn "Static content: \"%s\"" staticContent

let app =
    choose
        [GET >>= pathScan "/api/status/%s/%s" (fun (origin, destination) ->
            json <| Controller.checkStatus
                config.Credentials
                (Uri.UnescapeDataString origin)
                (Uri.UnescapeDataString destination))
         GET >>= path "/api/stations" >>= (json <| Controller.getAllStations config.Credentials)
         GET >>= path "/api/stations/closest" >>= (fun context -> async {
                let stations = opt {
                    let! lat = context.request.["lat"] |> Option.tryMap Double.TryParse
                    let! lon = context.request.["lon"] |> Option.tryMap Double.TryParse
                    let! count = context.request.["count"] |> Option.tryMap Int32.TryParse
                    printfn "Looking for %i stations near %f, %f" count lat lon
                    return Controller.getClosest config.Credentials lat lon count
                }
                match stations with
                | Some(s) -> return! json s context
                | None -> return None
            })
         GET >>= path "/" >>= file "Index.html"
         GET >>= pathRegex staticContent >>= browse __SOURCE_DIRECTORY__
         OK "Nothing here"]

startWebServer serverConfig app

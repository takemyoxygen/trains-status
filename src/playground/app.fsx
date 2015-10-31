#load "AssemblyLoader.fsx"
#load "load.fsx"

#r "System.Xml.Linq"

#load "fs/Json.fs"
#load "fs/Common.fs"
#load "fs/Config.fs"
#load "fs/Http.fs"
#load "fs/TravelOptions.fs"
#load "fs/Status.fs"
#load "fs/Stations.fs"
#load "fs/Storage.fs"
#load "fs/Controller.fs"
#load "fs/Auth.fs"

open System
open Suave
open Suave.Http.Successful
open Suave.Web
open Suave.Types
open System.Net
open Suave.Http
open Suave.Http.Applicatives
open Suave.Http.RequestErrors
open Suave.Http.ServerErrors
open Suave.Http.Files
open Suave.Http.Writers
open Newtonsoft.Json
open Newtonsoft.Json.Serialization

open Common

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
        mimeTypesMap = mimeTypes }

let staticContent  =
    [".js"; ".jsx"; ".css"; ".html"; ".woff"; ".woff2"; ".ttf"]
    |> Seq.map (fun s -> s.Replace(".", "\."))
    |> String.concat "|"
    |> sprintf "(%s)$"

printfn "Static content: \"%s\"" staticContent

let app =
    choose
        [GET >>= pathScan "/api/status/%s/%s" (fun (origin, destination) ->
            Controller.checkStatus config.Credentials origin destination)
         GET >>= path "/api/stations/all" >>= request (fun _ -> Controller.allStations config.Credentials |> Json.asResponse)
         GET >>= path "/api/stations/nearby" >>= (fun context -> async {
                let stations = opt {
                    let! lat = context.request.["lat"] |> Option.tryMap Double.TryParse
                    let! lon = context.request.["lon"] |> Option.tryMap Double.TryParse
                    let! count = context.request.["count"] |> Option.tryMap Int32.TryParse
                    printfn "Looking for %i stations near %f, %f" count lat lon
                    return Controller.getClosest config.Credentials lat lon count
                }
                match stations with
                | Some(s) -> return! Json.asResponse s context
                | None -> return None
            })
         GET >>= pathScan "/api/user/%s/favourite" (Controller.favouriteStations config >> Json.asResponse)
         PUT >>= pathScan "/api/user/%s/favourite" (fun id -> request (fun req ->
            match Controller.saveFavourites config id req.rawForm with
            | Ok -> OK "Saved"
            | Error -> INTERNAL_ERROR "Failed"))
         GET >>= path "/api/user/info" >>= Auth.getUserInfo
         GET >>= path "/" >>= file "Index.html"
         GET >>= pathRegex staticContent >>= browse __SOURCE_DIRECTORY__
         NOT_FOUND "Nothing here"]
     >>= noCache

startWebServer serverConfig app

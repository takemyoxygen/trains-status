module Controller

open System
open Suave
open Suave.Http.RequestErrors

open Common
open Config
open Suave.Http
open Suave.Http.Applicatives
open Suave.Types
open Suave.Http.ServerErrors
open Suave.Http.Successful
open Suave.Http.Files

type StationResponse = {Name: string; LiveDeparturesUrl: string; Code: string}
type StationName = {Name: string}

let private unescape = Uri.UnescapeDataString

let checkStatus creds origin destination =
    match Status.check creds (unescape origin) (unescape destination) with
    | Status.NoOptionsFound(orig, dest) -> NOT_FOUND <| sprintf "No travel options found between %s  and  %s" orig dest
    | Status.TravelOptionsStatus(status) -> Json.asResponse status

let private liveDeparturesUrl (station: Stations.T) =
    sprintf "http://www.ns.nl/actuele-vertrektijden/avt?station=%s" (station.Code.ToLower())

let private toStationResponse (station: Stations.T) =
    {Name = station.Name; Code = station.Code; LiveDeparturesUrl = liveDeparturesUrl station}

let getClosest credentials lat lon count =
    Stations.getClosest credentials {Latitude = lat; Longitude = lon} count
    |> List.map toStationResponse

let favouriteStations config id =
    Stations.favouriteDirections config id
    |> List.map toStationResponse

let allStations creds =
    Stations.all creds
    |> List.map toStationResponse

let saveFavourites config user rawFavourites = 
    let favourites = Json.fromByteArray<StationName list> rawFavourites
                     |> Seq.map (fun s -> s.Name)
                     |> List.ofSeq

    Storage.saveFavourites config user favourites

let userInfo context = 
    let claims = async {
            match context.request.["token"] with
            | Some(token) -> return! Auth.getClaims token
            | None -> return None 
    }

    async {
        let! claims = claims
        match claims with
        | Some(c) -> return! Json.asResponse c context
        | None -> return! BAD_REQUEST "Invalid authentication token" context
    }

let stations config = 
    choose
        [GET >>= path "/api/stations/all" >>= request (fun _ -> allStations config.Credentials |> Json.asResponse)
         GET >>= path "/api/stations/nearby" >>= (fun context -> async {
            let stations = opt {
                let! lat = context.request.["lat"] |> Option.tryMap Double.TryParse
                let! lon = context.request.["lon"] |> Option.tryMap Double.TryParse
                let! count = context.request.["count"] |> Option.tryMap Int32.TryParse
                printfn "Looking for %i stations near %f, %f" count lat lon
                return getClosest config.Credentials lat lon count
            }
            match stations with
            | Some(s) -> return! Json.asResponse s context
            | None -> return None
        })]

let status config =
    GET >>= pathScan 
                "/api/status/%s/%s" 
                (fun (origin, destination) -> checkStatus config.Credentials origin destination)

let user config =
    choose
        [GET >>= pathScan "/api/user/%s/favourite" (favouriteStations config >> Json.asResponse)
         PUT >>= pathScan "/api/user/%s/favourite" (fun id -> request (fun req ->
                match saveFavourites config id req.rawForm with
                | Ok -> OK "Saved"
                | Error -> INTERNAL_ERROR "Failed"))
         GET >>= path "/api/user/info" >>= userInfo]

let content =
    let staticContent  =
        [".js"; ".jsx"; ".css"; ".html"; ".woff"; ".woff2"; ".ttf"]
        |> Seq.map (fun s -> s.Replace(".", "\."))
        |> String.concat "|"
        |> sprintf "(%s)$"

    choose
        [GET >>= path "/" >>= file "Index.html"
         GET >>= pathRegex staticContent >>= browseHome]

let notfound = NOT_FOUND "Nothing here"

let webParts config = 
    choose 
        [ stations config;
          user config;
          status config;
          content
          notfound ]
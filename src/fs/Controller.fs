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

let private asyncResponse (result: Async<'a>) (createWebPart: 'a -> WebPart) context =
    async.Bind(result, fun res -> createWebPart res context)

let private asyncJsonResponse (result: Async<'a>) = asyncResponse result Json.asResponse

let checkStatus creds origin destination =
    asyncResponse
        (Status.check creds (unescape origin) (unescape destination))
        (fun result ->
            match result with
            | None -> NOT_FOUND <| sprintf "No travel options found between %s  and  %s" origin destination
            | Some(status) -> Json.asResponse status)

let private liveDeparturesUrl (station: Stations.T) =
    sprintf "http://www.ns.nl/actuele-vertrektijden/avt?station=%s" (station.Code.ToLower())

let private toStationResponse (station: Stations.T) =
    {Name = station.Name; Code = station.Code; LiveDeparturesUrl = liveDeparturesUrl station}

let getClosest credentials lat lon count =
    Stations.getClosest credentials {Latitude = lat; Longitude = lon} count
    |> (Async.map <| List.map toStationResponse)

let favouriteStations config id =
    Stations.favouriteDirections config id
    |> (Async.map <| List.map toStationResponse)

let allStations creds =
    Stations.all creds
    |> (Async.map <| List.map toStationResponse)

let saveFavourites config user rawFavourites =
    let favourites = Json.fromByteArray<StationName list> rawFavourites
                     |> Seq.map (fun s -> s.Name)
                     |> List.ofSeq

    Storage.saveFavourites config user favourites

let userInfo (request: HttpRequest) =
    let claims = async {
            match request.["token"] with
            | Some(token) -> return! Auth.getClaims token
            | None -> return None
    }

    asyncResponse
        claims
        (function
            | Some(c) -> Json.asResponse c
            | None -> BAD_REQUEST "Invalid authentication token")

let stations config =
    choose
        [GET >>= path "/api/stations/all" >>= request (fun _ -> allStations config.Credentials |> asyncJsonResponse)
         GET >>= path "/api/stations/nearby" >>= request (fun request ->
            let parameters = opt {
                let! lat = request.["lat"] |> Option.tryMap Double.TryParse
                let! lon = request.["lon"] |> Option.tryMap Double.TryParse
                let! count = request.["count"] |> Option.tryMap Int32.TryParse
                return lat, lon, count
            }
            match parameters with
            | Some(lat, lon, count) ->
                asyncJsonResponse <| getClosest config.Credentials lat lon count
            | None -> BAD_REQUEST "lat, lon and count query params expected." )]

let status config =
    GET >>= pathScan
                "/api/status/%s/%s"
                (fun (origin, destination) -> checkStatus config.Credentials origin destination)

let user config =
    choose
        [GET >>= pathScan "/api/user/%s/favourite" (favouriteStations config >> asyncJsonResponse)
         PUT >>= pathScan "/api/user/%s/favourite" (fun id -> request (fun req ->
                asyncResponse
                    (saveFavourites config id req.rawForm)
                    (function
                        | Ok -> OK "Saved"
                        | Error -> INTERNAL_ERROR "Failed")))
         GET >>= path "/api/user/info" >>= request userInfo]

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

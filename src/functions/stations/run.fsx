#r "System.Net.Http"
#r "System.Web.Http"

#if !COMPILED
#r "../../../packages/Microsoft.Azure.WebJobs/lib/net45/Microsoft.Azure.WebJobs.Host.dll"
#endif

#load "../common/http.fsx"
#load "../common/xml.fsx"
#load "../common/json.fsx"
#load "../common/prelude.fsx"

open System
open System.Net
open System.Net.Http
open System.Net.Http.Headers
open Microsoft.Azure.WebJobs.Host

type Coordinates = 
    { Latitude: double
      Longitude: double }

type Station = 
    { Name: string
      LiveDeparturesUrl: string
      Code: string }

let private distance coord1 coord2 =
    let toRad x =  x * (Math.PI / 180.0)
    let haversin x = pown (sin <| x / 2.0) 2

    let r = 6371000.0
    let φ1, φ2 = toRad coord1.Latitude, toRad coord2.Latitude
    let λ1, λ2 = toRad coord1.Longitude, toRad coord2.Longitude

    2.0 * r * ((haversin(φ2 - φ1) + cos(φ1) * cos(φ2) * haversin(λ2 - λ1)) |> sqrt |> asin)

let private translateCoordinates stationXml =
    { Latitude = stationXml -!> "Lat" |> xval |> Double.Parse
      Longitude = stationXml -!> "Lon" |> xval |> Double.Parse }

let private translateStation stationXml =
    let code = stationXml -!> "Code" |> xval
    { Name = stationXml -!> "Namen" -!> "Lang" |> xval
      Code = code
      LiveDeparturesUrl = sprintf "http://www.ns.nl/actuele-vertrektijden/avt?station=%s" <| code.ToLower() }

let private translateResponse xml =
    (xparse xml) -!> "Stations" -*> "Station"
    |> Seq.map (fun stationXml -> translateCoordinates stationXml, translateStation stationXml)
    |> List.ofSeq

let private maybeSortByProximity lat lon stations =
    match lat, lon with
    | Some(lat), Some(lon) ->
        let location = {Latitude = lat; Longitude = lon}
        stations |> List.sortBy (fun (coord, station) -> distance coord location) 
    | _ -> stations


let private maybeTakeSome count stations =
    match count with
    | Some(c) -> stations |> List.take c
    | _ -> stations

let Run (req: HttpRequestMessage, log: TraceWriter) =
    
    let username = Environment.GetEnvironmentVariable("NS_USERNAME")
    let password = Environment.GetEnvironmentVariable("NS_PASSWORD")
    
    let queryString = System.Web.HttpUtility.ParseQueryString(req.RequestUri.Query)
    let lat = queryString.Get "lat" |> Option.ofObj |> Option.tryMap Double.TryParse
    let lon = queryString.Get "lon" |> Option.ofObj |> Option.tryMap Double.TryParse
    let count = queryString.Get "count" |> Option.ofObj |> Option.tryMap Int32.TryParse

    log.Info("Downloading stations list...")

    Http.get "http://webservices.ns.nl/ns-api-stations-v2" username password
    |> translateResponse 
    |> fun stations -> log.Info(sprintf "Got %i stations" stations.Length); stations
    |> maybeSortByProximity lat lon
    |> maybeTakeSome count
    |> List.map snd
    |> Json.serialize
    |> Http.Response.ofJson
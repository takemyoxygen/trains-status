#r "System.Net.Http"

#if !COMPILED
#r "../../../packages/Microsoft.Azure.WebJobs/lib/net45/Microsoft.Azure.WebJobs.Host.dll"
#endif

#load "../common/http.fsx"
#load "../common/xml.fsx"
#load "../common/json.fsx"

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

let Run (req: HttpRequestMessage, log: TraceWriter) =
    
    let username = Environment.GetEnvironmentVariable("NS_USERNAME")
    let password = Environment.GetEnvironmentVariable("NS_PASSWORD")
    
    log.Info("Downloading stations list...")

    Http.get "http://webservices.ns.nl/ns-api-stations-v2" username password
    |> translateResponse
    |> fun stations -> log.Info(sprintf "Got %i stations" stations.Length); stations
    |> List.map snd
    |> Json.serialize
    |> Http.Response.ofJson
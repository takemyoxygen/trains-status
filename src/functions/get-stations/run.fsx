#r "System.Net.Http"

#load "../common/xml.fsx"

#if !COMPILED
#r "../../../packages/Microsoft.Azure.WebJobs/lib/net45/Microsoft.Azure.WebJobs.Host.dll"
#r "../packages/FSharp.Data/2.3.2/lib/net40/FSharp.Data.dll"
#r "../packages/Newtonsoft.Json/9.0.1/lib/net45/Newtonsoft.Json.dll"
#endif

open System
open System.Net
open System.Net.Http
open System.Net.Http.Headers
open Microsoft.Azure.WebJobs.Host

open FSharp.Data
open FSharp.Data.HttpRequestHeaders

open Newtonsoft.Json
open Newtonsoft.Json.Serialization

open Xml

type Station = { 
    Name: string
    LiveDeparturesUrl: string
    Code: string
}

let private translateResponse xml =
    (xparse xml) -!> "Stations" -*> "Station"
    |> Seq.map (fun stationXml -> 
        let code = stationXml -!> "Code" |> xval
        { Name = stationXml -!> "Namen" -!> "Lang" |> xval
          Code = code
          LiveDeparturesUrl = sprintf "http://www.ns.nl/actuele-vertrektijden/avt?station=%s" <| code.ToLower() })
    |> List.ofSeq

let private settings = 
    new JsonSerializerSettings(ContractResolver = new CamelCasePropertyNamesContractResolver())

let toJsonString x = JsonConvert.SerializeObject(x, settings)

let Run (req: HttpRequestMessage, log: TraceWriter) =
    
    let username = Environment.GetEnvironmentVariable("NS_USERNAME")
    let password = Environment.GetEnvironmentVariable("NS_PASSWORD")
    
    log.Info("Downloading stations list...")

    let stationsXml = 
        Http.RequestString(
            "http://webservices.ns.nl/ns-api-stations-v2", 
            headers = [BasicAuth username password])

    log.Info("Stations list downloaded.")

    let stations = translateResponse stationsXml

    log.Info(sprintf "Got %i stations" stations.Length)

    let responseJson = toJsonString stations

    let res = new HttpResponseMessage()
    res.Content <- new StringContent(responseJson)
    res.StatusCode <- HttpStatusCode.OK
    res.Content.Headers.ContentType <- MediaTypeHeaderValue("text/json")
    res
#r "System.Net.Http"

#if !COMPILED
#r "../../../packages/Microsoft.Azure.WebJobs/lib/net45/Microsoft.Azure.WebJobs.Host.dll"
#r "../packages/FSharp.Data/2.3.2/lib/net40/FSharp.Data.dll"
#endif

open System
open System.Net.Http
open System.Net.Http.Headers
open Microsoft.Azure.WebJobs.Host

open FSharp.Data
open FSharp.Data.HttpRequestHeaders

open System.Net


let Run (req: HttpRequestMessage, log: TraceWriter) =
    
    let username = Environment.GetEnvironmentVariable("NS_USERNAME")
    let password = Environment.GetEnvironmentVariable("NS_PASSWORD")
    
    let stations = 
        Http.RequestString(
            "http://webservices.ns.nl/ns-api-stations-v2", 
            headers = [BasicAuth username password])

    let res = new HttpResponseMessage()
    res.Content <- new StringContent(stations)
    res.StatusCode <- HttpStatusCode.OK
    res.Content.Headers.ContentType <- MediaTypeHeaderValue("text/xml")
    res
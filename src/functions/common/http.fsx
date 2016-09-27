[<RequireQualifiedAccess>]
module Http

#r "System.Net.Http"

#if !COMPILED
#r "../packages/FSharp.Data/2.3.2/lib/net40/FSharp.Data.dll"
#endif

open System.Net
open System.Net.Http
open System.Net.Http.Headers

open FSharp.Data
open FSharp.Data.HttpRequestHeaders

/// Performs HTTP GET request to the given URL using Basic authentication
let get url username password = 
    Http.RequestString(url, headers = [BasicAuth username password])


module Response = 

    /// Creates 200 response with the content of given JSON string
    let ofJson json = 
        let res = new HttpResponseMessage(Content = new StringContent(json), StatusCode = HttpStatusCode.OK)
        res.Content.Headers.ContentType <- MediaTypeHeaderValue("text/json")
        res

module Http

open Common
open FSharp.Data

let private auth creds =
    HttpRequestHeaders.BasicAuth creds.Username creds.Password

let get creds url query =
    let queryString = (query |> Seq.map (fun (f, s) -> f + "=" + s) |> String.concat "&")
    printfn "Sending GET request to %s?%s" url queryString
    Http.RequestString(url, query = query, headers = [auth creds])

let getAsync creds url query =
    Http.AsyncRequestString(url, query = query, headers = [auth creds])
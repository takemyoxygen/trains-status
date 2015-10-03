module Auth

open FSharp.Data
open Newtonsoft.Json.Linq

let private key = "1070118148604-54uf2ocdsog6qsbigomu22v42aujahht.apps.googleusercontent.com"

let validate token = async {
    try
        let url = sprintf "https://www.googleapis.com/oauth2/v3/tokeninfo?id_token=%s" token
        let! response = Http.AsyncRequestString(url)
        let result = JObject.Parse response
        let email = result.Value<string>("email")
        let aud = result.Value<string>("aud")
        printfn "%O" response
        printfn "Email: %s and aud: %s" email aud
        return Some email
    with exn ->
        printfn "Failed to validate authentication token %s" token
        return None
}

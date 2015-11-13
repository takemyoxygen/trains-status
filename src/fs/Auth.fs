module Auth

open FSharp.Data
open Newtonsoft.Json.Linq

open Suave.Types
open Suave.Http.RequestErrors

type UserInfo =
    { Email: string;
      Id: string }

let private key = "1070118148604-54uf2ocdsog6qsbigomu22v42aujahht.apps.googleusercontent.com"

let getClaims token = async {
    try
        let url = sprintf "https://www.googleapis.com/oauth2/v3/tokeninfo?id_token=%s" token
        let! response = Http.AsyncRequestString(url)
        let result = JObject.Parse response
        let email = result.Value<string>("email")
        let aud = result.Value<string>("aud")
        let sub = result.Value<string>("sub")
        return
            if aud = key then Some {Email = email; Id = sub}
            else None
    with exn ->
        printfn "Failed to validate authentication token %s: %s" token exn.Message
        return None
}
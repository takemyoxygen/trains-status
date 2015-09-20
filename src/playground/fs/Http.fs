module Http

open Common
open FSharp.Data

let private auth creds =
    HttpRequestHeaders.BasicAuth creds.Username creds.Password

let get creds url query =
    Http.RequestString(url, query = query, headers = [auth creds])

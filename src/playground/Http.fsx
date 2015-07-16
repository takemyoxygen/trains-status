#load "Credentials.fsx"
#r "packages/Fsharp.Data/lib/net40/FSharp.Data.dll"

open FSharp.Data
open Credentials

module Http =
    
    let private auth = HttpRequestHeaders.BasicAuth Credentials.username Credentials.password

    let get url query = Http.RequestString(url, query = query, headers = [auth])

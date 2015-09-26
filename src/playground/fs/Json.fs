module Json

open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open Suave.Http
open Suave.Http.Successful
open Suave.Http.Writers

let private settings = new JsonSerializerSettings(ContractResolver = new CamelCasePropertyNamesContractResolver())

let toJsonString x = JsonConvert.SerializeObject(x, settings)

let asResponse response =
    toJsonString response
    |> OK
    >>= setMimeType "application/json"
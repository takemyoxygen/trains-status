module Json

open System.Text
open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open Suave.Http
open Suave.Http.Successful
open Suave.Http.Writers

let private settings = new JsonSerializerSettings(ContractResolver = new CamelCasePropertyNamesContractResolver())

let toJsonString x = JsonConvert.SerializeObject(x, settings)
let fromJsonString<'a> x = JsonConvert.DeserializeObject<'a>(x)

let asResponse response =
    toJsonString response
    |> OK
    >>= setMimeType "application/json"

let fromByteArray<'a> bytes =
    let s = System.Text.Encoding.UTF8.GetString bytes
    fromJsonString<'a> s

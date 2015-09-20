module Json

open Newtonsoft.Json
open Newtonsoft.Json.Serialization

let private settings = new JsonSerializerSettings(ContractResolver = new CamelCasePropertyNamesContractResolver())

let toJsonString x = JsonConvert.SerializeObject(x, settings)

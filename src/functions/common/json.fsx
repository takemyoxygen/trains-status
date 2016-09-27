[<RequireQualifiedAccess>]
module Json

#if !COMPILED
#r "../packages/Newtonsoft.Json/9.0.1/lib/net45/Newtonsoft.Json.dll"
#endif

open Newtonsoft.Json
open Newtonsoft.Json.Serialization

let private settings = 
    new JsonSerializerSettings(ContractResolver = new CamelCasePropertyNamesContractResolver())

/// Serialises given object to a JSON string
let serialize x = JsonConvert.SerializeObject(x, settings)
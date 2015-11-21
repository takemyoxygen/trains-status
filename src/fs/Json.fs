module Json

open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open Suave.Http
open Suave.Http.Successful
open Suave.Http.Writers
open Newtonsoft.Json.Converters

type OptionConverter() =
    inherit JsonConverter()
    override __.CanConvert(objectType) = 
        objectType.IsGenericType && objectType.GetGenericTypeDefinition() = typedefof<Option<_>>
    override __.WriteJson(writer, obj, serializer) =
        if (isNull obj) then writer.WriteNull()
        else
            let value = obj.GetType().GetProperty("Value").GetValue(obj)
            serializer.Serialize(writer, value)
            
    override __.ReadJson(_, _, _, _) = failwith "Not supported"

let converters: JsonConverter list = [ OptionConverter(); StringEnumConverter() ]

let private settings = 
    new JsonSerializerSettings(
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        Converters = ResizeArray(converters))

let toJsonString x = JsonConvert.SerializeObject(x, settings)
let fromJsonString<'a> x = JsonConvert.DeserializeObject<'a>(x)

let asResponse response =
    toJsonString response
    |> OK
    >>= setMimeType "application/json"

let fromByteArray<'a> bytes =
    let s = System.Text.Encoding.UTF8.GetString bytes
    fromJsonString<'a> s
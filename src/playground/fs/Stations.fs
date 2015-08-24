#if !INTERACTIVE
module Stations
#endif
open FSharp.Data

let private endpoint = "http://webservices.ns.nl/ns-api-stations-v2"

type T = 
    { Name : string
      Latitude : double
      Longitude : double }

type private Xml = XmlProvider< "samples/stations.xml" >

let all credentials = 
    let response = Http.get credentials endpoint []
    (Xml.Parse response).Stations 
    |> Seq.map (fun s -> 
        { Name = s.Namen.Lang
          Latitude = float s.Lat 
          Longitude = float s.Lon })
    |> List.ofSeq

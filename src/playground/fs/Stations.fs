#if !INTERACTIVE
module Stations
#endif

open FSharp.Data

open Common

let private endpoint = "http://webservices.ns.nl/ns-api-stations-v2"

type T =
    { Name : string
      Coordinates: Coordinates }

type private Xml = XmlProvider< "samples/stations.xml" >

let all credentials =
    let response = Http.get credentials endpoint []
    (Xml.Parse response).Stations
    |> Seq.filter (fun s -> s.Land = "NL")
    |> Seq.map (fun s ->
        { Name = s.Namen.Lang
          Coordinates = 
            { Latitude = float s.Lat
              Longitude = float s.Lon}})
    |> List.ofSeq

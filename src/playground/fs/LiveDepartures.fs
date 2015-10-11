module LiveDepartures

open System
open Http
open FSharp.Data

type private Xml = XmlProvider< "samples/live-departures.xml" >

type T =
    { Id : int
      DepartsAt : DateTime
      Destination : string
      Stops : string list }

let private endpoint = "http://webservices.ns.nl/ns-api-avt"

let from station creds =
    let xml = Http.get creds endpoint [ "station", station ]
    let data = Xml.Parse xml
    data.VertrekkendeTreins
    |> Seq.map (fun xml ->
                      { Id = xml.RitNummer
                        DepartsAt = xml.VertrekTijd
                        Destination = xml.EindBestemming
                        Stops =
                            xml.RouteTekst.Split(',')
                            |> Seq.map (fun s -> s.Trim())
                            |> List.ofSeq })
    |> List.ofSeq

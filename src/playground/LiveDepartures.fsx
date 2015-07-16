#load "Http.fsx"
#r "packages/Fsharp.Data/lib/net40/FSharp.Data.dll"
#r "System.Xml.Linq.dll"

open System
open FSharp.Data
open Http

module LiveDepartures = 
    type private Xml = XmlProvider< "samples/live-departures.xml" >
    
    type T = 
        { Id : int
          DepartsAt : DateTime
          Destination : string
          Stops : string list }
    
    let private endpoint = "http://webservices.ns.nl/ns-api-avt"
    
    let from station = 
        let xml = Http.get endpoint [ "station", station ]
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

module Stations

open FSharp.Data

open Common

let private endpoint = "http://webservices.ns.nl/ns-api-stations-v2"

type T =
    { Name : string
      Coordinates: Coordinates;
      Code: string}

type private Xml = XmlProvider<"samples/stations.xml">

let private downloadAll credentials = async {
    let! response = Http.getAsync credentials endpoint []
    return
        (Xml.Parse response).Stations
        |> Seq.filter (fun s -> s.Land = "NL")
        |> Seq.map (fun s ->
            { Name = s.Namen.Lang
              Code = s.Code
              Coordinates =
                { Latitude = float s.Lat
                  Longitude = float s.Lon}})
        |> List.ofSeq
}

let private downloader = MailboxProcessor<Credentials * AsyncReplyChannel<T list>>.Start(fun agent -> 
    let rec loop cached = async {
        let! creds, channel = agent.Receive()
        match cached with
        | Some(stations) -> 
            channel.Reply stations
            do! loop cached
        | None -> 
            let! stations = downloadAll creds
            channel.Reply stations
            do! loop <| Some stations
    }

    loop None)

let all credentials =
    downloader.PostAndReply(fun channel -> credentials, channel)

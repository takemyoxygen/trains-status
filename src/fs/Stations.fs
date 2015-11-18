module Stations

open FSharp.Data

open System
open Common

let private endpoint = "http://webservices.ns.nl/ns-api-stations-v2"

type Coordinates =
    { Latitude: double
      Longitude: double }

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
              Code = s.Code.ToLower()
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
    downloader.PostAndAsyncReply(fun channel -> credentials, channel)

let getClosest credentials origin count =
    let distance coord1 coord2 =
        let toRad x =  x * (Math.PI / 180.0)
        let haversin x = pown (sin <| x / 2.0) 2

        let r = 6371000.0
        let φ1, φ2 = toRad coord1.Latitude, toRad coord2.Latitude
        let λ1, λ2 = toRad coord1.Longitude, toRad coord2.Longitude

        2.0 * r * ((haversin(φ2 - φ1) + cos(φ1) * cos(φ2) * haversin(λ2 - λ1)) |> sqrt |> asin)
    async {
        let! stations = all credentials
        return 
            stations
            |> Seq.sortBy (fun st -> distance origin st.Coordinates)
            |> Seq.take count
            |> List.ofSeq
    }

let favouriteDirections config id = async {
    let! favs = Storage.getFavourites config id
    let! stations = all config.Credentials
    return
        stations
        |> Seq.filter (fun st -> favs |> List.contains st.Name)
        |> List.ofSeq
}
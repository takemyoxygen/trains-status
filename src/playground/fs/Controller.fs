module Controller

open System

open Common
open Status

type TravelInfo =
    { Time: DateTime;
      Legs: string list;
      ArrivalDelay: string;
      DepartureDelay: string}

type CheckStatusResponse =
    { Status: string;
      Message: string;
      Trains: TravelInfo list}

type GetStationsResponse = {Name: string}

let checkStatus creds origin destination =
    let status, options = Status.check creds origin destination
    let trainsResp =
        options
        |> List.map (fun opt ->
                { Time = opt.DepartureTime.Planned;
                  Legs = opt.Legs |> List.map (fun leg -> (leg.Stops |> List.last).Name);
                  ArrivalDelay = defaultArg opt.ArrivalDelay String.Empty
                  DepartureDelay = defaultArg opt.DepartureDelay String.Empty })

    let statusResp, messageResp =
        match status with
        | Ok-> "OK", null
        | Delayed -> "Delayed", null
        | NotFound -> "NotFound", (sprintf "No travel options found between %s and %s" origin destination)

    { Status = statusResp; Message = messageResp; Trains = trainsResp }

let getAllStations credentials =
    Stations.all credentials
    |> List.map (fun s -> {Name = s.Name})

let getClosest credentials lat lon count =
    let distance coord1 coord2 =
        let toRad x =  x * (Math.PI / 180.0)
        let haversin x = pown (sin <| x / 2.0) 2

        let r = 6371000.0
        let φ1, φ2 = toRad coord1.Latitude, toRad coord2.Latitude
        let λ1, λ2 = toRad coord1.Longitude, toRad coord2.Longitude

        2.0 * r * ((haversin(φ2 - φ1) + cos(φ1) * cos(φ2) * haversin(λ2 - λ1)) |> sqrt |> asin)

    let origin = {Latitude = lat; Longitude = lon}
    Stations.all credentials
    |> Seq.sortBy (fun st -> distance origin st.Coordinates)
    |> Seq.take count
    |> Seq.map (fun s -> {Name = s.Name})
    |> List.ofSeq

let favouriteStations () =
    [{Name = "Amsterdam Zuid"}; {Name = "Naarden-Bussum"}]

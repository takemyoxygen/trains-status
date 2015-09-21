module Controller

open System
open Suave.Http.RequestErrors

open Common

type GetStationsResponse = {Name: string}

let private unescape = Uri.UnescapeDataString

let checkStatus creds origin destination =
    match Status.check creds (unescape origin) (unescape destination) with
    | Status.NoOptionsFound(orig, dest) -> NOT_FOUND <| sprintf "No travel options found between %s  and  %s" orig dest
    | Status.TravelOptionsStatus(status) -> Json.asResponse status


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

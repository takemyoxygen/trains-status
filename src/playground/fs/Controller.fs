﻿#if !INTERACTIVE
module Controller
#endif

open Status

type CheckStatusResponse = {Status: string; Message: string}
type GetStationsResponse = {Name: string}

let checkStatus creds origin destination =
    match Status.check creds origin destination with
    | Ok-> {Status = "OK"; Message = null}
    | Delayed -> {Status = "Delayed"; Message = null}
    | NotFound -> {Status = "NotFound"; Message = sprintf "No travel options found between %s and %s" origin destination}

let getAllStations credentials =
    Stations.all credentials
    |> List.map (fun s -> {Name = s.Name})
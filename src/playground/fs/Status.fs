module Status

open System

type StationStatus =
    { Station: string;
      Time: DateTime;
      Delay: string }

type TravelOptionStatus =
    { From: StationStatus;
      To: StationStatus;
      Via: StationStatus list;
      Status: string } // ok, warning, cancelled

type TravelOptionsList =
    { Options: TravelOptionStatus list;
      Status: string} // warning or ok

type TravelOptionsStatusCheckResult =
    | NoOptionsFound of origin: string * destination: string
    | TravelOptionsStatus of TravelOptionsList

let private hasDelays (opt: TravelOptions.T) =
    opt.ArrivalDelay.IsSome ||
    opt.DepartureDelay.IsSome ||
    opt.Status = TravelOptions.Status.Cancelled ||
    opt.Status = TravelOptions.Status.Delayed

let check creds origin destination =
    match TravelOptions.find creds origin destination with
    | [] -> NoOptionsFound(origin, destination)
    | options ->
        let status = if options |> List.exists hasDelays then "warning" else "ok"
        let result =
            options
            |> List.map(fun opt ->
                let status =
                    match opt.Status with
                    | TravelOptions.Status.Delayed -> "warning"
                    | TravelOptions.Status.Cancelled -> "cancelled"
                    | _ -> "ok"
                { From = { Station = origin; Time = opt.DepartureTime.Planned; Delay = defaultArg opt.DepartureDelay null };
                  To = { Station = destination; Time = opt.ArrivalTime.Planned; Delay = defaultArg opt.ArrivalDelay null };
                  Status = status
                  Via =
                    opt.Legs
                    |> Seq.skip 1
                    |> Seq.map (fun leg ->
                        let stop = leg.Stops.[0]
                        { Station = stop.Name; Time = stop.Time; Delay = defaultArg stop.Delay null })
                    |> List.ofSeq })
        TravelOptionsStatus { Options = result; Status = status }

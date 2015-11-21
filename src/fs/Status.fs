module Status

open System

/// Significant stop on the route: origin, destination or a stop when train change is required
type StopInfo =
    { Station: string;
      Time: DateTime option;
      Delay: string option}

type TravelOptionStatus =
    | Ok = 0
    | Delayed = 1
    | Cancelled = 2

/// Particular travel option including departure/arrival time and transfers
type TravelOption =
    { From: StopInfo;
      To: StopInfo;
      Via: StopInfo list;
      Status: TravelOptionStatus;
      Warnings: string list; }

type DirectionStatus =
    | Ok = 0
    | Warning = 1

/// All travel options from an origin to a destination
type Direction =
    { Options: TravelOption list;
      Status: DirectionStatus }

// TODO check if API returns consistent statuses
// Maybe it would make sense to check for delays manually (departure delay, arrival delay, delays on stops along the route)
let private travelOptionStatus (opt: TravelOptions.T) =
    match opt.Status with
    | TravelOptions.Status.Delayed -> TravelOptionStatus.Delayed
    | TravelOptions.Status.Cancelled -> TravelOptionStatus.Cancelled
    | _ -> TravelOptionStatus.Ok

let private directionStatus (options: TravelOption list) =
    if options |> List.exists(fun opt -> opt.Status <> TravelOptionStatus.Ok) then DirectionStatus.Warning
    else DirectionStatus.Ok

let private travelOptionFrom origin destination creds (opt: TravelOptions.T) = async {
    let! outages =
        let legOutages =
            opt.Legs
            |> Seq.collect (fun leg -> [leg.PlannedOutage; leg.UnplannedOutage])
            |> Seq.filter Option.isSome
            |> Seq.map Option.get
            |> List.ofSeq

        (opt.Outages |> List.map (fun out -> out.Id)) 
        @ legOutages
        |> List.map (Outages.getMessageFor creds)
        |> Async.Parallel // only for simplicity; will be sequentialized by underlying MailboxProcessor in Outages.fs
        |> Async.map (Seq.filter Option.isSome >> Seq.map Option.get >> List.ofSeq)

    return
        { From =
            { Station = origin;
                Time = Some opt.DepartureTime.Planned;
                Delay = opt.DepartureDelay };
          To =
            { Station = destination;
                Time = Some opt.ArrivalTime.Planned;
                Delay =  opt.ArrivalDelay };
          Status = travelOptionStatus opt
          Warnings = outages
          Via =
            opt.Legs
            |> Seq.skip 1
            |> Seq.map (fun leg ->
                let stop = leg.Stops.[0]
                { Station = stop.Name;
                    Time = stop.Time;
                    Delay = stop.Delay})
            |> List.ofSeq }
}

let check creds origin destination = async {
    let! options = TravelOptions.find creds origin destination
    match options with
    | [] -> return None
    | options ->
        return!
            options 
            |> Seq.map(travelOptionFrom origin destination creds)
            |> Async.Parallel
            |> Async.map List.ofArray
            |> Async.map (fun options -> Some { Options = options; Status = directionStatus options })
}

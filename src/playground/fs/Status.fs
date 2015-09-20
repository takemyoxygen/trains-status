module Status

type Status =
    | Ok
    | Delayed
    | NotFound

let private hasDelays (opt: TravelOptions.T) =
    opt.ArrivalDelay.IsSome || opt.DepartureDelay.IsSome || opt.Status = TravelOptions.Status.Cancelled

let check creds origin destination =
    match TravelOptions.find creds origin destination with
    | [] -> NotFound, []
    | options when options |> Seq.exists hasDelays -> Delayed, options
    | _ as other -> Ok, other

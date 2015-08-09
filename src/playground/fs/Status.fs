#if !INTERACTIVE
module Status
#endif

type Status = 
    | Ok
    | Delayed
    | NotFound

let private hasDelays (opt: TravelOptions.T) = 
    opt.ArrivalDelay.IsSome || opt.DepartureDelay.IsSome

let check creds origin destination = 
    match TravelOptions.find creds origin destination with
    | [] -> NotFound
    | options when options |> Seq.exists hasDelays -> Delayed
    | _ -> Ok
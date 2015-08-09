#load "TravelOptions.fsx"

type Status = 
    | Ok
    | Delayed
    | NotFound

let private hasDelays (opt: TravelOptions.T) = 
    opt.ArrivalDelay.IsSome || opt.DepartureDelay.IsSome

let check origin destination = 
    match TravelOptions.find origin destination with
    | [] -> NotFound
    | options when options |> Seq.exists hasDelays -> Delayed
    | _ -> Ok
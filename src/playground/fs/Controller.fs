#if !INTERACTIVE
module Controller
#endif

open Status

let checkStatus creds origin destination =
    match Status.check creds origin destination with
    | Ok-> "OK"
    | Delayed -> "Delayed"
    | NotFound -> sprintf "No travel options found between %s and %s" origin destination


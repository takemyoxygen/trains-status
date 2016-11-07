[<RequireQualifiedAccess>]
module Option

let tryMap f = function
| Some(x) ->
    match f x with
    | true, value -> Some value
    | _ -> None
| None -> None
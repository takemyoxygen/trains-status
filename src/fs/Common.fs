module Common

type Credentials =
    { Username : string
      Password : string }

type Result =
    | Ok
    | Error

[<RequireQualifiedAccess>]
module Option =
    let tryMap f = function
    | Some(x) ->
        match f x with
        | true, value -> Some value
        | _ -> None
    | None -> None


type OptionBuilder() =
    member x.Bind(v,f) = Option.bind f v
    member x.Return v = Some v
    member x.ReturnFrom o = o

let opt = OptionBuilder()

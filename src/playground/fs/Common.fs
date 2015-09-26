module Common

type Credentials =
    { Username : string
      Password : string }

type Coordinates =
    { Latitude: double
      Longitude: double;}

[<RequireQualifiedAccess>]
module Option =
    let tryMap f = function
    | Some(x) ->
        match f x with
        | true, _ as result -> Some <| snd result
        | _ -> None
    | None -> None


type OptionBuilder() =
    member x.Bind(v,f) = Option.bind f v
    member x.Return v = Some v
    member x.ReturnFrom o = o

let opt = OptionBuilder()

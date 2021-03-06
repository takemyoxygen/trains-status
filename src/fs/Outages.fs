module Outages

open System
open System.Collections.Generic

open FSharp.Data
open Common

#if INTERACTIVE

[<Literal>]
let private XmlPath = __SOURCE_DIRECTORY__ + "/../samples/outages.xml"

#else

[<Literal>]
let private XmlPath = "samples/outages.xml"

#endif

type private Xml = XmlProvider< XmlPath >

type T = {
    Id : string;
    Advice: string option;
    Reason: string option;
    Message: string option;
    Delay: string option;
}

let generateMessage outage =
    [outage.Reason; outage.Advice; outage.Message; outage.Delay;]
    |> Seq.filter Option.isSome
    |> Seq.map Option.get
    |> String.concat ". "


let actual creds =
    async {
        let! response = Http.getAsync creds "http://webservices.ns.nl/ns-api-storingen" ["actual", "true"]
        let data = Xml.Parse response
        let planned =
            data.Gepland.Storings
            |> Seq.map (fun st ->
                { Id = st.Id
                  Advice = st.Advies
                  Message = st.Bericht
                  Reason = st.Oorzaak
                  Delay = st.Vertraging })

        let unplanned =
            data.Ongepland.Storings
            |> Seq.map (fun st ->
                { Id = st.Id
                  Advice = st.Advies
                  Message = st.Bericht
                  Reason = st.Oorzaak
                  Delay = st.Vertraging })

        return Seq.append planned unplanned |> List.ofSeq
    }

type Cache = Dictionary<string, string>

type OutagesCommand =
    | Update of id: string * message: string
    | Get of id: string * credentials: Credentials * replyChannel: AsyncReplyChannel<string option>

let tryFind key (cache: Cache) =
    match cache.TryGetValue(key) with
    | true, value -> Some value
    | _ -> None

let private outages = MailboxProcessor.Start(fun agent ->
    let rec loop (cache: Cache) = async {
        let! message = agent.Receive()
        match message with
        | Update(id, message) -> cache.[id] <- message
        | Get(id, creds, replyChannel) ->
            match tryFind id cache with
            | Some(message) -> replyChannel.Reply(Some message)
            | _ ->
                let! all = actual creds

                all
                |> Seq.map (fun out -> out.Id, generateMessage out)
                |> Seq.filter (snd >> String.IsNullOrEmpty >> not)
                |> Seq.iter (fun (id, message) -> cache.[id] <- message)

                replyChannel.Reply <| tryFind id cache
        do! loop cache
    }
    loop <| Cache())


let knownOutage id message = outages.Post <| Update(id, message)

let getMessageFor creds id = outages.PostAndAsyncReply(fun channel -> Get(id, creds, channel))

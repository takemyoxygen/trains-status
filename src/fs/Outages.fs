module Outages

open System.Collections.Generic

open FSharp.Data
open Common

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

type private Xml = XmlProvider< "samples/outages.xml" >

let actual creds = 
    async { 
        let! data = Xml.AsyncLoad("samples/outages.xml")
//        let! response = Http.getAsync creds "http://webservices.ns.nl/ns-api-storingen" ["actual", "true"]
//        let data' = Xml.Parse response
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
                all |> List.iter (fun out -> cache.[out.Id] <- generateMessage out)
                replyChannel.Reply <| tryFind id cache
        do! loop cache
    }
    loop <| Cache())


let knownOutage id message = outages.Post <| Update(id, message)

let getMessageFor creds id = outages.PostAndAsyncReply(fun channel -> Get(id, creds, channel))


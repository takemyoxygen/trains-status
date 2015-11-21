module Outages

open System.Collections.Generic

type OutagesCommand =
    | Update of id: string * message: string
    | Get of id: string * replyChannel: AsyncReplyChannel<string option>

let private outages = MailboxProcessor.Start(fun agent ->
    let rec loop (cache: Dictionary<string, string>) = async {
        let! message = agent.Receive()
        match message with
        | Update(id, message) ->
            cache.[id] <- message
        | Get(id, replyChannel) ->
            match cache.TryGetValue(id) with
            | true, message -> replyChannel.Reply(Some message)
            | _ -> replyChannel.Reply(None) // TODO in case of cache miss - send a request for the outages to the web API
        do! loop cache
    }
    loop <| Dictionary<_, _>())


let knownOutage id message = outages.Post <| Update(id, message)

let getMessageFor id = outages.PostAndAsyncReply(fun channel -> Get(id, channel))
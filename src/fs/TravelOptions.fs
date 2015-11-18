module TravelOptions

open System
open FSharp.Data

type private Xml = XmlProvider< "samples/travel-options.xml" >

type Status =
    | AccordingToPlan
    | Amended
    | Delayed
    | New
    | NotOptimal
    | Cancelled
    | PlanChanged
    | Unknown
    static member fromString  = function
        | "VOLGENS-PLAN" -> Status.AccordingToPlan
        | "GEWIJZIGD" -> Status.Amended
        | "VERTRAAGD" -> Status.Delayed
        | "NIEUW" -> Status.New
        | "NIET-OPTIMAAL" -> Status.NotOptimal
        | "NIET-MOGELIJK" -> Status.Cancelled
        | "PLAN-GEWIJZIGD" -> Status.PlanChanged
        | _ -> Status.Unknown

type Estimated<'a> =
    { Planned : 'a
      Actual : 'a option }

type Stop =
    { Name : string
      Time : DateTime option
      Delay : string option }

type OutageId = string

type Outage =
    { Id : OutageId
      Severe : bool
      Text : string }

type TravelLeg =
    { TrainId : int
      Stops : Stop list
      Status : string
      PlannedOutage : OutageId option
      UnplannedOutage : OutageId option }

type T =
    { Outages : Outage list
      Transfers : int
      Duration : Estimated<TimeSpan>
      DepartureDelay : string option
      ArrivalDelay : string option
      DepartureTime : Estimated<DateTime>
      ArrivalTime : Estimated<DateTime>
      Legs : TravelLeg list
      IsOptimal : bool
      Status : Status }

let private endpoint = "http://webservices.ns.nl/ns-api-treinplanner"

let find creds origin destination = async {
    let! xml =
        Http.getAsync creds endpoint [ "fromStation", origin;
                                  "toStation", destination;
                                  "previousAdvices", "0" ]

    let data = Xml.Parse xml
    return
        data.ReisMogelijkheids
        |> Seq.map (fun option ->
               { Outages =
                     option.Meldings
                     |> Seq.map (fun outage ->
                            { Id = outage.Id
                              Severe = outage.Ernstig
                              Text = outage.Text })
                     |> List.ofSeq
                 Transfers = option.AantalOverstappen
                 Duration =
                     { Planned = option.GeplandeReisTijd.TimeOfDay
                       Actual = option.ActueleReisTijd |> Option.map (fun dt -> dt.TimeOfDay) }
                 Legs =
                     option.ReisDeels
                     |> Seq.map (fun leg ->
                            { TrainId = defaultArg leg.RitNummer 0
                              Status = leg.Status
                              PlannedOutage = leg.GeplandeStoringId
                              UnplannedOutage = leg.OngeplandeStoringId
                              Stops =
                                  leg.ReisStops
                                  |> Seq.map (fun stop ->
                                         { Name = stop.Naam
                                           Time = stop.Tijd
                                           Delay = stop.VertrekVertraging })
                                  |> List.ofSeq })
                     |> List.ofSeq
                 DepartureTime =
                     { Planned = option.GeplandeVertrekTijd
                       Actual = Some option.ActueleVertrekTijd }
                 ArrivalTime =
                     { Planned = option.GeplandeAankomstTijd
                       Actual = Some option.ActueleAankomstTijd }
                 DepartureDelay = option.VertrekVertraging
                 ArrivalDelay = option.AankomstVertraging
                 IsOptimal = option.Optimaal
                 Status = Status.fromString option.Status })
        |> List.ofSeq
}
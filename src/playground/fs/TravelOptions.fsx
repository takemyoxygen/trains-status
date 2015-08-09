#load "Http.fsx"
#r "../packages/Fsharp.Data/lib/net40/FSharp.Data.dll"
#r "System.Xml.Linq.dll"

open System
open FSharp.Data

type private Xml = XmlProvider< "../samples/travel-options.xml" >

type Estimated<'a> = 
    { Planned : 'a
      Actual : 'a option }

type Stop = 
    { Name : string
      Time : DateTime
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
      Status : string }

let private endpoint = "http://webservices.ns.nl/ns-api-treinplanner"

let find origin destination = 
    let xml = 
        Http.get endpoint [ "fromStation", origin;
                            "toStation", destination;
                            "previousAdvices", "0" ]
    
    let data = Xml.Parse xml
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
                        { TrainId = leg.RitNummer
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
             Status = option.Status })
    |> List.ofSeq

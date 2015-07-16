#load "setup.fsx"
#load "credentials.fsx"
#r "packages/Fsharp.Data/lib/net40/FSharp.Data.dll"
#r "System.Xml.Linq.dll"

open System
open System.Xml.Linq
open FSharp.Data
open Credentials

let auth = HttpRequestHeaders.BasicAuth Credentials.username Credentials.password

type LiveDeparturesXml = XmlProvider<"samples/live-departures.xml">
type LiveDeparture = {
    Id: int;
    DepartsAt: DateTime
    Destination: string;
    Stops: string list
}

type Estimated<'a> = {
    Planned: 'a;
    Actual: 'a option
}

type TravelOptionsXml = XmlProvider<"samples/travel-options.xml">

type Stop = {
    Name: string;
    Time: DateTime;
    Delay: string option;
}

type OutageId = string

type Outage = {
    Id: OutageId;
    Severe: bool;
    Text: string;
}

type TravelLeg = {
    TrainId: int;
    Stops: Stop list;
    Status: string;
    PlannedOutage: OutageId option;
    UnplannedOutage: OutageId option
}

type TravelOption = {
    Outages: Outage list;
    Transfers: int;
    Duration: Estimated<TimeSpan>
    DepartureDelay: string option;
    ArrivalDelay: string option;
    DepartureTime: Estimated<DateTime>;
    ArrivalTime: Estimated<DateTime>;
    Legs: TravelLeg list;
    IsOptimal: bool;
    Status: string
}

let (-->) (xml: XElement) name = 
    let xname = XName.Get name
    xml.Element xname |> Option.ofObj

let (-?>) (xml: XElement option) name =
    xml |> Option.map (fun x -> x --> name)

let (-!>) (xml: XElement option) f =
    xml |> Option.map (fun x -> f x.Value)

let liveDepartures from =
    let endpoint = "http://webservices.ns.nl/ns-api-avt"
    let response = Http.RequestString
                    ( endpoint,
                      query = ["station", from],
                      headers = [auth])
    let departures = LiveDeparturesXml.Parse response
    departures.VertrekkendeTreins
    |> Seq.map (fun xml ->
                   { Id = xml.RitNummer;
                     DepartsAt = xml.VertrekTijd;
                     Destination = xml.EindBestemming;
                     Stops = xml.RouteTekst.Split(',') |> Seq.map (fun s -> s.Trim()) |> List.ofSeq})
    |> List.ofSeq



let routes origin destination =
    let endpoint = "http://webservices.ns.nl/ns-api-treinplanner"
    let response = Http.RequestString
                    ( endpoint,
                      query = ["fromStation", origin; "toStation", destination],
                      headers = [auth])

    let xml = TravelOptionsXml.Parse response
    xml.ReisMogelijkheids
    |> Seq.map (fun option ->
                    { Outages = option.Meldings |> Seq.map (fun outage -> { Id = outage.Id; Severe = outage.Ernstig; Text = outage.Text }) |> List.ofSeq;
                      Transfers = option.AantalOverstappen;
                      Duration = {Planned = option.GeplandeReisTijd.TimeOfDay; Actual = option.ActueleReisTijd |> Option.map (fun dt -> dt.TimeOfDay)};
                      Legs = option.ReisDeels |> Seq.map (fun leg ->
                                                                { TrainId = leg.RitNummer;
                                                                  Status = leg.Status;
                                                                  PlannedOutage = leg.GeplandeStoringId;
                                                                  UnplannedOutage = leg.OngeplandeStoringId;
                                                                  Stops = leg.ReisStops |> Seq.map (fun stop -> {Name = stop.Naam; Time = stop.Tijd; Delay = stop.VertrekVertraging}) |> List.ofSeq})
                                              |> List.ofSeq
                      DepartureTime = {Planned = option.GeplandeVertrekTijd; Actual = Some option.ActueleVertrekTijd}
                      ArrivalTime = {Planned = option.GeplandeAankomstTijd; Actual = Some option.ActueleAankomstTijd};
                      DepartureDelay = option.VertrekVertraging;
                      ArrivalDelay = option.AankomstVertraging;
                      IsOptimal = option.Optimaal;
                      Status = option.Status})
    |> List.ofSeq

let response = routes "Naarden-Bussum" "Amsterdam Zuid"

response.[0].Duration
//
//let sample = System.IO.File.ReadAllText "samples/travel-options.xml"
//             |> TravelOptionsXml.Parse
//
//let filtered = sample.ReisMogelijkheids
//                |> Seq.filter (fun xml -> xml.ReisDeels |> Seq.exists (fun deel -> deel.RitNummer = 5852))
//                |> List.ofSeq
//
//filtered.[0].XElement.Elements(System.Xml.Linq.XName.Get("GeplandeReisTijd"))
//|> Seq.map (fun x -> x.Value)
//|> Seq.iter (printfn "%s")

#load "setup.fsx"
#load "credentials.fsx"
#r "packages/Fsharp.Data/lib/net40/FSharp.Data.dll"
#r "System.Xml.Linq.dll"

open System
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
    Actual: 'a
}

type TravelOptionsXml = XmlProvider<"samples/travel-options.xml">

type Stop = {
    Name: string;
    Time: DateTime
}

type Outage = {
    Id: string;
    Severe: bool;
    Text: string;
}

type TravelLeg = {
    TrainId: int;
    Stops: Stop list;
    Status: string;
    Type: string;
}

type TravelOption = {
    Outages: Outage list;
    Transfers: int;
    Duration: Estimated<TimeSpan>
    Delay: string option;
    DepartureTime: Estimated<DateTime>;
    ArrivalTime: Estimated<DateTime>;
    Legs: TravelLeg list
}

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
                      Duration = {Planned = option.GeplandeAankomstTijd.TimeOfDay; Actual = option.ActueleAankomstTijd.TimeOfDay};
                      Legs = [];
                      DepartureTime = {Planned = DateTime.Now; Actual = DateTime.Now}
                      ArrivalTime = Unchecked.defaultof<_>;
                      Delay = option.AankomstVertraging})
    |> List.ofSeq

let response = routes "Naarden-Bussum" "Amsterdam Zuid"

let sample = System.IO.File.ReadAllText "samples/travel-options.xml"
             |> TravelOptionsXml.Parse


let filtered = sample.ReisMogelijkheids
                |> Seq.filter (fun xml -> xml.ReisDeels |> Seq.exists (fun deel -> deel.RitNummer = 5852))
                |> List.ofSeq

filtered.[0].XElement.Elements(System.Xml.Linq.XName.Get("GeplandeReisTijd"))
|> Seq.map (fun x -> x.Value)
|> Seq.iter (printfn "%s")
//response.[0].GeplandeReisTijd.TimeOfDay
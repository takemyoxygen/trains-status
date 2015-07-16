#load "setup.fsx"

#load "LiveDepartures.fsx"
#load "TravelOptions.fsx"

open LiveDepartures
open TravelOptions

let departures = LiveDepartures.from "Naarden-Bussum"
let options = TravelOptions.find "Naarden-Bussum" "Amsterdam Zuid"

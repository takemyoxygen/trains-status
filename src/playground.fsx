#r "System.Xml.Linq"

#load "load.fsx"

#load "fs/Json.fs"
#load "fs/Async.fs"
#load "fs/Common.fs"
#load "fs/Config.fs"
#load "fs/Http.fs"
#load "fs/Outages.fs"
#load "fs/TravelOptions.fs"
#load "fs/Status.fs"

open Common

let creds = {Username = "username"; Password = "password"}
let result = Status.check creds "foo" "bar" |> Async.RunSynchronously |> Option.get
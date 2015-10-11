#load "load.fsx"

#load "fs/Json.fs"
#load "fs/Common.fs"
#load "fs/Config.fs"
#load "fs/Storage.fs"

let config = Config.current __SOURCE_DIRECTORY__

let id = "user-id"

let favs = ["Naarden-Bussum"; "Amsterdam Zuid"; "Utrecht Centraal"; "Amsterdam Centraal"]

let favs' = Storage.getFavourites config id

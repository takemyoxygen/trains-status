module Storage

open Microsoft.WindowsAzure.Storage
open FSharp.Azure.Storage.Table

open Config

type UserFavourites =
    { [<RowKey>] Id: string;
      [<PartitionKey>] Source: string
      Favourites: string }

type Result =
    | Ok
    | Error

let private tableClientFrom config =
    let account = CloudStorageAccount.Parse config.ConnectionString
    account.CreateCloudTableClient()

[<Literal>]
let private FavouritesTable = "favourites"

let getFavourites config id =
    let tableClient = tableClientFrom config
    let query = Query.all<UserFavourites> |> Query.where <@ fun x s -> s.RowKey = id @>
    let favs = fromTable tableClient FavouritesTable query |> Seq.tryHead
    match favs with
    | Some(favs, _) -> favs.Favourites |> Json.fromJsonString<string list>
    | None -> []

let saveFavourites config id (favourites: string list) =
    let tableClient = tableClientFrom config
    let favs = { Id = id; Source = "google"; Favourites = Json.toJsonString favourites }
    let result = favs |> InsertOrReplace |> inTable tableClient FavouritesTable
    if result.HttpStatusCode >= 200 && result.HttpStatusCode < 300 then Ok
    else Error

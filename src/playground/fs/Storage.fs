module Storage

open System
open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Table

open Config

type UserFavourites(id, origin, favs: string) =
    inherit TableEntity(origin, id)
    new() = UserFavourites(String.Empty, String.Empty, String.Empty)
    member val Favourites = favs with get, set

type Result =
    | Ok
    | Error

[<Literal>]
let private FavouritesTable = "favourites"

let private tableClientFrom config =
    let account = CloudStorageAccount.Parse config.ConnectionString
    account.CreateCloudTableClient().GetTableReference(FavouritesTable)

let getFavourites config id =
    let table = tableClientFrom config
    let query = (new TableQuery<UserFavourites>()).Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, id))
    printfn "Loading favourite stations for %s from Azure..." id
    let favs = table.ExecuteQuery query |> Seq.tryHead
    match favs with
    | Some(favs) -> favs.Favourites |> Json.fromJsonString<string list>
    | None -> []

let saveFavourites config id (favourites: string list) =
    let table = tableClientFrom config
    let favs = new UserFavourites(id, "google", Json.toJsonString favourites)
    let result = favs |> TableOperation.InsertOrReplace |> table.Execute
    if result.HttpStatusCode >= 200 && result.HttpStatusCode < 300 then Ok
    else Error

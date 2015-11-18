module Storage

open System
open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Table

open Common
open Config

type UserFavourites(id, origin, favs: string) =
    inherit TableEntity(origin, id)
    new() = UserFavourites(String.Empty, String.Empty, String.Empty)
    member val Favourites = favs with get, set

[<Literal>]
let private FavouritesTable = "favourites"

let private tableClientFrom config =
    let account = CloudStorageAccount.Parse config.ConnectionString
    account.CreateCloudTableClient().GetTableReference(FavouritesTable)

let getFavourites config id = async {
    let table = tableClientFrom config
    let query = (new TableQuery<UserFavourites>()).Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, id))
    printfn "Loading favourite stations for %s from Azure..." id
    let! result = table.ExecuteQuerySegmentedAsync(query, null) |> Async.AwaitTask
    if result.Results.Count > 0 then 
        return result.Results.[0].Favourites |> Json.fromJsonString<string list>
    else
        return []
}

let saveFavourites config id (favourites: string list) = async {
    let table = tableClientFrom config
    let favs = new UserFavourites(id, "google", Json.toJsonString favourites)
    let! result = favs |> TableOperation.InsertOrReplace |> table.ExecuteAsync |> Async.AwaitTask
    if result.HttpStatusCode >= 200 && result.HttpStatusCode < 300 then 
        return Ok
    else return Error
}

module Config

open Suave.Sockets
open Suave.Logging
open System
open System.IO
open Common

type Config =
    { Port: Port
      Credentials: Credentials;
      LogLevel: LogLevel;
      ConnectionString: string}

let private args =
    Environment.GetCommandLineArgs()
    |> Seq.map (fun arg -> arg.Split('='))
    |> Seq.filter (fun tokens -> tokens.Length = 2)
    |> Seq.map (fun tokens -> tokens.[0].ToLower(), tokens.[1])
    |> Map.ofSeq

let private getArg name defaultValue =
    match args.TryFind name with
    | Some(x) -> x
    | None -> defaultValue

let current home =
    let username, password = args.TryFind "username", args.TryFind "password"
    let username, password =
        match username, password with
        | Some(user), Some(pass) -> user, pass
        | _ ->
            let file = Path.Combine(home, "credentials.txt")
            printfn "Using credentials from \"%s\"" file
            let lines = File.ReadAllLines file
            defaultArg username lines.[0], defaultArg password lines.[1]

    { Port = Port.Parse <| getArg "port" "8081"
      LogLevel = getArg "loglevel" "warn" |> LogLevel.FromString
      ConnectionString = String.Empty
      Credentials =
          { Username = username
            Password = password } }

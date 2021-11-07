open Suave
open Suave.Operators
open Suave.Filters
open Suave.WebSocket
open Suave.Successful
open Suave.RequestErrors
open SimpleChatroom.ClientHandler
open SimpleChatroom.RoomHandler
open Newtonsoft.Json
open Newtonsoft.Json.Serialization

let JSON v =
    let jsonSerializerSettings = JsonSerializerSettings()
    jsonSerializerSettings.ContractResolver 
      <- CamelCasePropertyNamesContractResolver()
    JsonConvert.SerializeObject(v, jsonSerializerSettings)
    |> OK
    >=> Writers.setMimeType "application/json"

let app : WebPart = 
  choose [
    path "/websocket" >=> handShake handleMessage
    GET >=> choose [
        path "/rooms" >=> warbler (fun _ -> getRoomInfos() |> JSON)
        ]
    NOT_FOUND "Found no handlers."
    ]

[<EntryPoint>]
let main _ =
  startWebServer defaultConfig app
  0
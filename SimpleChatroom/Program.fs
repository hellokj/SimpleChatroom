open Suave
open Suave.Operators
open Suave.Filters
open Suave.WebSocket
open Suave.RequestErrors
open SimpleChatroom.ClientHandler

let app : WebPart = 
  choose [
    path "/websocket" >=> handShake handleMessage
    NOT_FOUND "Found no handlers."
    ]

[<EntryPoint>]
let main _ =
  startWebServer defaultConfig app
  0
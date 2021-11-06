open Suave
open Suave.Operators
open Suave.Filters
open Suave.Logging
open Suave.WebSocket

open SimpleChatroom.ClientHandler

let app : WebPart = 
  choose [
    path "/websocket" >=> handShake handleMessage]

[<EntryPoint>]
let main _ =
  startWebServer { defaultConfig with logger = Targets.create Verbose [||] } app
  0
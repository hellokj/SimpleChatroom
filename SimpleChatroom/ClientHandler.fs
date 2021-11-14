namespace SimpleChatroom
open System
open System.Collections
open Suave.WebSocket
open Suave.Sockets
open Suave.Sockets.Control
open Suave.Http
open Helper
open Metadata
open RoomHandler

module ClientHandler = 
    let private roomHandler = RoomHandler() :> IRoomHandler
    let handleMessage (webSocket : WebSocket) (_: HttpContext) = 
        socket {
            let mutable loop = true

            while loop do
                let! (opCode, data, _) = webSocket.read()

                let message = UTF8.toString data |> deserializeMessage
                let clientId = 
                    match String.isEmpty message.ClientId with
                    | true -> Guid.NewGuid().ToString()
                    | false -> message.ClientId

                match opCode with
                | Text -> 
                    match message.Action with
                    | Action.Join -> 
                        roomHandler.join message.RoomId clientId webSocket 
                        |> 
                        broadcast true clientId message.NickName $"{message.NickName} join the room"
                        |> ignore
                    | Action.Leave -> 
                        roomHandler.leave message.RoomId clientId 
                        |> 
                        broadcast true clientId message.NickName $"{message.NickName} leaves the room"
                        |> ignore
                    | Action.Broadcast -> 
                        roomHandler.getClients message.RoomId |> Seq.toArray
                        |>
                        broadcast false clientId message.NickName message.Content |> ignore
                    | _ -> 0 |> ignore
                | Close -> 
                    if (String.IsNullOrEmpty(message.RoomId) <> true) then 
                        roomHandler.leave message.RoomId clientId 
                        |> broadcast true clientId message.NickName $"{message.NickName} leaves the room"
                        |> ignore
                    let emptyResponse = [||] |> ByteSegment
                    webSocket.send Close emptyResponse true |> ignore
                    loop <- false
                | _ -> ()
        }
        

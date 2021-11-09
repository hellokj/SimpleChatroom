namespace SimpleChatroom
open System
open Suave.WebSocket
open Suave.Sockets
open Suave.Sockets.Control
open Suave.Http
open Helper
open RoomHandler

module ClientHandler = 
    
    let handleMessage (webSocket : WebSocket) (context: HttpContext)= 
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
                    | Join -> 
                        joinRoom message.RoomId clientId webSocket 
                        |> 
                        broadcast true clientId message.NickName $"{message.NickName} join the room"
                        |> ignore
                    | Leave -> 
                        leaveRoom message.RoomId clientId webSocket 
                        |> 
                        broadcast true clientId message.NickName $"{message.NickName} leaves the room"
                        |> ignore
                    | Broadcast -> 
                        getRoomClients message.RoomId
                        |>
                        broadcast false clientId message.NickName message.Content |> ignore
                    | _ -> 0 |> ignore
                | Close -> 
                    leaveRoom message.RoomId clientId webSocket 
                    |> broadcast true clientId message.NickName $"{message.NickName} leaves the room"
                    |> ignore
                    let emptyResponse = [||] |> ByteSegment
                    webSocket.send Close emptyResponse true |> ignore
                    loop <- false
                | _ -> ()
        }
        

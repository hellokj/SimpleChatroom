namespace SimpleChatroom
open System.Collections
open System
open Suave.WebSocket
open Suave.Sockets
open Suave.Sockets.Control
open Suave.Http
open MessageHelper
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

                let room = roomDict.GetOrAdd(message.RoomId, fun _ -> 
                    let newRoom = Concurrent.ConcurrentDictionary<string, WebSocket>()
                    newRoom.TryAdd(clientId, webSocket) |> ignore
                    newRoom)
                
                room.TryAdd(clientId, webSocket) |> ignore
                let mutable clients = room |> Seq.map(fun x -> x.Value) |> Seq.toArray

                match opCode with
                | Text -> 
                    match message.Action with
                    | Join -> 
                        joinRoom room clientId webSocket 
                        |> 
                        broadcast clientId message.RoomId $"{message.NickName} join the room"
                        |> ignore
                    | Leave -> 
                        leaveRoom room clientId webSocket 
                        |> 
                        broadcast clientId message.RoomId $"{message.NickName} leaves the room"
                        |> ignore
                    | Broadcast -> 
                        broadcast clientId message.RoomId message.Content clients |> ignore
                    | _ -> 0 |> ignore
                | Close -> 
                    leaveRoom room clientId webSocket 
                    |> broadcast clientId message.RoomId $"{message.NickName} leaves the room"
                    |> ignore
                    let emptyResponse = [||] |> ByteSegment
                    webSocket.send Close emptyResponse true |> ignore
                    loop <- false
                | _ -> ()
        }
        

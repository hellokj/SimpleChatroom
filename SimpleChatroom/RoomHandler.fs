namespace SimpleChatroom
open System.Collections
open Suave.WebSocket
open Suave.Sockets
open FSharp.Json


module RoomHandler = 
    let roomDict = Concurrent.ConcurrentDictionary<string, Concurrent.ConcurrentDictionary<string, WebSocket>>()

    let getRoomInfos() = 
        let infos = roomDict |> Seq.map(fun x -> {| Key = x.Key; Count = x.Value.Count |})
        infos

    let joinRoom (room: Concurrent.ConcurrentDictionary<string, WebSocket>) (clientId:string) (webSocket:WebSocket) =
        room.TryAdd(clientId, webSocket) |> ignore
        let clients = room.Values |> Seq.toArray
        clients

    let leaveRoom (room: Concurrent.ConcurrentDictionary<string, WebSocket>) (clientId:string) (webSocket:WebSocket) =
        room.TryRemove(Generic.KeyValuePair<string, WebSocket>(clientId, webSocket)) |> ignore
        let clients = room.Values |> Seq.toArray
        clients

    let broadcast (from:string) (roomId:string) (content:string) (webSockets:WebSocket[]) = 
        let reply = { FromId = from; RoomId = roomId; Content = content; Count = webSockets.Length }

        let byteResponse =
          reply
          |> Json.serialize
          |> System.Text.Encoding.ASCII.GetBytes
          |> ByteSegment
        webSockets
        |> Seq.map(fun x -> 
        async {
            return! x.send Text byteResponse true
        })
        |> Async.Parallel
        |> Async.Ignore
        |> Async.RunSynchronously


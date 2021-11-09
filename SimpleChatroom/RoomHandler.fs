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

    let getRoom (roomId: string) = 
        roomDict.GetOrAdd(roomId, fun _ -> Concurrent.ConcurrentDictionary<string, WebSocket>())

    let getRoomClients (roomId: string) = 
        getRoom roomId |> Seq.map(fun x -> Generic.KeyValuePair<string, WebSocket>(x.Key, x.Value)) |> Seq.toArray

    let joinRoom (roomId: string) (clientId:string) (webSocket:WebSocket) =
        let room = getRoom roomId
        room.TryAdd(clientId, webSocket) |> ignore
        let clients = room |> Seq.map(fun x -> Generic.KeyValuePair<string, WebSocket>(x.Key, x.Value)) |> Seq.toArray
        clients

    let leaveRoom (roomId: string) (clientId:string) (webSocket:WebSocket) =
        let room = getRoom roomId
        room.TryRemove(Generic.KeyValuePair<string, WebSocket>(clientId, webSocket)) |> ignore
        let clients = room |> Seq.map(fun x -> Generic.KeyValuePair<string, WebSocket>(x.Key, x.Value)) |> Seq.toArray
        clients

    let broadcast (isFromSystem: bool) (fromId: string) (from: string) (content: string) (clients: Generic.KeyValuePair<string, WebSocket>[])= 
        clients
        // map each pair to replyWebSocketTuple
        |> Seq.map(fun pair -> ({IsFromSystem = isFromSystem; FromId = fromId; From = from; ClientId = pair.Key; Content = content;}, pair.Value))
        |> Seq.map(fun tuple -> 
            let reply, websocket = tuple
            let byteResponse = 
                reply |> Json.serialize |> System.Text.Encoding.ASCII.GetBytes |> ByteSegment
            async{
                return! websocket.send Text byteResponse true
            })
        |> Async.Parallel
        |> Async.Ignore
        |> Async.RunSynchronously
        // TODO : deal with the fail socket operation
        |> ignore

namespace SimpleChatroom
open System.Collections
open Suave.WebSocket
open Suave.Sockets
open Suave.Http
open FSharp.Json

type IRoomHandler = 
    abstract member get : roomId:string -> Concurrent.ConcurrentDictionary<string, WebSocket>
    abstract member join : roomId:string -> clientId:string -> clientSocket:WebSocket -> Generic.KeyValuePair<string, WebSocket>[]
    abstract member leave : roomId:string -> clientId:string -> clientSocket:WebSocket -> Generic.KeyValuePair<string, WebSocket>[]

    // may be get the dictionary from outer source
type RoomHandler() =
    let roomDict = Concurrent.ConcurrentDictionary<string, Concurrent.ConcurrentDictionary<string, WebSocket>>()
    
    member this.get(roomId:string) =
        roomDict.GetOrAdd(roomId, fun _ -> Concurrent.ConcurrentDictionary<string, WebSocket>())

    interface IRoomHandler with
        member this.get (roomId:string) = 
            roomDict.GetOrAdd(roomId, fun _ -> Concurrent.ConcurrentDictionary<string, WebSocket>())
    
        member this.join (roomId: string) (clientId:string) (webSocket:WebSocket) = 
            let room = this.get roomId
            room.TryAdd(clientId, webSocket) |> ignore
            let clients = room |> Seq.map(fun x -> Generic.KeyValuePair<string, WebSocket>(x.Key, x.Value)) |> Seq.toArray
            clients

        member this.leave (roomId: string) (clientId:string) (webSocket:WebSocket) = 
            let room = this.get roomId
            room.TryRemove(Generic.KeyValuePair<string, WebSocket>(clientId, webSocket)) |> ignore
            let clients = room |> Seq.map(fun x -> Generic.KeyValuePair<string, WebSocket>(x.Key, x.Value)) |> Seq.toArray
            clients


module SocketHandler = 
    let private handler = RoomHandler() :> IRoomHandler

    let roomDict = Concurrent.ConcurrentDictionary<string, Concurrent.ConcurrentDictionary<string, WebSocket>>()

    let getRoomInfos() = 
        let infos = roomDict |> Seq.map(fun x -> {| Key = x.Key; Count = x.Value.Count |})
        infos

    let joinRoom (room: Concurrent.ConcurrentDictionary<string, WebSocket>) (clientId:string) (webSocket:WebSocket) =
        room.TryAdd(clientId, webSocket) |> ignore
        let clients = room.Values |> Seq.map(fun x -> x) |> Seq.toArray
        clients

    let leaveRoom (room: Concurrent.ConcurrentDictionary<string, WebSocket>) (clientId:string) (webSocket:WebSocket) =
        room.TryRemove(Generic.KeyValuePair<string, WebSocket>(clientId, webSocket)) |> ignore
        let clients = room.Values |> Seq.map(fun x -> x) |> Seq.toArray
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


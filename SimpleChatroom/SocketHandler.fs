namespace SimpleChatroom
open System.Collections
open Suave.WebSocket
open Suave.Sockets
open FSharp.Json

type IRoomHandler = 
    abstract member get : 
        roomId:string -> Concurrent.ConcurrentDictionary<string, WebSocket>
    abstract member join : 
        roomId:string -> clientId:string -> clientSocket:WebSocket -> Generic.KeyValuePair<string, WebSocket>[]
    abstract member leave : 
        roomId:string -> clientId:string -> clientSocket:WebSocket -> Generic.KeyValuePair<string, WebSocket>[]

type RoomHandler() =
    // TODO : may get the dictionary from outer source
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
    let private roomHandler = RoomHandler() :> IRoomHandler

    let joinRoom (roomId:string) (clientId:string) (webSocket:WebSocket) =
        roomHandler.join roomId clientId webSocket

    let leaveRoom (roomId:string) (clientId:string) (webSocket:WebSocket) =
        roomHandler.leave roomId clientId webSocket

    let getRoomClients (roomId:string) =
        roomHandler.get roomId |> Seq.map(fun x -> Generic.KeyValuePair<string, WebSocket>(x.Key, x.Value)) |> Seq.toArray

    let broadcast (isFromSystem: bool) (fromId:string) (from:string) (content:string) (pairs:Generic.KeyValuePair<string, WebSocket>[]) = 
        pairs
        |> Seq.map(fun pair -> 
            let reply = { ClientId = pair.Key; IsFromSystem = isFromSystem; FromId = fromId; From = from; Content = content; }
            let byteResponse = reply |> Json.serialize |> System.Text.Encoding.ASCII.GetBytes |> ByteSegment
            async {
                return! pair.Value.send Text byteResponse true
            })
        |> Async.Parallel
        |> Async.Ignore
        |> Async.RunSynchronously
        // TODO : deal with the fail socket operation
        |> ignore

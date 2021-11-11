namespace SimpleChatroom
open System.Collections
open Suave.WebSocket
open Suave.Sockets
open FSharp.Json

module RoomHandler = 
    type IRoomHandler = 
        abstract member join : 
            roomId:string -> clientId:string -> clientSocket:WebSocket -> Generic.KeyValuePair<string, WebSocket>[]
        abstract member leave : 
            roomId:string -> clientId:string -> Generic.KeyValuePair<string, WebSocket>[]
        abstract member getClients :
            roomId:string -> Generic.KeyValuePair<string, WebSocket>[]
            

    type RoomHandler() =
        // TODO : may get the dictionary from outer source
        let roomDict = Concurrent.ConcurrentDictionary<string, Concurrent.ConcurrentDictionary<string, WebSocket>>()
        
        member private this.add(roomId:string) = 
            let newRoom = Concurrent.ConcurrentDictionary<string, WebSocket>()
            roomDict.TryAdd(roomId, newRoom) |> ignore
            newRoom
    
        member private this.checkRoomExist(roomId:string) = 
            match roomDict.ContainsKey roomId with
            | true -> true
            | _ -> false
    
        member this.get(roomId:string) =
            match roomDict.TryGetValue roomId with
            | true, room -> room
            | _ -> failwith $"Can't find {roomId}"
    
        interface IRoomHandler with
            member this.join (roomId: string) (clientId:string) (webSocket:WebSocket) = 
                let room = 
                    match this.checkRoomExist roomId with
                    | true -> this.get roomId
                    | _ -> this.add roomId
                room.TryAdd(clientId, webSocket) |> ignore
                let clients = room |> Seq.map(fun x -> Generic.KeyValuePair<string, WebSocket>(x.Key, x.Value)) |> Seq.toArray
                clients
    
            member this.leave (roomId: string) (clientId:string) = 
                let mutable room = this.get roomId
                match room.TryRemove clientId with
                | true, _ -> room |> Seq.map(fun x -> Generic.KeyValuePair<string, WebSocket>(x.Key, x.Value)) |> Seq.toArray
                | _ -> failwith "Remove fail."

            member this.getClients (roomId:string) = 
                this.get roomId |> Seq.map(fun x -> Generic.KeyValuePair<string, WebSocket>(x.Key, x.Value)) |> Seq.toArray

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

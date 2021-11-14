namespace SimpleChatroom

open Suave
open FSharp.Json
open System
open Metadata

module Helper = 
    let deserializeMessage msg = 
        try
            let data = Json.deserialize<Message> msg
            data
        with _ -> 
            {
                ClientId = String.Empty;
                RoomId = String.Empty;
                NickName = String.Empty;
                Content = String.Empty;
                Action = None;
            }

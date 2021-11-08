namespace SimpleChatroom

open Suave
open FSharp.Json
open System

module MessageHelper = 
    let deserializeMessage msg = 
        try
            let data = Json.deserialize<Message> msg
            data
        with _ -> 
            {
                ClientId = Guid.NewGuid().ToString(); 
                RoomId = String.Empty;
                NickName = String.Empty;
                Content = String.Empty;
                Action = None;
            }

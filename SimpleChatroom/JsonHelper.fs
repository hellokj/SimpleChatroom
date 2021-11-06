namespace SimpleChatroom

open FSharp.Json
open System

module JsonHelper = 
    let deserializeMessage msg = 
        try
            let data = Json.deserialize<Message> msg
            data
        with ex -> 
            {
                ClientId = String.Empty; 
                RoomId = String.Empty;
                NickName = String.Empty;  
                Content = String.Empty;
                Action = None;
            }
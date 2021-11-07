namespace SimpleChatroom
open Suave
open Suave.Operators
open Suave.Successful
open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open FSharp.Json
open System

module JsonHelper = 
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

    let JSON v =
        let jsonSerializerSettings = new JsonSerializerSettings()
        jsonSerializerSettings.ContractResolver 
          <- new CamelCasePropertyNamesContractResolver()
        JsonConvert.SerializeObject(v, jsonSerializerSettings)
        |> OK
        >=> Writers.setMimeType "application/json"
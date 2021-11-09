﻿namespace SimpleChatroom

type Action = 
    | Join
    | Leave
    | Broadcast
    | None

type Message =
    { 
        ClientId: string
        RoomId: string
        NickName: string
        Content: string
        Action: Action
    }

type Reply = 
    {
        IsFromSystem: bool
        ClientId: string
        FromId: string
        From: string
        Content: string
    }
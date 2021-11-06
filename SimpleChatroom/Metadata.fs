namespace SimpleChatroom

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
        FromId: string
        RoomId: string
        Content: string
        Count: int
    }
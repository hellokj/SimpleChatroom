# SimpleChatroom
websocket server for simple chatroom using F#

### Run server

```console
cd SimpleChatroom
dotnet restore
dotnet run SimpleChatroom
```

### Testing client project

- https://github.com/hellokj/simple-chatroom-vue

### Dependency

- Suave
- FSharp.Json
- NewtonSoft.json

### System architecture

- **Metadata**
    Define the using type between server and client.
- **Helper**
    Helper module for retrieving the message from client.
- **RoomHandler**
    Manage the room actions including join, leaving and broadcast the specific room.
- **ClientHanlder**
    The most important part of handling the websocket request and deciding which action to do.


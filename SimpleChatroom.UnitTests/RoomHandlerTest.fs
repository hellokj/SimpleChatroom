namespace SimpleChatroom.UnitTests

module RoomHandlerTests =
    open Xunit
    open Moq
    open System
    open Suave.WebSocket
    open SimpleChatroom.RoomHandler

    let getAnyWebSocket = (It.IsAny<WebSocket>(): WebSocket)

    [<Fact>]
    let ``getClients ExistingRoom NotEmpty``() = 
        let roomId = "roomId"
        let existingClient = "client"
        let roomHandler = RoomHandler() :> IRoomHandler
        roomHandler.join roomId existingClient getAnyWebSocket |> ignore

        let clients = roomHandler.getClients roomId

        Assert.NotEmpty(clients)

    [<Fact>]
    let ``getClients NotExistingRoom ThrowException``() = 
        let roomId = "roomId"
        let roomHandler = RoomHandler() :> IRoomHandler
        Assert.Throws<Exception>(fun () -> roomHandler.getClients roomId |> ignore) |> ignore

    [<Fact>]
    let ``join NewRoom returnSelfAsClient`` () =
        let roomId = "roomId"
        let client = "client"
        let roomHandler = RoomHandler() :> IRoomHandler
        let clients = roomHandler.join roomId client getAnyWebSocket

        let self = clients.[0]
        Assert.NotEmpty(clients)
        Assert.Equal(client, self.Key)

    [<Fact>]
    let ``join ExistingRoom ContainOtherClient``() = 
        let roomId = "roomId"
        let client1 = "client1"
        let roomHandler = RoomHandler() :> IRoomHandler
        roomHandler.join roomId client1 getAnyWebSocket |> ignore

        let client2 = "client2"
        let clients = roomHandler.join roomId client2 getAnyWebSocket

        let clientIds = clients |> Seq.map(fun x -> x.Key)
        Assert.Contains(client1, clientIds)
        Assert.Contains(client2, clientIds)

    [<Fact>]
    let ``leave ExistingRoom Success``() =
        let roomId = "roomId"
        let clientId = "clientId"
        let roomHandler = RoomHandler() :> IRoomHandler
        let mutable clients = roomHandler.join roomId clientId getAnyWebSocket

        Assert.NotEmpty(clients)
        clients <- roomHandler.leave roomId clientId

        Assert.Empty(clients)

    [<Fact>]
    let ``leave NotExistingRoom ThrowExcpetion``() =
        let roomId = "roomId"
        let clientId = "clientId"
        let roomHandler = RoomHandler() :> IRoomHandler
        Assert.Throws<Exception>(fun () -> roomHandler.leave roomId clientId |> ignore) |> ignore



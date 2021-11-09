namespace SimpleChatroom.UnitTests

module RoomHandlerTests =
    open System.Collections
    open Xunit
    open Moq
    open Suave.WebSocket
    open SimpleChatroom.RoomHandler

    let initRoom = 
        roomDict = Concurrent.ConcurrentDictionary<string, Concurrent.ConcurrentDictionary<string, WebSocket>>() |> ignore

    let getAnyWebSocket = (It.IsAny<WebSocket>(): WebSocket)

    [<Fact>]
    let ``getRoomClients ExistingRoom NotEmpty``() = 
        initRoom
        let roomId = "roomId"
        joinRoom roomId "clientId" getAnyWebSocket |> ignore

        let clients = getRoomClients roomId

        Assert.NotEmpty(clients)

    [<Fact>]
    let ``getRoomClients NotExistingRoom Empty``() = 
        initRoom
        let roomId = "roomId"
        let clients = getRoom roomId

        Assert.Empty(clients)

    [<Fact>]
    let ``joinRoom createNewRoomToJoin returnSelfAsClient`` () =
        let roomId = "roomId"
        let client = "client"
        let clients = joinRoom roomId client getAnyWebSocket

        let self = clients.[0]
        Assert.NotEmpty(clients)
        Assert.Equal(client, self.Key)

    [<Fact>]
    let ``joinRoom joinExistingRoom ContainOtherClient``() = 
        let roomId = "roomId"
        let client1 = "client1"
        
        joinRoom roomId client1 getAnyWebSocket |> ignore

        let client2 = "client2"
        let clients = joinRoom roomId client2 getAnyWebSocket

        let clientIds = clients |> Seq.map(fun x -> x.Key)
        Assert.Contains(client1, clientIds)
        Assert.Contains(client2, clientIds)

    [<Fact>]
    let ``leaveRoom leaveExistingRoom Success``() =
        let roomId = "roomId"
        let client = "client"


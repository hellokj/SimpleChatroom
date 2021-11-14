namespace SimpleChatroom.UnitTests

module HelperTest = 
    open Xunit
    open Moq
    open System
    open FSharp.Json
    open SimpleChatroom.Metadata
    open SimpleChatroom.Helper

    [<Fact>]
    let ``deserializeMessage success messageWithClientData``() = 
        let testMessage = { ClientId = "clientId"; RoomId = "roomId"; Action = Action.None; NickName = It.IsAny<string>(); Content = It.IsAny<string>();}
        let serializedData = Json.serialize(testMessage)

        let message = deserializeMessage serializedData

        Assert.Equal(testMessage.ClientId, message.ClientId)

    [<Fact>]
    let ``deserializeMessage wrongInput messageWithDefaultValue``() =
        let wrongInput = String.Empty
        let message = deserializeMessage wrongInput

        Assert.Equal(message.Action, Action.None)
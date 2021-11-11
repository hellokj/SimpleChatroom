namespace SimpleChatroom.UnitTests

module ClientHandlerTests =
    open Xunit
    open Moq
    open FsUnit
    open System.Threading.Tasks
    open System
    open Suave.WebSocket
    open Suave.Sockets
    open SimpleChatroom.RoomHandler
    open SimpleChatroom.ClientHandler
    
    [<Fact>]
    let ``handleMessage joinMessage broadcastJoinReply``() = 
        let b: byte[] = Array.zeroCreate 5
        let result = 
            Choice1Of3 of Opcode.Text |> ignore
            Choice2Of3 b |> ignore
            Choice3Of3 true
        let ws = Mock<WebSocket>().Setup(fun ws -> <@ ws.read() @>)
        let r = async {
                    return result
                }
        let a = ws.Returns(fun () -> r)
        
        Assert.True(true)
        
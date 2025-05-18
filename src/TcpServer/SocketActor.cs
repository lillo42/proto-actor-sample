using Proto;

namespace TcpServer;

public class SocketActor : IActor
{
    public async Task ReceiveAsync(IContext context)
    {
        if (context.Message is not SocketAccepted socketAccepted)
        {
            return;
        }

        var socket = socketAccepted.Socket;
        var buffer = new byte[socket.Available];
        await socket.ReceiveAsync(buffer);
        
        var props = Props
            .FromProducer(() => new ProcessActor())
            .WithChildSupervisorStrategy(new OneForOneStrategy( 
                (_, exception) =>
                {
                    Console.WriteLine("Error: {0}", exception);
                    return SupervisorDirective.Restart;
                },
                3,
                TimeSpan.FromSeconds(1)));
        try
        {
            var actor = context.SpawnNamed(props, "json-serializer");
            context.Send(actor, new SocketReceived(buffer));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
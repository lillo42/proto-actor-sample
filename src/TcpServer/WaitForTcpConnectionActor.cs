using System.Net.Sockets;
using Proto;

namespace TcpServer;

public class WaitForTcpConnectionActor(int port) : IActor
{
    private static readonly Props Props = Props.FromProducer(() => new ReceiveBytesActor())
        .WithChildSupervisorStrategy(new OneForOneStrategy(
            (_, exception) =>
            {
                Console.WriteLine("Error: {0}", exception);
                return SupervisorDirective.Restart;
            },
            3,
            TimeSpan.FromSeconds(1)));
    
    private TcpListener? _listener;
    
    public async Task ReceiveAsync(IContext context)
    {
        if (context is { Message: Terminated, Sender: not null })
        {
            Close();
        }
        else if (context.Message is Started)
        {
            Open();
            context.Send(context.Self, new WaitForNextConnection());
        }
        else if (context.Message is ProcessCompleted)
        {
            Console.WriteLine("stopping actor: {0}", context.Sender);
            await context.StopAsync(context.Sender!);
        }
        else if(context.Message is WaitForNextConnection)
        {
            var socket = await AcceptTcpClientAsync(context.CancellationToken);
            Console.WriteLine("Accepted connection from {0}", socket.RemoteEndPoint);
            var actor = context.Spawn(Props);
            context.Send(actor, new SocketAccepted(socket));
            context.Send(context.Self, new WaitForNextConnection());
        }
    }

    private void Open()
    {
        Close();
        Console.WriteLine("Listening on port 9091");
        _listener = TcpListener.Create(port);
        _listener.Start();
    }

    private async Task<Socket> AcceptTcpClientAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Waiting for connection...");
        return await _listener!.AcceptSocketAsync(cancellationToken);
    }

    private void Close()
    {
        Console.WriteLine("Closing listener");
        _listener?.Dispose();
    }
}
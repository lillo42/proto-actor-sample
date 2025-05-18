using System.Net.Sockets;
using Proto;

namespace TcpServer;

public class ReceiveBytesActor : IActor
{
    private Socket? _socket;
    private byte[]? _buffer;
    
    public async Task ReceiveAsync(IContext context)
    {
        if (context.Message is Terminated)
        {
            Console.WriteLine("Terminating actor");
            _buffer = null;
            _socket?.Dispose();
            await context.Children.StopMany(context);
        }
        else if (context.Message is ProcessCompleted pc)
        {
            Console.WriteLine("Process completed");
            await context.StopAsync(pc.Id);
            context.Send(context.Parent!, new ProcessCompleted(context.Self));
        }
        else if (context.Message is SocketAccepted socketAccepted)
        {
            await SocketAccepted(socketAccepted.Socket);
            
            var props = Props.FromProducer(() => new ProcessActor());
            var actor = context.SpawnNamed(props, "json-serializer");
            context.Send(actor, new SocketReceived(_buffer!));
        }
        else if (context.Message is ResendSocketAccepted resend)
        {
            context.Send(resend.Id, new SocketReceived(_buffer!));
        }
    }
    
    private async Task SocketAccepted(Socket socket)
    {
        if (_socket != null)
        {
            return;
        }
        
        _socket = socket;
        _buffer = new byte[_socket.Available];
        await _socket.ReceiveAsync(_buffer);
        
    }
}
using System.Net;
using System.Net.Sockets;
using Proto;
using TcpServer;

var system = new ActorSystem();
var props = Props.FromProducer(() => new SocketActor())
    .WithChildSupervisorStrategy(new OneForOneStrategy( 
        (_, _) => SupervisorDirective.Stop,
        1,
        null))
    ;

var server = new TcpListener(IPAddress.Any, 9091);
server.Start();

var cancellationTokenSource = new CancellationTokenSource();
Console.CancelKeyPress += (_, _) =>
{
    cancellationTokenSource.Cancel();
};

Console.WriteLine("Listening on port 9091");
while (!cancellationTokenSource.IsCancellationRequested)
{
    try
    {
        Console.WriteLine("Waiting for connection...");
        var socket = await server.AcceptSocketAsync(cancellationTokenSource.Token);
        Console.WriteLine("Accepted connection from {0}", socket.RemoteEndPoint);
        var actor = system.Root.Spawn(props);
        system.Root.Send(actor, new SocketAccepted(socket));
    }
    catch (OperationCanceledException)
    {
    }
}

server.Stop();
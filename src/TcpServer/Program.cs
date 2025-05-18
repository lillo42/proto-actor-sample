using Proto;
using TcpServer;

var system = new ActorSystem();

var cancellationTokenSource = new CancellationTokenSource();
Console.CancelKeyPress += (_, _) =>
{
    cancellationTokenSource.Cancel();
};

system.Root.Spawn(Props.FromProducer(() => new WaitForTcpConnectionActor(9091)));

while (!cancellationTokenSource.IsCancellationRequested)
{
    await Task.Delay(1_000);
}

await system.ShutdownAsync();

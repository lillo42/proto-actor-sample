using Microsoft.Extensions.Hosting;
using Proto;
using Proto.Cluster;

namespace VirtualActor;

public class ActorSystemClusterHostedService(ActorSystem actorSystem) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await actorSystem.Cluster().StartMemberAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await actorSystem.Cluster().ShutdownAsync();
    }
}

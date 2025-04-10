using Microsoft.Extensions.Logging;
using Proto;
using Proto.Cluster;

namespace VirtualActor;

public class GreetingActor(
    IContext context,
    ClusterIdentity clusterIdentity,
    ILogger<GreetingActor> logger
) : GreetingGrainBase(context)
{
    private int _state = 0;

    public override Task SayHello(SayHelloRequest request)
    {
        logger.LogInformation(
            "Hello {Name} (from cluster: {ClusterIdentity} | Invoked count: {InvokedCount})",
            request.Name,
            clusterIdentity.Identity,
            _state
        );

        _state++;

        return Task.CompletedTask;
    }
}

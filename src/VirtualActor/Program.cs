// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Proto;
using Proto.Cluster;
using Proto.Cluster.Partition;
using Proto.Cluster.Testing;
using Proto.DependencyInjection;
using Proto.Remote.GrpcNet;
using Serilog;
using VirtualActor;

Serilog.Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var host = new HostBuilder()
    .UseSerilog()
    .ConfigureServices(
        (_, services) =>
        {
            services
                .AddHostedService<ActorSystemClusterHostedService>()
                .AddSingleton(provider =>
                {
                    var actorSystemConfig = Proto.ActorSystemConfig.Setup();

                    var remoteConfig = GrpcNetRemoteConfig.BindToLocalhost();

                    var clusterConfig = ClusterConfig
                        .Setup(
                            clusterName: "VirtualActor",
                            clusterProvider: new TestProvider(
                                new TestProviderOptions(),
                                new InMemAgent()
                            ),
                            identityLookup: new PartitionIdentityLookup()
                        )
                        .WithClusterKind(
                            kind: GreetingGrainActor.Kind,
                            prop: Props.FromProducer(
                                () =>
                                    new GreetingGrainActor(
                                        (context, clusterIdentity) =>
                                            ActivatorUtilities.CreateInstance<GreetingActor>(
                                                provider,
                                                context,
                                                clusterIdentity
                                            )
                                    )
                            )
                        );

                    return new ActorSystem(actorSystemConfig)
                        .WithServiceProvider(provider)
                        .WithRemote(remoteConfig)
                        .WithCluster(clusterConfig);
                });
        }
    )
    .Build();

var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
Proto.Log.SetLoggerFactory(loggerFactory);

_ = host.RunAsync();

while (true)
{
    Console.Write("Say your name (or q to quit): ");
    var fromName = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(fromName))
    {
        continue;
    }

    if (fromName == "q")
    {
        break;
    }

    Console.Write("Say another name (or q to quit): ");

    var toName = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(toName))
    {
        continue;
    }

    if (toName == "q")
    {
        break;
    }

    var actorSystem = host.Services.GetRequiredService<ActorSystem>();
    var actor = actorSystem.Cluster().GetGreetingGrain(fromName);
    await actor.SayHello(new SayHelloRequest { Name = toName }, CancellationToken.None);
}

await host.WaitForShutdownAsync();

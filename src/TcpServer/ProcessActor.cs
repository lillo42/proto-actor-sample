using System.Text.Json;
using Proto;

namespace TcpServer;

public class ProcessActor : IActor
{
    public Task ReceiveAsync(IContext context)
    {
        if (context.Message is not SocketReceived socketReceived)
        {
            return Task.CompletedTask;
        }

        var json = JsonSerializer.Deserialize<Sample>(socketReceived.Data)!;
        Console.WriteLine("Received sample with id: {0} and name: {1}", json.Id, json.Name);
        return Task.CompletedTask;
    }
}
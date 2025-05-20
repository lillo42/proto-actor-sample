using System.Text.Json;
using Proto;

namespace TcpServer;

public class ProcessActor : IActor
{
    public Task ReceiveAsync(IContext context)
    {
        if (context.Message is Restarting)
        {
            context.Send(context.Parent!, new ResendBufferReceived());
        }
        else if (context.Message is BufferReceived socketReceived)
        {
            var json = JsonSerializer.Deserialize<Sample>(socketReceived.Data)!;
            Console.WriteLine("Received sample with id: {0} and name: {1}", json.Id, json.Name);
            context.Send(context.Parent!, new ProcessCompleted());
        }
        return Task.CompletedTask;
    }
}
using Proto;

var system = new ActorSystem();
var props = Props.FromProducer(() => new GreetingdActor());
var greeter = system.Root.Spawn(props);

while (true)
{
    Console.Write("What is your name(q to quit)? ");
    var name = Console.ReadLine();

    if (string.IsNullOrEmpty(name))
    {
        continue;
    }

    if (name == "q")
    {
        break;
    }


    system.Root.Send(greeter, new Hello(name));

    await Task.Delay(TimeSpan.FromSeconds(1));
}


public record Hello(string Who);

public class GreetingdActor : IActor
{
    public Task ReceiveAsync(IContext context)
    {
        if (context.Message is Hello hello)
        {
            Console.WriteLine("Hello {0}", hello.Who);
        }

        return Task.CompletedTask;
    }
}

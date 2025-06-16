// See https://aka.ms/new-console-template for more information

using Proto;

var system = new ActorSystem();
var actorOrder = system.Root.Spawn(Props.FromProducer(() => new OrderActor()));
var actorPayment = system.Root.Spawn(Props.FromProducer(() => new PaymentActor()));

system.EventStream.Subscribe<OrderCreated>(system.Root, actorPayment);

var cts = new CancellationTokenSource();

Console.CancelKeyPress += (_, _) => cts.Cancel();

var orderId = 0;
while (!cts.IsCancellationRequested)
{
    Console.Write(
        """
        Select one options:
        1) New Order
        2) List all orders
        3) Pay an order
        4) List orders waiting for payment
        q) Quit
        
        Options: 
        """
        );

    var option = Console.ReadLine();
    if (option == "1")
    {
        Console.Write("Amount: ");
        var amount = Console.ReadLine();
        orderId++;
        system.Root.Send(actorOrder, new CreateOrder(orderId, decimal.Parse(amount!)));
    }
    else if (option == "2")
    {
        var orders = await system.Root.RequestAsync<IEnumerable<Order>>(actorOrder, new AllOrder());
        foreach (var order in orders)
        {
            Console.WriteLine("Order ID: {0}, Amount: {1}", order.Id, order.Amount);
        }
    }
    else if (option == "3")
    {
        Console.Write("OrderID: ");
        var id = Console.ReadLine();
        system.EventStream.Publish(new OrderPaid(int.Parse(id!)));
    }
    else if (option == "4")
    {
        Console.Write("OrderID: ");
        var id = Console.ReadLine();
        var isWaiting = await system.Root.RequestAsync<bool>(actorPayment, new IsWaitingForPayment(int.Parse(id!)));
        Console.WriteLine("IsWaiting for Payment: {0}", isWaiting);
        
    }
    else if (option == "q")
    {
        break;
    }
}


await system.DisposeAsync();

public class OrderActor : IActor
{
    private readonly List<Order> _orders = [];
    public Task ReceiveAsync(IContext context)
    {
        if (context.Message is CreateOrder createOrder)
        {
            var order = new Order(createOrder.Id, createOrder.Amount);
            _orders.Add(order);
            context.Respond(order);
            context.System.EventStream.Publish(new OrderCreated(order));
        }
        else if (context.Message is AllOrder)
        {
            context.Respond(_orders.ToList());
        }
        
        return Task.CompletedTask;
    }
}

public class PaymentActor : IActor
{
    private List<Order> _waitingForPayment = [];
    public Task ReceiveAsync(IContext context)
    {
        if (context.Message is OrderCreated orderCreated)
        {
            _waitingForPayment.Add(orderCreated.Order);
        }
        else if (context.Message is OrderPaid orderPaid)
        {
            _waitingForPayment.RemoveAll(x => x.Id == orderPaid.Id);
        }
        else if (context.Message is IsWaitingForPayment isWaitingForPayment)
        {
            context.Respond(_waitingForPayment.Any(x => x.Id == isWaitingForPayment.Id));
        }
        
        return Task.CompletedTask;
    }
}

public record Order(int Id, decimal Amount);
public record CreateOrder(int Id, decimal Amount);

public record AllOrder;
public record OrderCreated(Order Order);
public record OrderPaid(int Id);
public record IsWaitingForPayment(int Id);
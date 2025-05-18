using System.Net.Sockets;
using System.Text.Json;
using TcpServer.Client;

var id = 0;
while (true)
{
    Console.Write("Type a name (q to quit/f to non json): ");
    var name = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(name))
    {
        continue;
    }
    
    if (name == "q")
    {
        break;
    }

    try
    {
        var connection = new TcpClient();
        await connection.ConnectAsync("localhost", 9091);
    
        var stream = connection.GetStream();
        if (name == "f")
        {
            await stream.WriteAsync(new[] { (byte)'f'  });
        }
        else
        {
            await JsonSerializer.SerializeAsync(stream, new Sample
            {
                Id = id++,
                Name = name,
            });
        }
        connection.Close();
    }
    catch (Exception e)
    {
        Console.WriteLine("Error: {0}",e);
    }
}
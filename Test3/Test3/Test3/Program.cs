using System.Net;
using System.Net.Sockets;
using System.Text;

if (args.Length == 0)
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  Server mode: NetworkChat.exe <port>");
    Console.WriteLine("  Client mode: NetworkChat.exe <ip> <port>");
    Console.WriteLine("Example:");
    Console.WriteLine("  Server: NetworkChat.exe 8888");
    Console.WriteLine("  Client: NetworkChat.exe 127.0.0.1 8888");
    return;
}

try
{
    if (args.Length == 1 && int.TryParse(args[0], out int port))
    {
        await RunAsServer(port);
    }
    else if (args.Length == 2 && int.TryParse(args[1], out port))
    {
        await RunAsClient(args[0], port);
    }
    else
    {
        Console.WriteLine("Invalid arguments.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

static async Task RunAsServer(int port)
{
    Console.WriteLine($"Starting server on port {port}...");
            
    var listener = new TcpListener(IPAddress.Any, port);
    listener.Start();
    Console.WriteLine($"Server started. Waiting for connections...");

    try
    {
        using var client = await listener.AcceptTcpClientAsync();
        Console.WriteLine("Client connected!");
        await HandleConnection(client, "Server", "Client");
    }
    finally
    {
        listener.Stop();
        Console.WriteLine("Server stopped.");
    }
}

static async Task RunAsClient(string ip, int port)
{
    Console.WriteLine($"Connecting to {ip}:{port}...");
            
    using var client = new TcpClient();
    await client.ConnectAsync(ip, port);
    Console.WriteLine("Connected to server!");
    await HandleConnection(client, "Client", "Server");
            
    Console.WriteLine("Disconnected.");
}

static async Task HandleConnection(TcpClient client, string localName, string remoteName)
{
    var stream = client.GetStream();
    var reader = new StreamReader(stream, Encoding.UTF8);
    var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

    var readTask = Task.Run(async () =>
    {
        try
        {
            while (client.Connected)
            {
                var message = await reader.ReadLineAsync();
                if (message is null) break;
                        
                if (message.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"{remoteName} disconnected.");
                    break;
                }
                Console.WriteLine($"{remoteName}: {message}");
            }
        }
        catch (IOException)
        {
            Console.WriteLine($"{remoteName} disconnected.");
        }
    });

    try
    {
        while (client.Connected && !readTask.IsCompleted)
        {
            var input = Console.ReadLine();
            if (string.IsNullOrEmpty(input)) continue;
                    
            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                await writer.WriteLineAsync("exit");
                break;
            }
                    
            await writer.WriteLineAsync(input);
        }
    }
    finally
    {
        await Task.WhenAny(readTask, Task.Delay(1000));
        client.Close();
    }
}
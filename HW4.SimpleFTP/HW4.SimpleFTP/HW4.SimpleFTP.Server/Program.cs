using System.Net;
using System.Net.Sockets;
using System.Text;

var listener = new TcpListener(IPAddress.Any, 8888);
listener.Start();
Console.WriteLine("FTP Server started on port 8888. Press Ctrl+C to stop.");

var cancellationTokenSource = new CancellationTokenSource();
Console.CancelKeyPress += (_, _) =>
{
    Console.WriteLine("\nShutting down...");
    cancellationTokenSource.Cancel();
};

var tasks = new List<Task>();

try
{
    while (!cancellationTokenSource.Token.IsCancellationRequested)
    {
        try
        {
            TcpClient? client = await listener.AcceptTcpClientAsync(cancellationTokenSource.Token);
            var task = Task.Run(() => ProcessClientAsync(client, cancellationTokenSource.Token), cancellationTokenSource.Token);
            tasks.Add(task);
        }
        catch (OperationCanceledException)
        {
            break;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Accept error: {ex.Message}");
        }
    }

    await Task.WhenAll(tasks.Where(t => !t.IsCompleted).ToArray());
}
finally
{
    listener.Stop();
}
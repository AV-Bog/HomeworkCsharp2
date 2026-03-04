// <copyright file="FtpClient.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

using System.Net.Sockets;
using System.Text;

namespace SimpleFtpClient;

public static class FtpClient
{
    private const string Host = "localhost";
    private const int Port = 8888;

    public static async Task ListAsync(string path)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(Host, Port);
        using var stream = client.GetStream();
        using var writer = new StreamWriter(stream) { NewLine = "\n" };
        using var reader = new StreamReader(stream);

        await writer.WriteLineAsync($"1 {path}");
        await writer.FlushAsync();

        string? response = await reader.ReadLineAsync();
        if (response == null)
        {
            Console.WriteLine("Empty response from server");
            return;
        }

        var parts = response.Split(' ');
        if (!int.TryParse(parts[0], out int count))
        {
            Console.WriteLine("Invalid response format");
            return;
        }

        if (count == -1)
        {
            Console.WriteLine("Directory not found");
            return;
        }

        if (parts.Length != 1 + count * 2)
        {
            Console.WriteLine("Response length mismatch");
            return;
        }

        Console.WriteLine($"{count} items:");
        for (int i = 1; i < parts.Length; i += 2)
        {
            string name = parts[i];
            bool isDir = parts[i + 1] == "true";
            Console.WriteLine($"  {name} - {(isDir ? "Directory" : "File")}");
        }
    }

    public static async Task GetAsync(string remotePath, string localPath)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(Host, Port);
        using var stream = client.GetStream();
        using var writer = new StreamWriter(stream) { NewLine = "\n" };

        await writer.WriteLineAsync($"2 {remotePath}");
        await writer.FlushAsync();

        byte[] headerBuffer = new byte[64];
        int bytesRead = await stream.ReadAsync(headerBuffer, 0, headerBuffer.Length);
        if (bytesRead == 0)
        {
            Console.WriteLine("No data received");
            return;
        }

        string header = Encoding.UTF8.GetString(headerBuffer, 0, bytesRead);
        int spaceIndex = header.IndexOf(' ');
        if (spaceIndex == -1)
        {
            Console.WriteLine("Malformed response: no space between size and content");
            return;
        }

        if (!long.TryParse(header.Substring(0, spaceIndex), out long fileSize))
        {
            Console.WriteLine("Invalid file size");
            return;
        }

        if (fileSize == -1)
        {
            Console.WriteLine("File not found on server");
            return;
        }

        using var fileStream = File.Create(localPath);

        int remainingBytes = bytesRead - spaceIndex - 1;
        if (remainingBytes > 0)
        {
            await fileStream.WriteAsync(headerBuffer.AsMemory(spaceIndex + 1, remainingBytes));
        }

        long totalRead = Math.Max(0, remainingBytes);
        byte[] buffer = new byte[8192];
        while (totalRead < fileSize)
        {
            int toRead = (int)Math.Min(buffer.Length, fileSize - totalRead);
            int n = await stream.ReadAsync(buffer, 0, toRead);
            if (n == 0) break;
            await fileStream.WriteAsync(buffer.AsMemory(0, n));
            totalRead += n;
        }

        Console.WriteLine($"Downloaded {totalRead} bytes to {localPath}");
    }
}
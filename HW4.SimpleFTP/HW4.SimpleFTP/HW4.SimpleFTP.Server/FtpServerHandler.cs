// <copyright file="FtpServerHandler.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

using System.Net.Sockets;
using System.Text;

namespace SimpleFtpServer;

public static class FtpServerHandler
{
    public static async Task ProcessClientAsync(TcpClient client, CancellationToken ct)
    {
        using (client)
        using (var stream = client.GetStream())
        {
            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, ct);
            if (bytesRead == 0) return;

            string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            int newlinePos = request.IndexOf('\n');
            if (newlinePos < 0)
            {
                await SendErrorResponse(stream);
                return;
            }

            string line = request[..newlinePos].Trim();
            string[] parts = line.Split(' ', 2);
            if (parts.Length != 2)
            {
                await SendErrorResponse(stream);
                return;
            }

            string command = parts[0];
            string path = parts[1];

            switch (command)
            {
                case "1":
                    await HandleListAsync(path, stream);
                    break;
                case "2":
                    await HandleGetAsync(path, stream);
                    break;
                default:
                    await SendErrorResponse(stream);
                    break;
            }
        }
    }

    private static async Task HandleListAsync(string path, NetworkStream stream)
    {
        if (!Directory.Exists(path))
        {
            await SendErrorResponse(stream);
            return;
        }

        try
        {
            var entries = Directory.GetFileSystemEntries(path);
            var sb = new StringBuilder();
            sb.Append(entries.Length);

            foreach (string entry in entries)
            {
                string name = Path.GetFileName(entry);
                bool isDir = Directory.Exists(entry);
                sb.Append(' ').Append(name).Append(' ').Append(isDir ? "true" : "false");
            }

            byte[] response = Encoding.UTF8.GetBytes(sb.ToString() + "\n");
            await stream.WriteAsync(response);
        }
        catch
        {
            await SendErrorResponse(stream);
        }
    }

    private static async Task HandleGetAsync(string path, NetworkStream stream)
    {
        if (!File.Exists(path))
        {
            await SendErrorResponse(stream);
            return;
        }

        try
        {
            FileInfo fi = new(path);
            long size = fi.Length;

            string header = $"{size} ";
            byte[] headerBytes = Encoding.UTF8.GetBytes(header);
            await stream.WriteAsync(headerBytes);

            using var fileStream = File.OpenRead(path);
            await fileStream.CopyToAsync(stream);
        }
        catch
        {
            await SendErrorResponse(stream);
        }
    }

    private static async Task SendErrorResponse(NetworkStream stream)
    {
        byte[] error = Encoding.UTF8.GetBytes("-1\n");
        await stream.WriteAsync(error);
    }
}
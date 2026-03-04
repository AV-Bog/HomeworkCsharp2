// <copyright file="Program.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

using System.Net.Sockets;
using SimpleFtpClient;

if (args.Length < 2)
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  list <path>                     – List directory contents");
    Console.WriteLine("  get <remotePath> <localPath>    – Download file");
    return;
}

var command = args[0].ToLowerInvariant();
try
{
    switch (command)
    {
        case "list":
            if (args.Length != 2)
                throw new ArgumentException("Expected exactly one argument for 'list'");
            await FtpClient.ListAsync(args[1]);
            break;

        case "get":
            if (args.Length != 3)
                throw new ArgumentException("Expected two arguments for 'get': remotePath localPath");
            await FtpClient.GetAsync(args[1], args[2]);
            break;

        default:
            Console.WriteLine($"Unknown command: {command}");
            break;
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    Environment.Exit(1);
}
   

        for (int i = 1; i < parts.Length; i += 2)
        {
            var name = parts[i];
            var isDirStr = parts[i + 1];
            var type = isDirStr == "true" ? "Директория" : "Файл";

            Console.WriteLine($"  {name} - {type}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка при выполнении команды List: {ex.Message}");
    }
}
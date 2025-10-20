// See https://aka.ms/new-console-template for more information

using System.Net.Sockets;
using HW4.SimpleFTP;

TcpListener listener = TcpListener.Create(8888);
listener.Start();
while (true)
{
    try
    {
        TcpClient client = await listener.AcceptTcpClientAsync();
        _ = Task.Run(async () => await FTP.ЧТЕНИЕ_КОМАНДЫ(client));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка при принятии подключения: {ex.Message}");
    }
}

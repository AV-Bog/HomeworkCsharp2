// <copyright file="FTP.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

using System.Net.Sockets;

namespace HW4.SimpleFTP;

public class FTP
{
    public static async Task ЧТЕНИЕ_КОМАНДЫ(TcpClient  client)
    {
        using (client)
        using (StreamReader reader = new StreamReader(client.GetStream()))
        using (StreamWriter writer = new StreamWriter(client.GetStream()))
        {
            string content = await reader.ReadLineAsync();
            var content2 = content.Split(' ');
            var codComande = content2[0];
            var pathComande = content2[1];
            if (codComande == "1")
                await ОбработкаЗапросаДля1(pathComande, writer);
            if (codComande == "2")
                await ОбработкаЗапросаДля2(pathComande,  writer);
            else
            {
                //отправить клиенту -1 и пусть гуляет
            }
        }
        
    }

    private static async void ОбработкаЗапросаДля1(string pathComande, StreamWriter writer)
    {
        
    }
    
    private static async void ОбработкаЗапросаДля2(string pathComande, StreamWriter writer)
    {
        
    }
}
// <copyright file="FTP.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

using System.Net.Sockets;
using System.Text;

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
        }
    }

    private static async Task ОбработкаЗапросаДля1(string pathComande, StreamWriter writer) //List
    {
        if (!Directory.Exists(pathComande))
        {
            await writer.WriteLineAsync("-1"); // Директория не существует
            return;
        }
         //size (<name: String> <isDir: Boolean>)*\n
         try
         {
             var allEntries = Directory.GetFileSystemEntries(pathComande);
             var size = allEntries.Count();

             StringBuilder ansver = new StringBuilder();
             ansver.Append(size);

             foreach (var entry in allEntries)
             {
                 bool isDir = Directory.Exists(entry); //но там файл/дир/ничего поэтому так мб нельзя
                 var name = Path.GetFileName(entry);
                 ansver.AppendFormat(" (<name: {1}> <isDir: {2})>\n", name, isDir);
             }
             await writer.WriteAsync(ansver.ToString());
         }
         catch (Exception ex)
         {
             await writer.WriteLineAsync("-1");
         }
    }
    
    private static async void ОбработкаЗапросаДля2(string pathComande, StreamWriter writer) //Get
    {
        
    }
}
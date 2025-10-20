using System.Net.Sockets;
using System.Text;

TcpListener listener = TcpListener.Create(8888);
listener.Start();

while (true)
{
    try
    {
        TcpClient client = await listener.AcceptTcpClientAsync();
        _ = Task.Run(async () => await ProcessClientRequest(client));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка при принятии подключения: {ex.Message}");
    }
}

// <summary>
// Обрабатывает запрос от клиента: читает команду и перенаправляет на соответствующий обработчик
// </summary>
// <param name="client">TCP-клиент для взаимодействия</param>
static async Task ProcessClientRequest(TcpClient  client)
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
            await HandleDirectoryList(pathComande, writer);
        if (codComande == "2")
            await HandleFileGet(pathComande, writer, client.GetStream());
    }
}

// <summary>
// Обрабатывает команду List (код 1)
// Возвращает список файлов и папок в указанной директории
// </summary>
// <param name="requestPath">Путь к директории для сканирования</param>
// <param name="writer">Писатель для отправки ответа клиенту</param>
static async Task HandleDirectoryList(string pathComande, StreamWriter writer)
{
    if (!Directory.Exists(pathComande))
    {
        await writer.WriteLineAsync("-1");
        return;
    }
        
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
            ansver.Append($" {name} {isDir}\n");
        }
        await writer.WriteAsync(ansver.ToString());
    }
    catch (Exception ex)
    {
        await writer.WriteLineAsync("-1");
    }
}

// <summary>
// Обрабатывает команду Get (код 2)
// Отправляет размер файла, затем его содержимое
// </summary>
// <param name="requestPath">Путь к файлу для загрузки</param>
// <param name="writer">Писатель для отправки метаданных</param>
// <param name="stream">Сетевой поток для отправки содержимого файла</param>
static async Task HandleFileGet(string pathComande, StreamWriter writer, NetworkStream stream)
{
    if (!File.Exists(pathComande))
    {
        await writer.WriteLineAsync("-1");
        return;
    }

    try
    {
        var fileInfo = new FileInfo(pathComande);
        var size = fileInfo.Length;
            
        await writer.WriteLineAsync($"{size}");
        await writer.FlushAsync();
            
        using (FileStream fileStream = File.OpenRead(pathComande))
        {
            await fileStream.CopyToAsync(stream);
        }
    }
    catch (Exception ex)
    {
        await writer.WriteLineAsync("-1");
    }
}
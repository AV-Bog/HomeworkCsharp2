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
static async Task ProcessClientRequest(TcpClient client)
{
    using (client)
    using (StreamReader reader = new StreamReader(client.GetStream()))
    using (StreamWriter writer = new StreamWriter(client.GetStream()))
    {
        string content = await reader.ReadLineAsync();

        if (content == null)
        {
            return;
        }

        var content2 = content.Split(' ', 2);
        if (content2.Length != 2)
        {
            await writer.WriteLineAsync("-1");
            return;
        }

        var codComande = content2[0];
        var pathComande = content2[1];

        switch (codComande)
        {
            case "1": await HandleDirectoryList(pathComande, writer);
                break;
            case "2": await HandleFileGet(pathComande, writer, client.GetStream());
                break;
            default:
                await writer.WriteLineAsync("-1");
                break;
        }
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

        StringBuilder ansver = new StringBuilder();
        ansver.Append(allEntries.Count());

        foreach (var entry in allEntries)
        {
            bool isDir = Directory.Exists(entry);
            var name = Path.GetFileName(entry);

            ansver.Append($" {name} {(isDir ? "true" : "false")}");
        }

        ansver.AppendLine();

        await writer.WriteAsync(ansver.ToString());
        await writer.FlushAsync();
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
        try
        {
            await writer.WriteLineAsync("-1");
        }
        catch
        {
            // ignored
        }
    }
}
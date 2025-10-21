// <copyright file="Program.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

using System.Net.Sockets;

// <summary>
// Главная точка входа клиентского приложения: предоставляет интерфейс для взаимодействия с сервером
// </summary>
while (true)
{
    Console.WriteLine("Выберите команду:");
    Console.WriteLine("1 - List (получить список файлов)");
    Console.WriteLine("2 - Get (скачать файл)");
    Console.WriteLine("3 - Выход");
    Console.Write("Введите номер команды: ");

    var choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            await HandleListCommand();
            break;
        case "2":
            await HandleGetCommand();
            break;
        case "3":
            return;
        default:
            Console.WriteLine("Неизвестная команда");
            break;
    }

    Console.WriteLine();
}

// <summary>
// Обрабатывает команду List: формирует запрос к серверу, получает и отображает список файлов и директорий
// </summary>
static async Task HandleListCommand()
{
    try
    {
        Console.Write("Введите путь к директории: ");
        var path = Console.ReadLine();

        using TcpClient client = new TcpClient();
        await client.ConnectAsync("localhost", 8888);

        using NetworkStream stream = client.GetStream();
        using StreamReader reader = new StreamReader(stream);
        using StreamWriter writer = new StreamWriter(stream);

        var request = $"1 {path}";
        await writer.WriteLineAsync(request);
        await writer.FlushAsync();

        Console.WriteLine($"Отправлен запрос: {request}");

        var response = await reader.ReadLineAsync();
        Console.WriteLine($"Получен ответ: {response}");

        if (response == null)
        {
            Console.WriteLine("Пустой ответ от сервера");
            return;
        }

        var parts = response.Split(' ');

        if (parts.Length == 0)
        {
            Console.WriteLine("Ошибка неизвестного характера");
            return;
        }

        if (!int.TryParse(parts[0], out int size))
        {
            Console.WriteLine("Некорректный формат размера");
            return;
        }

        if (size == -1)
        {
            Console.WriteLine("Директория не существует");
            return;
        }

        if (parts.Length != size * 2 + 1)
        {
            Console.WriteLine("Несоответствие количества элементов в ответе");
            return;
        }

        Console.WriteLine($"Найдено {size} элементов:");

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

// <summary>
// Обрабатывает команду Get: формирует запрос на скачивание, получает файл с сервера и сохраняет его локально
// </summary>
static async Task HandleGetCommand()
{
    try
    {
        Console.Write("Введите путь к файлу: ");
        var filePath = Console.ReadLine();
        Console.Write("Введите имя для сохранения файла: ");
        var savePath = Console.ReadLine();

        using TcpClient client = new TcpClient();
        await client.ConnectAsync("localhost", 8888);

        using NetworkStream stream = client.GetStream();
        using StreamReader reader = new StreamReader(stream);
        using StreamWriter writer = new StreamWriter(stream);

        var request = $"2 {filePath}";
        await writer.WriteLineAsync(request);
        await writer.FlushAsync();

        Console.WriteLine($"Отправлен запрос: {request}");

        var sizeLine = await reader.ReadLineAsync();
        Console.WriteLine($"Получен размер файла: {sizeLine}");

        if (sizeLine == null)
        {
            Console.WriteLine("Пустой ответ от сервера");
            return;
        }

        if (!long.TryParse(sizeLine, out long fileSize))
        {
            Console.WriteLine("Некорректный формат размера файла");
            return;
        }

        if (fileSize == -1)
        {
            Console.WriteLine("Файл не существует на сервере");
            return;
        }

        Console.WriteLine($"Размер файла: {fileSize} байт");
        Console.WriteLine("Начинается загрузка файла...");

        using FileStream fileStream = File.Create(savePath);
        var buffer = new byte[4096];
        long totalRead = 0;

        while (totalRead < fileSize)
        {
            var bytesToRead = (int)Math.Min(buffer.Length, fileSize - totalRead);
            var bytesRead = await stream.ReadAsync(buffer, 0, bytesToRead);

            if (bytesRead == 0)
            {
                Console.WriteLine("Соединение прервано до завершения загрузки");
                break;
            }

            await fileStream.WriteAsync(buffer, 0, bytesRead);
            totalRead += bytesRead;

            var progress = (double)totalRead / fileSize * 100;
            Console.WriteLine($"Загружено: {totalRead}/{fileSize} байт ({progress:F1}%)");
        }

        Console.WriteLine($"Файл успешно сохранен как: {savePath}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка при выполнении команды Get: {ex.Message}");
    }
}

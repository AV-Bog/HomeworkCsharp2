Console.WriteLine("Введите путь: ");
var path = Console.ReadLine();
if (File.Exists(path))
{
    var extension = Path.GetExtension(path);
    if (extension == ".exe" || extension == ".dll")
    {
        MyNUnit.Testing(path);
    }
    else
    {
        Console.WriteLine("нет там сборки");
    }
}
else if (Directory.Exists(path))
{
    foreach (var file in Directory.GetFiles(path))
    {
        var extension = Path.GetExtension(file);
        if (extension == ".exe" || extension == ".dll")
        {
            MyNUnit.Testing(path);
        }
    }
}
else
{
    Console.WriteLine("Ну чумаааа");
}
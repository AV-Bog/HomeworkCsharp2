// <copyright file="CheckSum.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

using System.Text;

namespace CheckSum;

public class CheckSum
{
    public string CheckSumForFile(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        
        return File.ReadAllText(filePath);
    }

    public string CheckSumForDirectory(string filePath)
    {
        byte[] nameBytes = Encoding.UTF8.GetBytes(Directory.DirectoryInfo(filePath).Name);
        var entries = Directory.EnumerateFileSystemEntries(filePath).OrderBy(x => x);
        using var stream = File.OpenRead(filePath);
    }
}
// <copyright file="Program.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

using System.Diagnostics;
using CheckSum;

Console.WriteLine("Directory Checksum Calculator");
Console.WriteLine("=============================");

Console.Write("Enter directory path: ");
string directoryPath = Console.ReadLine();

if (!Directory.Exists(directoryPath))
{
    Console.WriteLine("Error: Directory does not exist!");
    return;
}

var singleThreadedHasher = new SingleThreadedAsyncDirectoryHasher();
var multiThreadedHasher = new MultiThreadedAsyncDirectoryHasher();

Console.WriteLine("\n1. Single-threaded ASYNC calculation:");
var stopwatch = Stopwatch.StartNew();
byte[] singleThreadedHash = await singleThreadedHasher.ComputeDirectoryHashAsync(directoryPath);
stopwatch.Stop();
Console.WriteLine($"Time: {stopwatch.ElapsedMilliseconds} ms");
Console.WriteLine($"Hash: {HashUtils.BytesToHex(singleThreadedHash)}");

Console.WriteLine("\n2. Multi-threaded ASYNC calculation:");
stopwatch.Restart();
byte[] multiThreadedHash = await multiThreadedHasher.ComputeDirectoryHashAsync(directoryPath);
stopwatch.Stop();
Console.WriteLine($"Time: {stopwatch.ElapsedMilliseconds} ms");
Console.WriteLine($"Hash: {HashUtils.BytesToHex(multiThreadedHash)}");

Console.WriteLine("\n3. Comparison:");
bool hashesMatch = CompareByteArrays(singleThreadedHash, multiThreadedHash);
Console.WriteLine($"Hashes match: {hashesMatch}");

if (hashesMatch)
{
    Console.WriteLine("✓ Both implementations produce the same result!");
}
else
{
    Console.WriteLine("✗ Implementations produce different results!");
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();

static bool CompareByteArrays(byte[] array1, byte[] array2)
{
    if (array1.Length != array2.Length)
    {
        return false;
    }

    for (int i = 0; i < array1.Length; i++)
    {
        if (array1[i] != array2[i])
        {
            return false;
        }
    }

    return true;
}
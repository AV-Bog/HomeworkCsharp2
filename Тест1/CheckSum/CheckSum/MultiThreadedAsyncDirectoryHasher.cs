// <copyright file="MultiThreadedAsyncDirectoryHasher.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

using System.Collections.Concurrent;
using System.Text;

namespace CheckSum;

public class MultiThreadedAsyncDirectoryHasher : IAsyncDirectoryHasher
{
    /// <summary>
    /// Асинхронное вычисление хеша файла.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    private async Task<byte[]> ComputeFileHashAsync(string filePath, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var fileName = Path.GetFileName(filePath);
        byte[] nameBytes = Encoding.UTF8.GetBytes(fileName);

        byte[] contentBytes;
        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous))
        {
            contentBytes = new byte[fileStream.Length];
            int bytesRead = await fileStream.ReadAsync(contentBytes, 0, (int)fileStream.Length, cancellationToken);

            if (bytesRead != fileStream.Length)
            {
                throw new IOException($"Не удалось прочитать весь файл: {filePath}");
            }
        }

        cancellationToken.ThrowIfCancellationRequested();

        byte[] combinedBytes = new byte[nameBytes.Length + contentBytes.Length];
        Buffer.BlockCopy(nameBytes, 0, combinedBytes, 0, nameBytes.Length);
        Buffer.BlockCopy(contentBytes, 0, combinedBytes, nameBytes.Length, contentBytes.Length);

        return await Task.Run(
            () => 
        {
            cancellationToken.ThrowIfCancellationRequested();
            return HashUtils.ComputeMD5(combinedBytes);
        }, cancellationToken);
    }

    /// <summary>
    /// Асинхронное вычисление хеша директории.
    /// </summary>
    /// <param name="directoryPath"></param>
    /// <returns></returns>
    public async Task<byte[]> ComputeDirectoryHashAsync(string directoryPath,  CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!Directory.Exists(directoryPath))
        {
            throw new DirectoryNotFoundException($"Директория не найдена: {directoryPath}");
        }

        var directoryName = Path.GetFileName(directoryPath.TrimEnd(Path.DirectorySeparatorChar));

        var files = Directory.GetFiles(directoryPath)
            .OrderBy(f => f, StringComparer.Ordinal)
            .ToArray();

        var subdirectories = Directory.GetDirectories(directoryPath)
            .OrderBy(d => d, StringComparer.Ordinal)
            .ToArray();

        var allHashes = new ConcurrentBag<byte[]>();

        var subdirectoryTasks = subdirectories.Select(async subdirectory =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            var subdirectoryHash = await ComputeDirectoryHashAsync(subdirectory, cancellationToken);
            allHashes.Add(subdirectoryHash);
        }).ToArray();

        await Task.WhenAll(subdirectoryTasks);

        var fileTasks = files.Select(async file =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            var fileHash = await ComputeFileHashAsync(file);
            allHashes.Add(fileHash);
        }).ToArray();

        await Task.WhenAll(fileTasks);

        cancellationToken.ThrowIfCancellationRequested();
        
        return ComputeFinalDirectoryHash(directoryName, allHashes.ToList());
    }

    /// <summary>
    /// очев
    /// </summary>
    /// <param name="directoryName"></param>
    /// <param name="contentHashes"></param>
    /// <returns></returns>
    private byte[] ComputeFinalDirectoryHash(string directoryName, List<byte[]> contentHashes)
    {
        byte[] nameBytes = Encoding.UTF8.GetBytes(directoryName);

        int totalSize = nameBytes.Length;
        foreach (var hash in contentHashes)
        {
            totalSize += hash.Length;
        }

        byte[] combinedBytes = new byte[totalSize];
        int currentPosition = 0;

        Buffer.BlockCopy(nameBytes, 0, combinedBytes, currentPosition, nameBytes.Length);
        currentPosition += nameBytes.Length;

        var sortedHashes = contentHashes
            .OrderBy(h => BitConverter.ToString(h), StringComparer.Ordinal)
            .ToList();

        foreach (var hash in sortedHashes)
        {
            Buffer.BlockCopy(hash, 0, combinedBytes, currentPosition, hash.Length);
            currentPosition += hash.Length;
        }

        return HashUtils.ComputeMD5(combinedBytes);
    }
}
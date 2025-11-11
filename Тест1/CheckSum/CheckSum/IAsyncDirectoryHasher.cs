// <copyright file="IAsyncDirectoryHasher.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

namespace CheckSum;

public interface IAsyncDirectoryHasher
{
    Task<byte[]> ComputeDirectoryHashAsync(string directoryPath);
}
// <copyright file="HashUtils.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

namespace CheckSum;

using System.Security.Cryptography;

/// <summary>
/// Класс для вычисления MD5 хешей.
/// </summary>
public class HashUtils
{
    /// <summary>
    /// Вычисляет MD5 хеш для массива байтов.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static byte[] ComputeMD5(byte[] data)
    {
        using (var md5 = MD5.Create())
        {
            return md5.ComputeHash(data);
        }
    }

    /// <summary>
    /// Конвертит массив байтов в hex-строку.
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static string BytesToHex(byte[] bytes)
    {
        return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
    }
}
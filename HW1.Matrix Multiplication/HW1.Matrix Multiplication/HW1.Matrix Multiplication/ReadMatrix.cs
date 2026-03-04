// <copyright file="ReadMatrix.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

namespace HW1.Matrix_Multiplication;

public class ReadMatrix
{
    public static int[][] ReadMatrixInFile(string filePath)
    {
        return File.ReadLines(filePath)
            .Select(line => line.Split(' ')
                .Select(x => int.Parse(x))
                .ToArray())
            .ToArray();
    }
}
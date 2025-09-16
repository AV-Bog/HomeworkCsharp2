// <copyright file="Testing.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

using System.Diagnostics;

namespace HW1.Matrix_Multiplication;

public class Testing
{
    static public double[][] Comparison(int N)
    {
        var result = new double[5][];
        int[] razmer = { 100, 1000, 10000, 100000, 1000000 };

        for (int i = 0; i < 5; i++)
        {
            result[i] = ГенерацияТестовыхДанныхДебильныйАнгл(razmer[i], N);
        }

        return result;
    }

    static private double[] ГенерацияТестовыхДанныхДебильныйАнгл(int n, int N)
    {
        GenerateMatrixFile(n, n, 1, 1000, "matrixA");
        GenerateMatrixFile(n, n, 1, 1000, "matrixB");

        var matrixA = ReadMatrix.ReadMatrixInFile("matrixA");
        var matrixB = ReadMatrix.ReadMatrixInFile("matrixB");

        var timeResults = new double[N + 1];

        var stopwatch = Stopwatch.StartNew();
        MatrixMultiplier.MultiplyMatrixNonParallel(matrixA, matrixB);
        stopwatch.Stop();
        timeResults[0] = stopwatch.Elapsed.TotalMilliseconds;

        for (var i = 0; i < N; i++)
        {
            stopwatch.Restart();
            MatrixMultiplier.MultiplyMatrixParallel(matrixA, matrixB);
            stopwatch.Stop();
            timeResults[i + 1] = stopwatch.Elapsed.TotalMilliseconds;
        }

        File.Delete("matrixA");
        File.Delete("matrixB");

        return timeResults;
    }

    static void GenerateMatrixFile(int rows, int cols, int minValue, int maxValue, string filename)
    {
        Random random = new Random();

        using (StreamWriter writer = new StreamWriter(filename))
        {
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    int value = random.Next(minValue, maxValue + 1);
                    writer.Write(value);

                    if (j < cols - 1)
                    {
                        writer.Write(" ");
                    }
                }

                writer.WriteLine();
            }
        }
    }
}
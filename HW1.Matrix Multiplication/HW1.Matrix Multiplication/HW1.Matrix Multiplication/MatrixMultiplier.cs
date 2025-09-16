// <copyright file="MatrixMultiplier.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

namespace HW1.Matrix_Multiplication;

public class MatrixMultiplier
{
    public static int[][] MultiplyMatrixParallel(int[][] matrix1, int[][] matrix2)
    {
        if (matrix1[0].Length != matrix2.Length)
        {
            throw new ArgumentException("Число столбцов первой матрицы не совпадает с числом строк второй матрицы");
        }

        int rowsA = matrix1.Length;
        int colsB = matrix2[0].Length;

        var result = new int[rowsA][];
        for (int i = 0; i < rowsA; i++)
        {
            result[i] = new int[colsB];
        }

        var treadCount = Environment.ProcessorCount;
        var threads = new Thread[treadCount];

        var rowsThreads = rowsA / treadCount;
        var remainingRows = rowsA % treadCount;

        var startCountRow = 0;

        for (int i = 0; i < treadCount; i++)
        {
            var start = startCountRow;
            var end = start + rowsThreads - 1;

            if (i < remainingRows)
            {
                end++;
            }

            threads[i] = new Thread(() =>
            {
                MultiplyMatrixRange(matrix1, matrix2, result, start, end);
            });

            threads[i].Start();
            startCountRow = end + 1;
        }

        foreach (Thread thread in threads)
        {
            thread.Join();
        }

        return result;
    }

    private static void MultiplyMatrixRange(int[][] matrixA, int[][] matrixB, int[][] result, int startRow, int endRow)
    {
        int colsA = matrixA[0].Length;
        int colsB = matrixB[0].Length;

        for (int i = startRow; i <= endRow; i++)
        {
            for (int j = 0; j < colsB; j++)
            {
                int sum = 0;
                for (int k = 0; k < colsA; k++)
                {
                    sum += matrixA[i][k] * matrixB[k][j];
                }

                result[i][j] = sum;
            }
        }
    }

    public static int[][] MultiplyMatrixNonParallel(int[][] matrixA, int[][] matrixB)
    {
        if (matrixA[0].Length != matrixB.Length)
        {
            throw new ArgumentException("Число столбцов первой матрицы не совпадает с числом строк второй матрицы");
        }

        var rowsA = matrixA.Length;
        var colsA = matrixA[0].Length;
        var colsB = matrixB[0].Length;

        var result = new int[rowsA][];
        for (int i = 0; i < rowsA; i++)
        {
            result[i] = new int[colsB];
            for (int j = 0; j < colsB; j++)
            {
                int sum = 0;
                for (int k = 0; k < colsA; k++)
                {
                    sum += matrixA[i][k] * matrixB[k][j];
                }

                result[i][j] = sum;
            }
        }

        return result;
    }
}
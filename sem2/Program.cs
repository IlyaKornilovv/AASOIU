using System;

// ============================================================
// ТЕСТОВЫЕ МАТРИЦЫ
// ============================================================

//int[,] matrix = {
//    { 1, 0, 0, 0, 0, 1 },
//    { 0, 2, 0, 0, 0, 0 },
//    { 3, 0, 0, 0, 0, 0 },
//    { 0, 0, 0, 0, 0, 4 },
//    { 0, 0, 5, 0, 0, 0 }
//};

//int[,] matrix = {
//    { 0, 1, 0, 2, 0, 0 },
//    { 0, 0, 0, 0, 0, 3 },
//    { 4, 0, 5, 0, 0, 0 },
//    { 0, 0, 0, 6, 0, 0 }
//};

int[,] matrix = {
    { 8, 0, 2, 0, 0 },
    { 0, 0, 5, 0, 0 },
    { 0, 0, 0, 0, 0 },
    { 0, 0, 0, 0, 0 },
    { 0, 0, 7, 1, 2 },
    { 0, 0, 0, 0, 0 },
    { 0, 0, 0, 9, 0 }
};

// ============================================================
// COO
// ============================================================

PrintSeparator("COO");

DenseToCOO(matrix, out int[] row, out int[] col, out int[] data);
PrintArray("Row", row);
PrintArray("Col", col);
PrintArray("Data", data);

var restoredCOO = COOToDense(row, col, data, matrix.GetLength(0), matrix.GetLength(1));
PrintMatrix(restoredCOO);

Console.WriteLine($"COO Effective: {isCOOEffective(matrix)}");

// ============================================================
// LIL
// ============================================================

PrintSeparator("LIL");

DenseToLIL(matrix, out int[][] rowsLIL, out int[][] dataLIL);
PrintJagged("Rows", rowsLIL);
PrintJagged("Data", dataLIL);

var restoredLIL = LILToDense(rowsLIL, dataLIL, matrix.GetLength(0), matrix.GetLength(1));
PrintMatrix(restoredLIL);

Console.WriteLine($"LIL Effective: {isLILEffective(matrix)}");

// ============================================================
// CSR
// ============================================================

PrintSeparator("CSR");

DenseToCSR(matrix, out int[] d, out int[] ind, out int[] ip);
PrintArray("Data", d);
PrintArray("Indices", ind);
PrintArray("IP", ip);

var restoredCSR = CSRToDense(d, ind, ip, matrix.GetLength(0), matrix.GetLength(1));
PrintMatrix(restoredCSR);

Console.WriteLine($"CSR Effective: {isCSREffective(matrix)}");

// ============================================================
// COO Функции
// ============================================================

static void DenseToCOO(int[,] matrix, out int[] row, out int[] col, out int[] data)
{
    int k = CountNonZero(matrix);
    row = new int[k];
    col = new int[k];
    data = new int[k];

    int idx = 0;
    for (int i = 0; i < matrix.GetLength(0); i++)
        for (int j = 0; j < matrix.GetLength(1); j++)
            if (matrix[i, j] != 0)
            {
                row[idx] = i;
                col[idx] = j;
                data[idx] = matrix[i, j];
                idx++;
            }
}

static int[,] COOToDense(int[] row, int[] col, int[] data, int n, int m)
{
    int[,] matrix = new int[n, m];
    for (int i = 0; i < data.Length; i++)
        matrix[row[i], col[i]] = data[i];
    return matrix;
}

static bool isCOOEffective(int[,] matrix)
{
    int n = matrix.GetLength(0);
    int m = matrix.GetLength(1);
    int k = CountNonZero(matrix);
    return k * 3 < n * m;
}

// ============================================================
// LIL Функции
// ============================================================

static void DenseToLIL(int[,] matrix, out int[][] rows, out int[][] data)
{
    int n = matrix.GetLength(0);
    int m = matrix.GetLength(1);

    rows = new int[n][];
    data = new int[n][];

    for (int i = 0; i < n; i++)
    {
        int count = 0;
        for (int j = 0; j < m; j++)
            if (matrix[i, j] != 0) count++;

        rows[i] = new int[count];
        data[i] = new int[count];

        int idx = 0;
        for (int j = 0; j < m; j++)
            if (matrix[i, j] != 0)
            {
                rows[i][idx] = j;
                data[i][idx] = matrix[i, j];
                idx++;
            }
    }
}

static int[,] LILToDense(int[][] rows, int[][] data, int n, int m)
{
    int[,] matrix = new int[n, m];
    for (int i = 0; i < n; i++)
        for (int k = 0; k < rows[i].Length; k++)
            matrix[i, rows[i][k]] = data[i][k];
    return matrix;
}

static bool isLILEffective(int[,] matrix)
{
    int n = matrix.GetLength(0);
    int m = matrix.GetLength(1);
    int k = CountNonZero(matrix);
    return k * 2 < n * m;
}

// ============================================================
// CSR Функции
// ============================================================

static void DenseToCSR(int[,] matrix, out int[] data, out int[] indices, out int[] ip)
{
    int n = matrix.GetLength(0);
    int m = matrix.GetLength(1);
    int k = CountNonZero(matrix);

    data = new int[k];
    indices = new int[k];
    ip = new int[n + 1];

    int idx = 0;
    for (int i = 0; i < n; i++)
    {
        ip[i] = idx;
        for (int j = 0; j < m; j++)
            if (matrix[i, j] != 0)
            {
                data[idx] = matrix[i, j];
                indices[idx] = j;
                idx++;
            }
    }
    ip[n] = k;
}

static int[,] CSRToDense(int[] data, int[] indices, int[] ip, int n, int m)
{
    int[,] matrix = new int[n, m];
    for (int i = 0; i < n; i++)
        for (int k = ip[i]; k < ip[i + 1]; k++)
            matrix[i, indices[k]] = data[k];
    return matrix;
}

static bool isCSREffective(int[,] matrix)
{
    int n = matrix.GetLength(0);
    int m = matrix.GetLength(1);
    int k = CountNonZero(matrix);
    return 2 * k + n + 1 < n * m;
}

// ============================================================
// ВСПОМОГАТЕЛЬНЫЕ ФУНКЦИИ
// ============================================================

static void PrintSeparator(string title)
{
    Console.WriteLine($"\n==== {title} ====");
}

static void PrintMatrix(int[,] matrix)
{
    for (int i = 0; i < matrix.GetLength(0); i++)
    {
        for (int j = 0; j < matrix.GetLength(1); j++)
            Console.Write($"{matrix[i, j],3}");
        Console.WriteLine();
    }
}

static void PrintArray(string name, int[] arr)
{
    Console.Write($"{name}: ");
    foreach (var x in arr) Console.Write(x + " ");
    Console.WriteLine();
}

static void PrintJagged(string name, int[][] arr)
{
    Console.WriteLine($"{name}:");
    for (int i = 0; i < arr.Length; i++)
    {
        Console.Write($"[{i}]: ");
        foreach (var x in arr[i]) Console.Write(x + " ");
        Console.WriteLine();
    }
}

static int CountNonZero(int[,] matrix)
{
    int count = 0;
    foreach (var x in matrix)
        if (x != 0) count++;
    return count;
}
using System;


Console.WriteLine("=== АНАЛИЗ КВАЛИФИКАЦИИ ГРАН-ПРИ ===\n");

// Ввод количества участников
int n = ReadInt("Введите количество участников: ");
Console.WriteLine();

// Массивы
string[] teams = new string[n];
double[] speeds = new double[n];

// Ввод данных
InputData(teams, speeds, n);

// Вычисление статистики
Console.WriteLine("---- СТАТИСТИКА ----");
CalculateStatistics(teams, speeds, n);
Console.WriteLine();

// Вывод исходного порядка
Console.WriteLine("---- ИСХОДНЫЙ ПОРЯДОК ----");
PrintTable(teams, speeds, n, false);
Console.WriteLine();

// Копирование массивов для сортировки
string[] sortedTeams = (string[])teams.Clone();
double[] sortedSpeeds = (double[])speeds.Clone();

// Сортировка
BubbleSort(sortedTeams, sortedSpeeds, n);

// Вывод отсортированного протокола
Console.WriteLine("---- ИТОГОВЫЙ ПРОТОКОЛ ----");
PrintTable(sortedTeams, sortedSpeeds, n, true);
Console.WriteLine();

// Фильтр по скорости
FilterBySpeed(sortedTeams, sortedSpeeds, n);






/* Ввод данных о командах и скоростях */
static void InputData(string[] teams, double[] speeds, int n)
{
    for (int i = 0; i < n; i++)
    {
        Console.WriteLine($"Участник #{i + 1}");

        Console.WriteLine("Команда: ");
        teams[i] = Console.ReadLine();

        speeds[i] = ReadDouble("Средняя скорость (км/ч): ");
        Console.WriteLine();
    }
}

/* Вычисление и вывод статистики */
static void CalculateStatistics(string[] teams, double[] speeds, int n)
{
    double sum = 0;
    double max = speeds[0], min = speeds[0];
    string fastest = teams[0], slowest = teams[0];

    for (int i = 0; i < n; i++)
    {
        sum += speeds[i];
        if (speeds[i] > max)
        {
            max = speeds[i];
            fastest = teams[i];
        }
        if (speeds[i] < min)
        {
            min = speeds[i];
            slowest = teams[i];
        }
    }
    double average = sum / n;

    Console.WriteLine($"Средняя скорость: {average:F2} км/ч");
    Console.WriteLine($"Лидер: {fastest} ({max:F2} км/ч)");
    Console.WriteLine($"Самый медленный: {slowest} ({min:F2} км/ч)");
    Console.WriteLine($"Разница темпа: {max - min:F2} км/ч");
}

/* Вывод таблицы результатов */
static void PrintTable(string[] teams, double[] speeds, int n, bool showPosition)
{
    Console.WriteLine("-------------------------------");
    if (showPosition)
    {
        Console.WriteLine("| Поз. | Команда | Скорость |");
    }
    else
    {
        Console.WriteLine("| Команда | Скорость (км/ч) |");
    }
    Console.WriteLine("-------------------------------");

    for (int i = 0; i < n; i++)
    {
        if (showPosition)
        {
            Console.WriteLine($"| {i + 1,4} | {teams[i],-20} | {speeds[i],13:F2} |");
        }
        else
        {
            Console.WriteLine($"| {teams[i],-20} | {speeds[i],19:F2} |");
        }
    }
    Console.WriteLine("-----------------------------------------------");
}

/* Пузырьковая сортировка по убыванию скорости */
static void BubbleSort(string[] teams, double[] speeds, int n)
{
    for (int i = 0; i < n - 1; i++)
    {
        for (int j = 0; j < n - i - 1; j++)
        {
            if (speeds[j] < speeds[j + 1])
            {
                double tempSpeed = speeds[j];
                speeds[j] = speeds[j + 1];
                speeds[j + 1] = tempSpeed;

                string tempTeam = teams[j];
                teams[j] = teams[j + 1];
                teams[j + 1] = tempTeam;
            }
        }
    }
}

/* Фильтрация по минимальной скорости */
static void FilterBySpeed(string[] teams, double[] speeds, int n)
{
    Console.WriteLine("--- ФИЛЬТР ПО СКОРОСТИ ---");

    double minSpeed = ReadDouble("Введите минимальную скорость для отбора (км/ч): ");
    Console.WriteLine();

    Console.WriteLine($"Команды со скоростью >= {minSpeed:F2} км/ч:");

    int count = 0;
    for (int i = 0; i < n; i++)
    {
        if (speeds[i] >= minSpeed)
        {
            Console.WriteLine($"- {teams[i]} ({speeds[i]:F2} км/ч)");
            count++;
        }
    }

    Console.WriteLine($"\nОтобрано команд: {count}\n");
}

static int ReadInt(string msg)
{
    int value;
    do
    {
        Console.Write(msg);
    }
    while (!int.TryParse(Console.ReadLine(), out value) || value <= 0);

    return value;
}

static double ReadDouble(string msg)
{
    double value;
    string input;
    do
    {
        Console.Write(msg);
        input = Console.ReadLine()
            ?.Replace('.', ',');
    }
    while (!double.TryParse(input, out value) || value < 0);

    return value;
}
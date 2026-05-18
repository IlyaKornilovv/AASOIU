using System.Text;

internal class Program
{
    private const string DbFileName = "university.db";
    private const string ChairCsvFileName = "chair.csv";
    private const string TeacherCsvFileName = "teacher.csv";

    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        string basePath = AppContext.BaseDirectory;

        string dbPath = Path.Combine(basePath, DbFileName);
        string chairCsvPath = Path.Combine(basePath, ChairCsvFileName);
        string teacherCsvPath = Path.Combine(basePath, TeacherCsvFileName);

        CreateSampleCsvFilesIfNotExist(chairCsvPath, teacherCsvPath);

        var db = new DatabaseManager(dbPath);
        db.InitializeDatabase(chairCsvPath, teacherCsvPath);

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("========== МЕНЮ ==========");
            Console.WriteLine("1. Показать кафедры");
            Console.WriteLine("2. Показать преподавателей");
            Console.WriteLine("3. Добавить преподавателя");
            Console.WriteLine("4. Изменить преподавателя");
            Console.WriteLine("5. Удалить преподавателя");
            Console.WriteLine("6. Преподаватели по кафедре");
            Console.WriteLine("7. Отчёт: преподаватели и кафедры");
            Console.WriteLine("8. Отчёт: статистика по кафедрам");
            Console.WriteLine("9. Экспорт в CSV");
            Console.WriteLine("0. Выход");
            Console.Write("Выберите пункт: ");

            string? choice = Console.ReadLine();

            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    ShowChairs(db);
                    break;

                case "2":
                    ShowTeachers(db);
                    break;

                case "3":
                    AddTeacher(db);
                    break;

                case "4":
                    UpdateTeacher(db);
                    break;

                case "5":
                    DeleteTeacher(db);
                    break;

                case "6":
                    ShowTeachersByChair(db);
                    break;

                case "7":
                    PrintTeachersReport(db);
                    break;

                case "8":
                    PrintChairStatisticsReport(db);
                    break;

                case "9":
                    ExportToCsv(db, basePath);
                    break;

                case "0":
                    Console.WriteLine("Программа завершена.");
                    return;

                default:
                    Console.WriteLine("Неверный пункт меню.");
                    break;
            }
        }
    }

    private static void CreateSampleCsvFilesIfNotExist(string chairCsvPath, string teacherCsvPath)
    {
        if (!File.Exists(chairCsvPath))
        {
            File.WriteAllLines(chairCsvPath, new[]
            {
                "chair_id;chair_name",
                "1;Информатика и вычислительная техника",
                "2;Математика",
                "3;Физика",
                "4;Экономика"
            }, Encoding.UTF8);
        }

        if (!File.Exists(teacherCsvPath))
        {
            File.WriteAllLines(teacherCsvPath, new[]
            {
                "teacher_id;chair_id;teacher_name;publications",
                "1;1;Иванов Иван Иванович;12",
                "2;1;Петров Пётр Петрович;8",
                "3;2;Сидорова Анна Сергеевна;15",
                "4;3;Кузнецов Алексей Викторович;5",
                "5;4;Смирнова Ольга Николаевна;20"
            }, Encoding.UTF8);
        }
    }

    private static void ShowChairs(DatabaseManager db)
    {
        List<Chair> chairs = db.GetAllChairs();

        Console.WriteLine("Список кафедр:");

        if (chairs.Count == 0)
        {
            Console.WriteLine("Кафедры не найдены.");
            return;
        }

        foreach (Chair chair in chairs)
        {
            Console.WriteLine(chair);
        }
    }

    private static void ShowTeachers(DatabaseManager db)
    {
        List<Teacher> teachers = db.GetAllTeachers();

        Console.WriteLine("Список преподавателей:");

        if (teachers.Count == 0)
        {
            Console.WriteLine("Преподаватели не найдены.");
            return;
        }

        foreach (Teacher teacher in teachers)
        {
            Console.WriteLine(teacher);
        }
    }

    private static void AddTeacher(DatabaseManager db)
    {
        Console.WriteLine("Добавление преподавателя");

        ShowChairs(db);

        int chairId = ReadInt("Введите ID кафедры: ");

        if (!ChairExists(db, chairId))
        {
            Console.WriteLine("Кафедра с таким ID не найдена.");
            return;
        }

        Console.Write("Введите ФИО преподавателя: ");
        string name = Console.ReadLine() ?? "";

        if (string.IsNullOrWhiteSpace(name))
        {
            Console.WriteLine("Имя не может быть пустым.");
            return;
        }

        int publications = ReadInt("Введите количество публикаций: ");

        try
        {
            var teacher = new Teacher(0, chairId, name, publications);
            db.AddTeacher(teacher);
            Console.WriteLine("Преподаватель добавлен.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    private static void UpdateTeacher(DatabaseManager db)
    {
        Console.WriteLine("Изменение преподавателя");

        int id = ReadInt("Введите ID преподавателя: ");

        Teacher? teacher = db.GetTeacherById(id);

        if (teacher == null)
        {
            Console.WriteLine("Преподаватель не найден.");
            return;
        }

        Console.WriteLine($"Текущие данные: {teacher}");
        Console.WriteLine();

        ShowChairs(db);

        int chairId = ReadInt("Введите новый ID кафедры: ");

        if (!ChairExists(db, chairId))
        {
            Console.WriteLine("Кафедра с таким ID не найдена.");
            return;
        }

        Console.Write("Введите новое ФИО преподавателя: ");
        string name = Console.ReadLine() ?? "";

        if (string.IsNullOrWhiteSpace(name))
        {
            Console.WriteLine("Имя не может быть пустым.");
            return;
        }

        int publications = ReadInt("Введите новое количество публикаций: ");

        try
        {
            teacher.ChairId = chairId;
            teacher.Name = name;
            teacher.Publications = publications;

            db.UpdateTeacher(teacher);
            Console.WriteLine("Данные преподавателя обновлены.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    private static void DeleteTeacher(DatabaseManager db)
    {
        Console.WriteLine("Удаление преподавателя");

        int id = ReadInt("Введите ID преподавателя: ");

        Teacher? teacher = db.GetTeacherById(id);

        if (teacher == null)
        {
            Console.WriteLine("Преподаватель не найден.");
            return;
        }

        Console.WriteLine($"Будет удалён: {teacher}");
        Console.Write("Удалить? y/n: ");

        string? answer = Console.ReadLine();

        if (answer?.ToLower() == "y" || answer?.ToLower() == "д")
        {
            db.DeleteTeacher(id);
            Console.WriteLine("Преподаватель удалён.");
        }
        else
        {
            Console.WriteLine("Удаление отменено.");
        }
    }

    private static void ShowTeachersByChair(DatabaseManager db)
    {
        ShowChairs(db);

        int chairId = ReadInt("Введите ID кафедры: ");

        if (!ChairExists(db, chairId))
        {
            Console.WriteLine("Кафедра с таким ID не найдена.");
            return;
        }

        List<Teacher> teachers = db.GetTeachersByChair(chairId);

        Console.WriteLine();
        Console.WriteLine($"Преподаватели кафедры #{chairId}:");

        if (teachers.Count == 0)
        {
            Console.WriteLine("Преподаватели не найдены.");
            return;
        }

        foreach (Teacher teacher in teachers)
        {
            Console.WriteLine(teacher);
        }
    }

    private static void PrintTeachersReport(DatabaseManager db)
    {
        var report = new ReportBuilder(db)
            .Title("Преподаватели и кафедры")
            .Query(@"
SELECT 
    t.teacher_id,
    t.teacher_name,
    c.chair_name,
    t.publications
FROM teacher t
JOIN chair c ON t.chair_id = c.chair_id
ORDER BY t.teacher_id;
")
            .Header("ID", "Преподаватель", "Кафедра", "Публикации")
            .ColumnWidths(8, 35, 40, 15)
            .Numbered()
            .Footer("Всего преподавателей");

        report.Print();
    }

    private static void PrintChairStatisticsReport(DatabaseManager db)
    {
        var report = new ReportBuilder(db)
            .Title("Статистика по кафедрам")
            .Query(@"
SELECT
    c.chair_name,
    COUNT(t.teacher_id) AS teacher_count,
    IFNULL(SUM(t.publications), 0) AS total_publications,
    IFNULL(ROUND(AVG(t.publications), 2), 0) AS avg_publications
FROM chair c
LEFT JOIN teacher t ON c.chair_id = t.chair_id
GROUP BY c.chair_id, c.chair_name
ORDER BY c.chair_id;
")
            .Header("Кафедра", "Преподавателей", "Всего публикаций", "Среднее")
            .ColumnWidths(40, 18, 20, 12)
            .Numbered()
            .Footer("Всего кафедр");

        report.Print();
    }

    private static void ExportToCsv(DatabaseManager db, string basePath)
    {
        string chairExportPath = Path.Combine(basePath, "chair_export.csv");
        string teacherExportPath = Path.Combine(basePath, "teacher_export.csv");

        db.ExportToCsv(chairExportPath, teacherExportPath);

        Console.WriteLine("Экспорт выполнен:");
        Console.WriteLine(chairExportPath);
        Console.WriteLine(teacherExportPath);
    }

    private static bool ChairExists(DatabaseManager db, int chairId)
    {
        return db.GetAllChairs().Any(chair => chair.Id == chairId);
    }

    private static int ReadInt(string message)
    {
        while (true)
        {
            Console.Write(message);

            string? input = Console.ReadLine();

            if (int.TryParse(input, out int value))
            {
                return value;
            }

            Console.WriteLine("Введите целое число.");
        }
    }
}
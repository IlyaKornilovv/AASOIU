using System.Text;
using Microsoft.Data.Sqlite;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

string baseDir = AppContext.BaseDirectory;
string dbPath = Path.Combine(baseDir, "teachers.db");
string chairCsv = Path.Combine(baseDir, "chair.csv");
string teacherCsv = Path.Combine(baseDir, "teacher.csv");
string reportsDir = Path.Combine(baseDir, "reports");

var db = new DatabaseManager(dbPath);
db.InitializeDatabase(chairCsv, teacherCsv);

RunMainMenu(db, reportsDir);

static void RunMainMenu(DatabaseManager db, string reportsDir)
{
    string choice;
    do
    {
        Console.Clear();
        PrintMainMenu();

        Console.Write("Ваш выбор: ");
        choice = Console.ReadLine()?.Trim() ?? string.Empty;
        Console.WriteLine();

        switch (choice)
        {
            case "1":
                ShowChairs(db);
                Pause();
                break;
            case "2":
                ShowTeachers(db);
                Pause();
                break;
            case "3":
                AddTeacher(db);
                Pause();
                break;
            case "4":
                EditTeacher(db);
                Pause();
                break;
            case "5":
                DeleteTeacher(db);
                Pause();
                break;
            case "6":
                ReportsMenu(db, reportsDir);
                break;
            case "7":
                FilterByChair(db);
                Pause();
                break;
            case "8":
                ExportCsv(db);
                Pause();
                break;
            case "0":
                Console.WriteLine("До свидания!");
                break;
            default:
                Console.WriteLine("Неверный пункт меню.");
                Pause();
                break;
        }
    }
    while (choice != "0");
}

static void PrintMainMenu()
{
    Console.WriteLine("╔══════════════════════════════════════════════╗");
    Console.WriteLine("║ УПРАВЛЕНИЕ ПРЕПОДАВАТЕЛЯМИ                  ║");
    Console.WriteLine("╠══════════════════════════════════════════════╣");
    Console.WriteLine("║ 1 — Показать все кафедры                    ║");
    Console.WriteLine("║ 2 — Показать всех преподавателей            ║");
    Console.WriteLine("║ 3 — Добавить преподавателя                  ║");
    Console.WriteLine("║ 4 — Редактировать преподавателя             ║");
    Console.WriteLine("║ 5 — Удалить преподавателя                   ║");
    Console.WriteLine("║ 6 — Отчёты                                  ║");
    Console.WriteLine("║ 7 — Фильтр по кафедре [дополнительно]       ║");
    Console.WriteLine("║ 8 — Экспорт в CSV [дополнительно]           ║");
    Console.WriteLine("║ 0 — Выход                                   ║");
    Console.WriteLine("╚══════════════════════════════════════════════╝");
}

static void ShowChairs(DatabaseManager db)
{
    Console.WriteLine("--- Все кафедры ---");
    var chairs = db.GetAllChairs();

    if (chairs.Count == 0)
    {
        Console.WriteLine("Список кафедр пуст.");
        return;
    }

    foreach (var chair in chairs)
        Console.WriteLine(" " + chair);

    Console.WriteLine($"Итого: {chairs.Count}");
}

static void ShowTeachers(DatabaseManager db)
{
    Console.WriteLine("--- Все преподаватели ---");
    var teachers = db.GetAllTeachers();

    if (teachers.Count == 0)
    {
        Console.WriteLine("Список преподавателей пуст.");
        return;
    }

    foreach (var teacher in teachers)
        Console.WriteLine(" " + teacher);

    Console.WriteLine($"Итого: {teachers.Count}");
}

static void AddTeacher(DatabaseManager db)
{
    Console.WriteLine("--- Добавление преподавателя ---");
    PrintChairsForSelection(db);

    int chairId = ReadInt("ID кафедры: ");
    if (!db.ChairExists(chairId))
    {
        Console.WriteLine("Ошибка: кафедра с таким ID не существует.");
        return;
    }

    string name = ReadRequiredString("Имя преподавателя: ");
    int publications = ReadInt("Количество публикаций: ", minValue: 0);

    try
    {
        var teacher = new Teacher(0, chairId, name, publications);
        db.AddTeacher(teacher);
        Console.WriteLine("Преподаватель добавлен.");
    }
    catch (SqliteException ex)
    {
        Console.WriteLine($"Ошибка SQLite: {ex.Message}");
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine($"Ошибка: {ex.Message}");
    }
}

static void EditTeacher(DatabaseManager db)
{
    Console.WriteLine("--- Редактирование преподавателя ---");
    int id = ReadInt("Введите ID преподавателя: ");

    var teacher = db.GetTeacherById(id);
    if (teacher is null)
    {
        Console.WriteLine($"Преподаватель с ID={id} не найден.");
        return;
    }

    Console.WriteLine($"Текущие данные: {teacher}");
    Console.WriteLine("(нажмите Enter, чтобы оставить значение без изменений)");

    Console.Write($"Имя [{teacher.Name}]: ");
    string input = Console.ReadLine()?.Trim() ?? string.Empty;
    if (!string.IsNullOrWhiteSpace(input))
        teacher.Name = input;

    Console.Write($"ID кафедры [{teacher.ChairId}]: ");
    input = Console.ReadLine()?.Trim() ?? string.Empty;
    if (!string.IsNullOrWhiteSpace(input))
    {
        if (!int.TryParse(input, out int newChairId))
        {
            Console.WriteLine("Ошибка: ID кафедры должен быть целым числом.");
            return;
        }

        if (!db.ChairExists(newChairId))
        {
            Console.WriteLine("Ошибка: кафедра с таким ID не существует.");
            return;
        }

        teacher.ChairId = newChairId;
    }

    Console.Write($"Публикации [{teacher.Publications}]: ");
    input = Console.ReadLine()?.Trim() ?? string.Empty;
    if (!string.IsNullOrWhiteSpace(input))
    {
        if (!int.TryParse(input, out int newPublications))
        {
            Console.WriteLine("Ошибка: количество публикаций должно быть целым числом.");
            return;
        }

        try
        {
            teacher.Publications = newPublications;
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
            return;
        }
    }

    bool updated = db.UpdateTeacher(teacher);
    Console.WriteLine(updated ? "Данные обновлены." : "Запись не обновлена.");
}

static void DeleteTeacher(DatabaseManager db)
{
    Console.WriteLine("--- Удаление преподавателя ---");
    int id = ReadInt("Введите ID преподавателя: ");

    var teacher = db.GetTeacherById(id);
    if (teacher is null)
    {
        Console.WriteLine($"Преподаватель с ID={id} не найден.");
        return;
    }

    Console.WriteLine($"Найдена запись: {teacher}");
    Console.Write("Удалить этого преподавателя? (да/нет): ");
    string confirm = (Console.ReadLine() ?? string.Empty).Trim().ToLowerInvariant();

    if (confirm != "да")
    {
        Console.WriteLine("Удаление отменено.");
        return;
    }

    bool deleted = db.DeleteTeacher(id);
    Console.WriteLine(deleted ? "Преподаватель удалён." : "Удаление не выполнено.");
}

static void ReportsMenu(DatabaseManager db, string reportsDir)
{
    string choice;
    do
    {
        Console.Clear();
        Console.WriteLine("--- Отчёты ---");
        Console.WriteLine(" 1 — Преподаватели по кафедрам");
        Console.WriteLine(" 2 — Количество преподавателей по кафедрам");
        Console.WriteLine(" 3 — Среднее число публикаций по кафедрам");
        Console.WriteLine(" 0 — Назад");
        Console.Write("Ваш выбор: ");
        choice = Console.ReadLine()?.Trim() ?? string.Empty;
        Console.WriteLine();

        switch (choice)
        {
            case "1":
                ShowAndMaybeSaveReport(
                    BuildReport1_TeachersWithChairs(db),
                    Path.Combine(reportsDir, "report_teachers_with_chairs.txt"));
                Pause();
                break;

            case "2":
                ShowAndMaybeSaveReport(
                    BuildReport2_CountByChair(db),
                    Path.Combine(reportsDir, "report_count_by_chair.txt"));
                Pause();
                break;

            case "3":
                ShowAndMaybeSaveReport(
                    BuildReport3_AvgPublicationsByChair(db),
                    Path.Combine(reportsDir, "report_avg_publications_by_chair.txt"));
                Pause();
                break;

            case "0":
                break;

            default:
                Console.WriteLine("Неверный пункт.");
                Pause();
                break;
        }
    }
    while (choice != "0");
}

static ReportBuilder BuildReport1_TeachersWithChairs(DatabaseManager db)
{
    return new ReportBuilder(db)
        .Query(@"SELECT t.teacher_name, c.chair_name, t.publications
FROM teacher t
JOIN chair c ON t.chair_id = c.chair_id
ORDER BY t.teacher_name")
        .Title("Преподаватели по кафедрам")
        .Header("Имя", "Кафедра", "Публикации")
        .ColumnWidths(24, 34, 14)
        .Numbered()
        .Footer("Всего записей");
}

static ReportBuilder BuildReport2_CountByChair(DatabaseManager db)
{
    return new ReportBuilder(db)
        .Query(@"SELECT c.chair_name, COUNT(t.teacher_id) AS cnt
FROM chair c
LEFT JOIN teacher t ON t.chair_id = c.chair_id
GROUP BY c.chair_name
ORDER BY c.chair_name")
        .Title("Количество преподавателей по кафедрам")
        .Header("Кафедра", "Кол-во")
        .ColumnWidths(34, 10)
        .Footer("Всего строк");
}

static ReportBuilder BuildReport3_AvgPublicationsByChair(DatabaseManager db)
{
    return new ReportBuilder(db)
        .Query(@"SELECT c.chair_name,
ROUND(AVG(COALESCE(t.publications, 0)), 1) AS avg_publications
FROM chair c
LEFT JOIN teacher t ON t.chair_id = c.chair_id
GROUP BY c.chair_name
ORDER BY avg_publications DESC, c.chair_name")
        .Title("Среднее количество публикаций по кафедрам")
        .Header("Кафедра", "Среднее публикаций")
        .ColumnWidths(34, 20)
        .Footer("Всего строк");
}

static void ShowAndMaybeSaveReport(ReportBuilder report, string filePath)
{
    report.Print();
    Console.WriteLine();
    Console.Write("Сохранить отчёт в файл? (да/нет): ");
    string save = (Console.ReadLine() ?? string.Empty).Trim().ToLowerInvariant();

    if (save == "да")
        report.SaveToFile(filePath);
}

static void FilterByChair(DatabaseManager db)
{
    Console.WriteLine("--- Фильтр по кафедре ---");
    PrintChairsForSelection(db);

    int chairId = ReadInt("Введите ID кафедры: ");
    var chair = db.GetChairById(chairId);

    if (chair is null)
    {
        Console.WriteLine("Кафедра с таким ID не найдена.");
        return;
    }

    var teachers = db.GetTeachersByChair(chairId);
    Console.WriteLine($"\nПреподаватели кафедры \"{chair.Name}\":");

    if (teachers.Count == 0)
    {
        Console.WriteLine("На этой кафедре нет преподавателей.");
        return;
    }

    foreach (var teacher in teachers)
        Console.WriteLine(" " + teacher);

    Console.WriteLine($"Итого: {teachers.Count}");
}

static void ExportCsv(DatabaseManager db)
{
    string baseDir = AppContext.BaseDirectory;
    string chairPath = Path.Combine(baseDir, "chair_export.csv");
    string teacherPath = Path.Combine(baseDir, "teacher_export.csv");

    db.ExportToCsv(chairPath, teacherPath);
    Console.WriteLine($"Кафедры экспортированы в: {chairPath}");
    Console.WriteLine($"Преподаватели экспортированы в: {teacherPath}");
}

static void PrintChairsForSelection(DatabaseManager db)
{
    Console.WriteLine("Доступные кафедры:");
    var chairs = db.GetAllChairs();

    if (chairs.Count == 0)
    {
        Console.WriteLine(" Справочник кафедр пуст.");
        return;
    }

    foreach (var chair in chairs)
        Console.WriteLine(" " + chair);
}

static int ReadInt(string prompt, int? minValue = null)
{
    while (true)
    {
        Console.Write(prompt);
        string input = Console.ReadLine()?.Trim() ?? string.Empty;

        if (!int.TryParse(input, out int value))
        {
            Console.WriteLine("Ошибка: введите целое число.");
            continue;
        }

        if (minValue.HasValue && value < minValue.Value)
        {
            Console.WriteLine($"Ошибка: число должно быть не меньше {minValue.Value}.");
            continue;
        }

        return value;
    }
}

static string ReadRequiredString(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string input = Console.ReadLine()?.Trim() ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(input))
            return input;

        Console.WriteLine("Ошибка: поле не может быть пустым.");
    }
}

static void Pause()
{
    Console.WriteLine();
    Console.Write("Нажмите любую клавишу, чтобы продолжить...");
    Console.ReadKey(true);
}

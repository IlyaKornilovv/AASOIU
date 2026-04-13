using System.Text;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

string dbPath = "teachers.db";
string chairCsv = Path.Combine(AppContext.BaseDirectory, "chair.csv");
string teacherCsv = Path.Combine(AppContext.BaseDirectory, "teacher.csv");

var db = new DatabaseManager(dbPath);
db.InitializeDatabase(chairCsv, teacherCsv);
Console.WriteLine();

string choice;
do
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
    Console.Write("Ваш выбор: ");
    choice = Console.ReadLine()?.Trim() ?? "";
    Console.WriteLine();

    switch (choice)
    {
        case "1": ShowChairs(db); break;
        case "2": ShowTeachers(db); break;
        case "3": AddTeacher(db); break;
        case "4": EditTeacher(db); break;
        case "5": DeleteTeacher(db); break;
        case "6": ReportsMenu(db); break;
        case "7": FilterByChair(db); break;
        case "8": ExportCsv(db); break;
        case "0": Console.WriteLine("До свидания!"); break;
        default: Console.WriteLine("Неверный пункт меню."); break;
    }

    Console.WriteLine();
}
while (choice != "0");

static void ShowChairs(DatabaseManager db)
{
    Console.WriteLine("--- Все кафедры ---");
    var chairs = db.GetAllChairs();
    foreach (var chair in chairs)
        Console.WriteLine(" " + chair);
    Console.WriteLine($"Итого: {chairs.Count}");
}

static void ShowTeachers(DatabaseManager db)
{
    Console.WriteLine("--- Все преподаватели ---");
    var teachers = db.GetAllTeachers();
    foreach (var teacher in teachers)
        Console.WriteLine(" " + teacher);
    Console.WriteLine($"Итого: {teachers.Count}");
}

static void AddTeacher(DatabaseManager db)
{
    Console.WriteLine("--- Добавление преподавателя ---");
    Console.WriteLine("Доступные кафедры:");
    var chairs = db.GetAllChairs();
    foreach (var chair in chairs)
        Console.WriteLine(" " + chair);

    Console.Write("ID кафедры: ");
    if (!int.TryParse(Console.ReadLine(), out int chairId))
    {
        Console.WriteLine("Ошибка: введите целое число.");
        return;
    }

    Console.Write("Имя преподавателя: ");
    string name = Console.ReadLine()?.Trim() ?? "";
    if (name.Length == 0)
    {
        Console.WriteLine("Ошибка: имя не может быть пустым.");
        return;
    }

    Console.Write("Количество публикаций: ");
    if (!int.TryParse(Console.ReadLine(), out int publications))
    {
        Console.WriteLine("Ошибка: введите целое число.");
        return;
    }

    try
    {
        var teacher = new Teacher(0, chairId, name, publications);
        db.AddTeacher(teacher);
        Console.WriteLine("Преподаватель добавлен.");
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine($"Ошибка: {ex.Message}");
    }
}

static void EditTeacher(DatabaseManager db)
{
    Console.WriteLine("--- Редактирование преподавателя ---");
    Console.Write("Введите ID преподавателя: ");
    if (!int.TryParse(Console.ReadLine(), out int id))
    {
        Console.WriteLine("Ошибка: введите целое число.");
        return;
    }

    var teacher = db.GetTeacherById(id);
    if (teacher == null)
    {
        Console.WriteLine($"Преподаватель с ID={id} не найден.");
        return;
    }

    Console.WriteLine($"Текущие данные: {teacher}");
    Console.WriteLine("(нажмите Enter, чтобы оставить значение без изменений)");

    Console.Write($"Имя [{teacher.Name}]: ");
    string input = Console.ReadLine()?.Trim() ?? "";
    if (input.Length > 0)
        teacher.Name = input;

    Console.Write($"ID кафедры [{teacher.ChairId}]: ");
    input = Console.ReadLine()?.Trim() ?? "";
    if (input.Length > 0 && int.TryParse(input, out int newChairId))
        teacher.ChairId = newChairId;

    Console.Write($"Публикации [{teacher.Publications}]: ");
    input = Console.ReadLine()?.Trim() ?? "";
    if (input.Length > 0 && int.TryParse(input, out int newPublications))
    {
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

    db.UpdateTeacher(teacher);
    Console.WriteLine("Данные обновлены.");
}

static void DeleteTeacher(DatabaseManager db)
{
    Console.WriteLine("--- Удаление преподавателя ---");
    Console.Write("Введите ID преподавателя: ");
    if (!int.TryParse(Console.ReadLine(), out int id))
    {
        Console.WriteLine("Ошибка: введите целое число.");
        return;
    }

    var teacher = db.GetTeacherById(id);
    if (teacher == null)
    {
        Console.WriteLine($"Преподаватель с ID={id} не найден.");
        return;
    }

    Console.Write($"Удалить «{teacher.Name}»? (да/нет): ");
    string confirm = Console.ReadLine()?.Trim().ToLower() ?? "";
    if (confirm == "да")
    {
        db.DeleteTeacher(id);
        Console.WriteLine("Преподаватель удалён.");
    }
    else
    {
        Console.WriteLine("Удаление отменено.");
    }
}

static void ReportsMenu(DatabaseManager db)
{
    string choice;
    do
    {
        Console.WriteLine("--- Отчёты ---");
        Console.WriteLine(" 1 — Преподаватели по кафедрам");
        Console.WriteLine(" 2 — Количество преподавателей по кафедрам");
        Console.WriteLine(" 3 — Среднее число публикаций по кафедрам");
        Console.WriteLine(" 0 — Назад");
        Console.Write("Ваш выбор: ");
        choice = Console.ReadLine()?.Trim() ?? "";

        switch (choice)
        {
            case "1": Report1_TeachersWithChairs(db); break;
            case "2": Report2_CountByChair(db); break;
            case "3": Report3_AvgPublicationsByChair(db); break;
            case "0": break;
            default: Console.WriteLine("Неверный пункт."); break;
        }

        Console.WriteLine();
    }
    while (choice != "0");
}

static void Report1_TeachersWithChairs(DatabaseManager db)
{
    new ReportBuilder(db)
        .Query(@"SELECT t.teacher_name, c.chair_name, t.publications
FROM teacher t
JOIN chair c ON t.chair_id = c.chair_id
ORDER BY t.teacher_name")
        .Title("Преподаватели по кафедрам")
        .Header("Имя", "Кафедра", "Публикации")
        .ColumnWidths(24, 28, 14)
        .Numbered()
        .Footer("Всего записей")
        .Print();
}

static void Report2_CountByChair(DatabaseManager db)
{
    new ReportBuilder(db)
        .Query(@"SELECT c.chair_name, COUNT(*) AS cnt
FROM teacher t
JOIN chair c ON t.chair_id = c.chair_id
GROUP BY c.chair_name
ORDER BY c.chair_name")
        .Title("Количество преподавателей по кафедрам")
        .Header("Кафедра", "Кол-во")
        .ColumnWidths(28, 10)
        .Print();
}

static void Report3_AvgPublicationsByChair(DatabaseManager db)
{
    new ReportBuilder(db)
        .Query(@"SELECT c.chair_name,
ROUND(AVG(t.publications), 1) AS avg_publications
FROM teacher t
JOIN chair c ON t.chair_id = c.chair_id
GROUP BY c.chair_name
ORDER BY avg_publications DESC")
        .Title("Среднее количество публикаций по кафедрам")
        .Header("Кафедра", "Среднее публикаций")
        .ColumnWidths(28, 20)
        .Print();
}

static void FilterByChair(DatabaseManager db)
{
    Console.WriteLine("--- Фильтр по кафедре ---");
    Console.WriteLine("Доступные кафедры:");
    var chairs = db.GetAllChairs();
    foreach (var chair in chairs)
        Console.WriteLine(" " + chair);

    Console.Write("Введите ID кафедры: ");
    if (!int.TryParse(Console.ReadLine(), out int chairId))
    {
        Console.WriteLine("Ошибка: введите целое число.");
        return;
    }

    var teachers = db.GetTeachersByChair(chairId);
    if (teachers.Count == 0)
    {
        Console.WriteLine("На этой кафедре нет преподавателей.");
        return;
    }

    Console.WriteLine($"\nПреподаватели кафедры #{chairId}:");
    foreach (var teacher in teachers)
        Console.WriteLine(" " + teacher);
    Console.WriteLine($"Итого: {teachers.Count}");
}

static void ExportCsv(DatabaseManager db)
{
    string chairPath = Path.Combine(AppContext.BaseDirectory, "chair_export.csv");
    string teacherPath = Path.Combine(AppContext.BaseDirectory, "teacher_export.csv");
    db.ExportToCsv(chairPath, teacherPath);
    Console.WriteLine($"Кафедры экспортированы в: {chairPath}");
    Console.WriteLine($"Преподаватели экспортированы в: {teacherPath}");
}

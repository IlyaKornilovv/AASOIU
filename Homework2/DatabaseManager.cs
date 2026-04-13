using Microsoft.Data.Sqlite;

/// <summary>
/// Управление базой данных SQLite.
/// Инкапсулирует операции: создание таблиц, импорт CSV, CRUD и выполнение запросов для отчётов.
/// </summary>
class DatabaseManager
{
    private readonly string _connectionString;

    /// <summary>
    /// Конструктор. Принимает путь к файлу БД.
    /// </summary>
    public DatabaseManager(string dbPath)
    {
        _connectionString = $"Data Source={dbPath}";
    }

    /// <summary>
    /// Создаёт таблицы и загружает CSV при первом запуске.
    /// </summary>
    public void InitializeDatabase(string chairCsvPath, string teacherCsvPath)
    {
        CreateTables();

        if (GetAllChairs().Count == 0 && File.Exists(chairCsvPath))
        {
            ImportChairsFromCsv(chairCsvPath);
            Console.WriteLine($"[OK] Загружены кафедры из {chairCsvPath}");
        }

        if (GetAllTeachers().Count == 0 && File.Exists(teacherCsvPath))
        {
            ImportTeachersFromCsv(teacherCsvPath);
            Console.WriteLine($"[OK] Загружены преподаватели из {teacherCsvPath}");
        }
    }

    /// <summary>Создание таблиц</summary>
    private void CreateTables()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        using var pragma = conn.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON;";
        pragma.ExecuteNonQuery();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS chair (
    chair_id INTEGER PRIMARY KEY AUTOINCREMENT,
    chair_name TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS teacher (
    teacher_id INTEGER PRIMARY KEY AUTOINCREMENT,
    chair_id INTEGER NOT NULL,
    teacher_name TEXT NOT NULL,
    publications INTEGER NOT NULL CHECK(publications >= 0),
    FOREIGN KEY (chair_id) REFERENCES chair(chair_id)
);";
        cmd.ExecuteNonQuery();
    }

    /// <summary>Импорт кафедр из CSV</summary>
    private void ImportChairsFromCsv(string path)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        string[] lines = File.ReadAllLines(path);

        for (int i = 1; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(';');
            if (parts.Length < 2) continue;

            var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO chair (chair_id, chair_name) VALUES (@id, @name)";
            cmd.Parameters.AddWithValue("@id", int.Parse(parts[0]));
            cmd.Parameters.AddWithValue("@name", parts[1]);
            cmd.ExecuteNonQuery();
        }
    }

    /// <summary>Импорт преподавателей из CSV</summary>
    private void ImportTeachersFromCsv(string path)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        string[] lines = File.ReadAllLines(path);

        for (int i = 1; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(';');
            if (parts.Length < 4) continue;

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
INSERT INTO teacher (teacher_id, chair_id, teacher_name, publications)
VALUES (@id, @chairId, @name, @publications)";
            cmd.Parameters.AddWithValue("@id", int.Parse(parts[0]));
            cmd.Parameters.AddWithValue("@chairId", int.Parse(parts[1]));
            cmd.Parameters.AddWithValue("@name", parts[2]);
            cmd.Parameters.AddWithValue("@publications", int.Parse(parts[3]));
            cmd.ExecuteNonQuery();
        }
    }

    /// <summary>Получить все кафедры</summary>
    public List<Chair> GetAllChairs()
    {
        var result = new List<Chair>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT chair_id, chair_name FROM chair ORDER BY chair_id";
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            result.Add(new Chair(reader.GetInt32(0), reader.GetString(1)));
        }

        return result;
    }

    /// <summary>Получить всех преподавателей</summary>
    public List<Teacher> GetAllTeachers()
    {
        var result = new List<Teacher>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT teacher_id, chair_id, teacher_name, publications FROM teacher ORDER BY teacher_id";
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            result.Add(new Teacher(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetString(2),
                reader.GetInt32(3)));
        }

        return result;
    }

    /// <summary>Получить преподавателя по Id</summary>
    public Teacher? GetTeacherById(int id)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
SELECT teacher_id, chair_id, teacher_name, publications
FROM teacher
WHERE teacher_id = @id";
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new Teacher(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetString(2),
                reader.GetInt32(3));
        }

        return null;
    }

    /// <summary>Добавить преподавателя (Id генерируется автоматически)</summary>
    public void AddTeacher(Teacher teacher)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
INSERT INTO teacher (chair_id, teacher_name, publications)
VALUES (@chairId, @name, @publications)";
        cmd.Parameters.AddWithValue("@chairId", teacher.ChairId);
        cmd.Parameters.AddWithValue("@name", teacher.Name);
        cmd.Parameters.AddWithValue("@publications", teacher.Publications);
        cmd.ExecuteNonQuery();
    }

    /// <summary>Обновить данные преподавателя</summary>
    public void UpdateTeacher(Teacher teacher)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
UPDATE teacher
SET chair_id = @chairId,
    teacher_name = @name,
    publications = @publications
WHERE teacher_id = @id";
        cmd.Parameters.AddWithValue("@id", teacher.Id);
        cmd.Parameters.AddWithValue("@chairId", teacher.ChairId);
        cmd.Parameters.AddWithValue("@name", teacher.Name);
        cmd.Parameters.AddWithValue("@publications", teacher.Publications);
        cmd.ExecuteNonQuery();
    }

    /// <summary>Удалить преподавателя по Id</summary>
    public void DeleteTeacher(int id)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM teacher WHERE teacher_id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Выполняет SQL-запрос и возвращает имена столбцов и строки результата.
    /// Используется классом ReportBuilder.
    /// </summary>
    public (string[] columns, List<string[]> rows) ExecuteQuery(string sql)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        using var reader = cmd.ExecuteReader();

        string[] columns = new string[reader.FieldCount];
        for (int i = 0; i < reader.FieldCount; i++)
            columns[i] = reader.GetName(i);

        var rows = new List<string[]>();
        while (reader.Read())
        {
            string[] row = new string[reader.FieldCount];
            for (int i = 0; i < reader.FieldCount; i++)
                row[i] = reader.GetValue(i)?.ToString() ?? "";
            rows.Add(row);
        }

        return (columns, rows);
    }

    /// <summary>[Дополнительно] Получить преподавателей конкретной кафедры</summary>
    public List<Teacher> GetTeachersByChair(int chairId)
    {
        var result = new List<Teacher>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
SELECT teacher_id, chair_id, teacher_name, publications
FROM teacher
WHERE chair_id = @chairId
ORDER BY teacher_name";
        cmd.Parameters.AddWithValue("@chairId", chairId);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Teacher(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetString(2),
                reader.GetInt32(3)));
        }

        return result;
    }

    /// <summary>[Дополнительно] Экспорт обеих таблиц в CSV-файлы</summary>
    public void ExportToCsv(string chairPath, string teacherPath)
    {
        var chairLines = new List<string> { "chair_id;chair_name" };
        foreach (var chair in GetAllChairs())
            chairLines.Add($"{chair.Id};{chair.Name}");
        File.WriteAllLines(chairPath, chairLines);

        var teacherLines = new List<string> { "teacher_id;chair_id;teacher_name;publications" };
        foreach (var teacher in GetAllTeachers())
            teacherLines.Add($"{teacher.Id};{teacher.ChairId};{teacher.Name};{teacher.Publications}");
        File.WriteAllLines(teacherPath, teacherLines);
    }
}

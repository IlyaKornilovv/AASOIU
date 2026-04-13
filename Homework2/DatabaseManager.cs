using Microsoft.Data.Sqlite;

/// <summary>
/// Управление базой данных SQLite:
/// создание таблиц, импорт CSV, CRUD и выполнение запросов для отчётов.
/// </summary>
public class DatabaseManager
{
    private readonly string _connectionString;

    /// <summary>Конструктор. Принимает путь к файлу БД.</summary>
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

        if (!File.Exists(chairCsvPath))
            Console.WriteLine($"[WARN] Не найден файл кафедр: {chairCsvPath}");

        if (!File.Exists(teacherCsvPath))
            Console.WriteLine($"[WARN] Не найден файл преподавателей: {teacherCsvPath}");

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

    /// <summary>Создание таблиц.</summary>
    private void CreateTables()
    {
        using var conn = CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS chair (
    chair_id   INTEGER PRIMARY KEY,
    chair_name TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS teacher (
    teacher_id    INTEGER PRIMARY KEY AUTOINCREMENT,
    chair_id      INTEGER NOT NULL,
    teacher_name  TEXT NOT NULL,
    publications  INTEGER NOT NULL CHECK(publications >= 0),
    FOREIGN KEY (chair_id) REFERENCES chair(chair_id)
        ON UPDATE CASCADE
        ON DELETE RESTRICT
);";
        cmd.ExecuteNonQuery();
    }

    /// <summary>Открывает подключение и включает внешние ключи для SQLite.</summary>
    private SqliteConnection CreateConnection()
    {
        var conn = new SqliteConnection(_connectionString);
        conn.Open();

        using var pragma = conn.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON;";
        pragma.ExecuteNonQuery();

        return conn;
    }

    /// <summary>Импорт кафедр из CSV.</summary>
    private void ImportChairsFromCsv(string path)
    {
        var lines = File.ReadAllLines(path);

        using var conn = CreateConnection();
        using var transaction = conn.BeginTransaction();

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;

            string[] parts = lines[i].Split(';');
            if (parts.Length < 2)
                continue;

            using var cmd = conn.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = @"
INSERT OR IGNORE INTO chair (chair_id, chair_name)
VALUES (@id, @name)";
            cmd.Parameters.AddWithValue("@id", int.Parse(parts[0]));
            cmd.Parameters.AddWithValue("@name", parts[1].Trim());
            cmd.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    /// <summary>Импорт преподавателей из CSV.</summary>
    private void ImportTeachersFromCsv(string path)
    {
        var lines = File.ReadAllLines(path);

        using var conn = CreateConnection();
        using var transaction = conn.BeginTransaction();

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;

            string[] parts = lines[i].Split(';');
            if (parts.Length < 4)
                continue;

            using var cmd = conn.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = @"
INSERT OR IGNORE INTO teacher (teacher_id, chair_id, teacher_name, publications)
VALUES (@id, @chairId, @name, @publications)";
            cmd.Parameters.AddWithValue("@id", int.Parse(parts[0]));
            cmd.Parameters.AddWithValue("@chairId", int.Parse(parts[1]));
            cmd.Parameters.AddWithValue("@name", parts[2].Trim());
            cmd.Parameters.AddWithValue("@publications", int.Parse(parts[3]));
            cmd.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    /// <summary>Получить все кафедры.</summary>
    public List<Chair> GetAllChairs()
    {
        var result = new List<Chair>();

        using var conn = CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT chair_id, chair_name FROM chair ORDER BY chair_id";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            result.Add(new Chair(reader.GetInt32(0), reader.GetString(1)));

        return result;
    }

    /// <summary>Получить кафедру по Id.</summary>
    public Chair? GetChairById(int id)
    {
        using var conn = CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
SELECT chair_id, chair_name
FROM chair
WHERE chair_id = @id";
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
            return null;

        return new Chair(reader.GetInt32(0), reader.GetString(1));
    }

    /// <summary>Существует ли кафедра.</summary>
    public bool ChairExists(int id) => GetChairById(id) is not null;

    /// <summary>Получить всех преподавателей.</summary>
    public List<Teacher> GetAllTeachers()
    {
        var result = new List<Teacher>();

        using var conn = CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
SELECT teacher_id, chair_id, teacher_name, publications
FROM teacher
ORDER BY teacher_id";

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

    /// <summary>Получить преподавателя по Id.</summary>
    public Teacher? GetTeacherById(int id)
    {
        using var conn = CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
SELECT teacher_id, chair_id, teacher_name, publications
FROM teacher
WHERE teacher_id = @id";
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
            return null;

        return new Teacher(
            reader.GetInt32(0),
            reader.GetInt32(1),
            reader.GetString(2),
            reader.GetInt32(3));
    }

    /// <summary>Добавить преподавателя.</summary>
    public void AddTeacher(Teacher teacher)
    {
        using var conn = CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
INSERT INTO teacher (chair_id, teacher_name, publications)
VALUES (@chairId, @name, @publications)";
        cmd.Parameters.AddWithValue("@chairId", teacher.ChairId);
        cmd.Parameters.AddWithValue("@name", teacher.Name.Trim());
        cmd.Parameters.AddWithValue("@publications", teacher.Publications);
        cmd.ExecuteNonQuery();
    }

    /// <summary>Обновить данные преподавателя.</summary>
    public bool UpdateTeacher(Teacher teacher)
    {
        using var conn = CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
UPDATE teacher
SET chair_id = @chairId,
    teacher_name = @name,
    publications = @publications
WHERE teacher_id = @id";
        cmd.Parameters.AddWithValue("@id", teacher.Id);
        cmd.Parameters.AddWithValue("@chairId", teacher.ChairId);
        cmd.Parameters.AddWithValue("@name", teacher.Name.Trim());
        cmd.Parameters.AddWithValue("@publications", teacher.Publications);

        return cmd.ExecuteNonQuery() > 0;
    }

    /// <summary>Удалить преподавателя по Id.</summary>
    public bool DeleteTeacher(int id)
    {
        using var conn = CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM teacher WHERE teacher_id = @id";
        cmd.Parameters.AddWithValue("@id", id);

        return cmd.ExecuteNonQuery() > 0;
    }

    /// <summary>
    /// Выполняет SQL-запрос и возвращает имена столбцов и строки результата.
    /// Используется классом ReportBuilder.
    /// </summary>
    public (string[] columns, List<string[]> rows) ExecuteQuery(string sql)
    {
        using var conn = CreateConnection();
        using var cmd = conn.CreateCommand();
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
                row[i] = reader.GetValue(i)?.ToString() ?? string.Empty;
            rows.Add(row);
        }

        return (columns, rows);
    }

    /// <summary>[Дополнительно] Получить преподавателей конкретной кафедры.</summary>
    public List<Teacher> GetTeachersByChair(int chairId)
    {
        var result = new List<Teacher>();

        using var conn = CreateConnection();
        using var cmd = conn.CreateCommand();
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

    /// <summary>[Дополнительно] Экспорт обеих таблиц в CSV-файлы.</summary>
    public void ExportToCsv(string chairPath, string teacherPath)
    {
        var chairLines = new List<string> { "chair_id;chair_name" };
        chairLines.AddRange(GetAllChairs().Select(chair => $"{chair.Id};{chair.Name}"));
        File.WriteAllLines(chairPath, chairLines);

        var teacherLines = new List<string> { "teacher_id;chair_id;teacher_name;publications" };
        teacherLines.AddRange(GetAllTeachers()
            .Select(teacher => $"{teacher.Id};{teacher.ChairId};{teacher.Name};{teacher.Publications}"));
        File.WriteAllLines(teacherPath, teacherLines);
    }
}

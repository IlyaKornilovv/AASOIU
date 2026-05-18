using Microsoft.Data.Sqlite;

// === Константы ===
const string dbFile = "database.db";
const string devCsv = "dev.csv";
const string depCsv = "dep.csv";

// === Запуск ===
CreateDatabase(dbFile);
LoadData(dbFile, devCsv, depCsv);

// Вывод таблиц
PrintTable(dbFile, "dep");
PrintTable(dbFile, "dev");

// Projection
Projection(dbFile, "dev", "dev_name");

// Where
Where(dbFile, "dev", "dep_id", "2");

// Join
Join(dbFile, "dev", "dep", "dep_id", "dep_id");

// Union (пример)
Union(dbFile, "dev", "dev");

// Intersect (пример)
Intersect(dbFile, "dev", "dev");

// Difference (пример)
Difference(dbFile, "dev", "dev");

// Cartesian Product
Product(dbFile, "dev", "dep");

// Group Avg
GroupAvg(dbFile, "dev", "dep_id", "dev_commits");


// ========================================
// === СОЗДАНИЕ БД ========================
// ========================================

static void CreateDatabase(string dbPath)
{
    if (File.Exists(dbPath))
        File.Delete(dbPath);

    using var conn = new SqliteConnection($"Data Source={dbPath}");
    conn.Open();

    var cmd = conn.CreateCommand();

    cmd.CommandText = @"
        CREATE TABLE dep (
            dep_id INTEGER PRIMARY KEY,
            dep_name TEXT
        );";
    cmd.ExecuteNonQuery();

    cmd.CommandText = @"
        CREATE TABLE dev (
            dev_id INTEGER PRIMARY KEY,
            dep_id INTEGER,
            dev_name TEXT,
            dev_commits INTEGER
        );";
    cmd.ExecuteNonQuery();

    Console.WriteLine("[OK] Database created");
}


// ========================================
// === ЗАГРУЗКА CSV =======================
// ========================================

static void LoadData(string dbPath, string devCsv, string depCsv)
{
    using var conn = new SqliteConnection($"Data Source={dbPath}");
    conn.Open();

    // dep
    var depLines = File.ReadAllLines(depCsv);
    foreach (var line in depLines.Skip(1))
    {
        var p = line.Split(';');

        var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO dep VALUES (@id, @name)";
        cmd.Parameters.AddWithValue("@id", int.Parse(p[0]));
        cmd.Parameters.AddWithValue("@name", p[1]);
        cmd.ExecuteNonQuery();
    }

    // dev
    var devLines = File.ReadAllLines(devCsv);
    foreach (var line in devLines.Skip(1))
    {
        var p = line.Split(';');

        var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO dev VALUES (@id, @dep, @name, @commits)";
        cmd.Parameters.AddWithValue("@id", int.Parse(p[0]));
        cmd.Parameters.AddWithValue("@dep", int.Parse(p[1]));
        cmd.Parameters.AddWithValue("@name", p[2]);
        cmd.Parameters.AddWithValue("@commits", int.Parse(p[3]));
        cmd.ExecuteNonQuery();
    }

    Console.WriteLine("[OK] Data loaded");
}


// ========================================
// === ВЫВОД ==============================
// ========================================

static void PrintTable(string dbPath, string table)
{
    using var conn = new SqliteConnection($"Data Source={dbPath}");
    conn.Open();

    var cmd = conn.CreateCommand();
    cmd.CommandText = $"SELECT * FROM {table}";

    using var reader = cmd.ExecuteReader();

    Console.WriteLine($"\n=== {table} ===");

    for (int i = 0; i < reader.FieldCount; i++)
        Console.Write($"{reader.GetName(i),-20}");
    Console.WriteLine();

    while (reader.Read())
    {
        for (int i = 0; i < reader.FieldCount; i++)
            Console.Write($"{reader[i],-20}");
        Console.WriteLine();
    }
}


// ========================================
// === ОПЕРАЦИИ ===========================
// ========================================

// Projection
static void Projection(string db, string table, string column)
{
    Console.WriteLine("\n=== PROJECTION ===");

    using var conn = new SqliteConnection($"Data Source={db}");
    conn.Open();

    var cmd = conn.CreateCommand();
    cmd.CommandText = $"SELECT {column} FROM {table}";

    using var reader = cmd.ExecuteReader();
    while (reader.Read())
        Console.WriteLine(reader[0]);
}


// Where
static void Where(string db, string table, string column, string value)
{
    Console.WriteLine("\n=== WHERE ===");

    using var conn = new SqliteConnection($"Data Source={db}");
    conn.Open();

    var cmd = conn.CreateCommand();
    cmd.CommandText = $"SELECT * FROM {table} WHERE {column} = @val";
    cmd.Parameters.AddWithValue("@val", value);

    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        for (int i = 0; i < reader.FieldCount; i++)
            Console.Write($"{reader[i]} ");
        Console.WriteLine();
    }
}


// Join
static void Join(string db, string t1, string t2, string k1, string k2)
{
    Console.WriteLine("\n=== JOIN ===");

    using var conn = new SqliteConnection($"Data Source={db}");
    conn.Open();

    var cmd = conn.CreateCommand();
    cmd.CommandText = $@"
        SELECT *
        FROM {t1}
        INNER JOIN {t2}
        ON {t1}.{k1} = {t2}.{k2}";

    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        for (int i = 0; i < reader.FieldCount; i++)
            Console.Write($"{reader[i]} ");
        Console.WriteLine();
    }
}


// Union
static void Union(string db, string t1, string t2)
{
    Console.WriteLine("\n=== UNION ===");

    using var conn = new SqliteConnection($"Data Source={db}");
    conn.Open();

    var cmd = conn.CreateCommand();
    cmd.CommandText = $@"
        SELECT * FROM {t1}
        UNION
        SELECT * FROM {t2}";

    PrintReader(cmd);
}


// Intersect
static void Intersect(string db, string t1, string t2)
{
    Console.WriteLine("\n=== INTERSECT ===");

    using var conn = new SqliteConnection($"Data Source={db}");
    conn.Open();

    var cmd = conn.CreateCommand();
    cmd.CommandText = $@"
        SELECT * FROM {t1}
        INTERSECT
        SELECT * FROM {t2}";

    PrintReader(cmd);
}


// Difference
static void Difference(string db, string t1, string t2)
{
    Console.WriteLine("\n=== DIFFERENCE ===");

    using var conn = new SqliteConnection($"Data Source={db}");
    conn.Open();

    var cmd = conn.CreateCommand();
    cmd.CommandText = $@"
        SELECT * FROM {t1}
        EXCEPT
        SELECT * FROM {t2}";

    PrintReader(cmd);
}


// Cartesian Product
static void Product(string db, string t1, string t2)
{
    Console.WriteLine("\n=== PRODUCT ===");

    using var conn = new SqliteConnection($"Data Source={db}");
    conn.Open();

    var cmd = conn.CreateCommand();
    cmd.CommandText = $"SELECT * FROM {t1}, {t2}";

    PrintReader(cmd);
}


// GroupAvg
static void GroupAvg(string db, string table, string group, string value)
{
    Console.WriteLine("\n=== GROUP AVG ===");

    using var conn = new SqliteConnection($"Data Source={db}");
    conn.Open();

    var cmd = conn.CreateCommand();
    cmd.CommandText = $@"
        SELECT {group}, AVG({value})
        FROM {table}
        GROUP BY {group}";

    PrintReader(cmd);
}


// ========================================
// === ВСПОМОГАТЕЛЬНОЕ ====================
// ========================================

static void PrintReader(SqliteCommand cmd)
{
    using var reader = cmd.ExecuteReader();

    while (reader.Read())
    {
        for (int i = 0; i < reader.FieldCount; i++)
            Console.Write($"{reader[i]} ");
        Console.WriteLine();
    }
}
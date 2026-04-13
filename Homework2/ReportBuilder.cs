using System.Text;

/// <summary>
/// Построитель отчётов с использованием паттерна Fluent Interface.
/// </summary>
public class ReportBuilder
{
    private readonly DatabaseManager _db;

    private string _sql = string.Empty;
    private string _title = string.Empty;
    private string[] _headers = Array.Empty<string>();
    private int[] _widths = Array.Empty<int>();
    private bool _numbered;
    private string _footer = string.Empty;

    /// <summary>Конструктор принимает DatabaseManager для доступа к данным.</summary>
    public ReportBuilder(DatabaseManager db)
    {
        _db = db;
    }

    /// <summary>SQL-запрос для отчёта.</summary>
    public ReportBuilder Query(string sql)
    {
        _sql = sql;
        return this;
    }

    /// <summary>Заголовок отчёта.</summary>
    public ReportBuilder Title(string title)
    {
        _title = title;
        return this;
    }

    /// <summary>Названия колонок для отображения.</summary>
    public ReportBuilder Header(params string[] columns)
    {
        _headers = columns;
        return this;
    }

    /// <summary>Ширина каждой колонки в символах.</summary>
    public ReportBuilder ColumnWidths(params int[] widths)
    {
        _widths = widths;
        return this;
    }

    /// <summary>[Дополнительно] Включить нумерацию строк.</summary>
    public ReportBuilder Numbered()
    {
        _numbered = true;
        return this;
    }

    /// <summary>[Дополнительно] Добавить итоговую строку.</summary>
    public ReportBuilder Footer(string label)
    {
        _footer = label;
        return this;
    }

    /// <summary>Выполняет запрос и возвращает готовую строку отчёта.</summary>
    public string Build()
    {
        if (string.IsNullOrWhiteSpace(_sql))
            throw new InvalidOperationException("Для отчёта не задан SQL-запрос.");

        var (columns, rows) = _db.ExecuteQuery(_sql);
        var sb = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(_title))
        {
            sb.AppendLine();
            sb.AppendLine($"=== {_title} ===");
        }

        string[] displayHeaders = _headers.Length > 0 ? _headers : columns;
        int colCount = displayHeaders.Length;
        int[] widths = ResolveWidths(colCount);
        int numWidth = _numbered ? 5 : 0;

        if (_numbered)
            sb.Append("№".PadRight(numWidth));

        for (int i = 0; i < colCount; i++)
            sb.Append(Fit(displayHeaders[i], widths[i]).PadRight(widths[i]));

        sb.AppendLine();

        int totalWidth = numWidth + widths.Sum();
        sb.AppendLine(new string('─', totalWidth));

        for (int r = 0; r < rows.Count; r++)
        {
            if (_numbered)
                sb.Append((r + 1).ToString().PadRight(numWidth));

            for (int c = 0; c < colCount; c++)
            {
                string value = c < rows[r].Length ? rows[r][c] : string.Empty;
                sb.Append(Fit(value, widths[c]).PadRight(widths[c]));
            }

            sb.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(_footer))
        {
            sb.AppendLine(new string('─', totalWidth));
            sb.AppendLine($"{_footer}: {rows.Count}");
        }

        return sb.ToString();
    }

    /// <summary>Выводит отчёт в консоль.</summary>
    public void Print()
    {
        Console.Write(Build());
    }

    /// <summary>[Дополнительно] Сохраняет отчёт в текстовый файл.</summary>
    public void SaveToFile(string path)
    {
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(path, Build(), Encoding.UTF8);
        Console.WriteLine($"Отчёт сохранён в файл: {path}");
    }

    private int[] ResolveWidths(int colCount)
    {
        if (_widths.Length >= colCount)
            return _widths;

        var widths = new int[colCount];
        for (int i = 0; i < colCount; i++)
            widths[i] = 20;
        return widths;
    }

    private static string Fit(string value, int width)
    {
        if (value.Length <= width)
            return value;

        return width <= 1 ? value[..width] : value[..(width - 1)] + "…";
    }
}

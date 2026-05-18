using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

var validator = new LibraryItemValidator();
var logger = new FileLogger("library.log");
var reportPrinter = new LibraryReportPrinter();
var reportExporter = new TextFileReportExporter("report.txt");

var libraryService = new LibraryService(
    validator,
    logger,
    reportPrinter,
    reportExporter);

libraryService.AddBook("Чистый код", "Роберт Мартин", 2008);
libraryService.AddBook("Рефакторинг", "Мартин Фаулер", 1999);
libraryService.AddMagazine("Наука и жизнь", 5);

libraryService.PrintReport();

Console.WriteLine();
Console.WriteLine("Файлы library.log и report.txt созданы.");

abstract class LibraryItem
{
    public string Title { get; }

    protected LibraryItem(string title)
    {
        Title = title;
    }

    public abstract string GetDisplayInfo();
}

class Book : LibraryItem
{
    public string Author { get; }
    public int Year { get; }

    public Book(string title, string author, int year) : base(title)
    {
        Author = author;
        Year = year;
    }

    public override string GetDisplayInfo()
    {
        return $"Книга: {Title}, автор: {Author}, год: {Year}";
    }
}

class Magazine : LibraryItem
{
    public int IssueNumber { get; }

    public Magazine(string title, int issueNumber) : base(title)
    {
        IssueNumber = issueNumber;
    }

    public override string GetDisplayInfo()
    {
        return $"Журнал: {Title}, номер выпуска: {IssueNumber}";
    }
}

class LibraryItemValidator
{
    private const int MinBookYear = 1000;

    public void ValidateBook(string title, string author, int year)
    {
        ValidateTitle(title);
        ValidateAuthor(author);
        ValidateBookYear(year);
    }

    public void ValidateMagazine(string title, int issueNumber)
    {
        ValidateTitle(title);
        ValidateIssueNumber(issueNumber);
    }

    private void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Название не может быть пустым");
    }

    private void ValidateAuthor(string author)
    {
        if (string.IsNullOrWhiteSpace(author))
            throw new ArgumentException("Автор не может быть пустым");
    }

    private void ValidateBookYear(int year)
    {
        if (year < MinBookYear || year > DateTime.Now.Year)
            throw new ArgumentException("Некорректный год издания");
    }

    private void ValidateIssueNumber(int issueNumber)
    {
        if (issueNumber <= 0)
            throw new ArgumentException("Номер выпуска должен быть положительным");
    }
}

interface ILogger
{
    void Log(string message);
}

class FileLogger : ILogger
{
    private readonly string _filePath;

    public FileLogger(string filePath)
    {
        _filePath = filePath;
    }

    public void Log(string message)
    {
        File.AppendAllText(_filePath, $"{DateTime.Now:u}: {message}{Environment.NewLine}");
    }
}

class LibraryReportPrinter
{
    public void Print(IReadOnlyCollection<LibraryItem> items)
    {
        Console.WriteLine($"=== Отчёт: {items.Count} элементов ===");

        foreach (var item in items)
        {
            Console.WriteLine(item.GetDisplayInfo());
        }
    }
}

interface IReportExporter
{
    void Export(IReadOnlyCollection<LibraryItem> items);
}

class TextFileReportExporter : IReportExporter
{
    private readonly string _filePath;

    public TextFileReportExporter(string filePath)
    {
        _filePath = filePath;
    }

    public void Export(IReadOnlyCollection<LibraryItem> items)
    {
        var report = new StringBuilder();

        report.AppendLine($"Всего элементов: {items.Count}");
        report.AppendLine($"Дата: {DateTime.Now:u}");
        report.AppendLine();

        foreach (var item in items)
        {
            report.AppendLine(item.GetDisplayInfo());
        }

        File.WriteAllText(_filePath, report.ToString());
    }
}

class LibraryService
{
    private readonly List<LibraryItem> _items = new();

    private readonly LibraryItemValidator _validator;
    private readonly ILogger _logger;
    private readonly LibraryReportPrinter _reportPrinter;
    private readonly IReportExporter _reportExporter;

    public LibraryService(
        LibraryItemValidator validator,
        ILogger logger,
        LibraryReportPrinter reportPrinter,
        IReportExporter reportExporter)
    {
        _validator = validator;
        _logger = logger;
        _reportPrinter = reportPrinter;
        _reportExporter = reportExporter;
    }

    public void AddBook(string title, string author, int year)
    {
        _validator.ValidateBook(title, author, year);

        var book = new Book(title, author, year);
        _items.Add(book);

        _logger.Log($"Добавлена книга «{title}»");
    }

    public void AddMagazine(string title, int issueNumber)
    {
        _validator.ValidateMagazine(title, issueNumber);

        var magazine = new Magazine(title, issueNumber);
        _items.Add(magazine);

        _logger.Log($"Добавлен журнал «{title}»");
    }

    public void PrintReport()
    {
        _reportPrinter.Print(_items);
        _reportExporter.Export(_items);
    }
}
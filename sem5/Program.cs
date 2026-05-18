using System;
using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjectionSeminar;

#region Интерфейсы

interface ILogger
{
    void Log(string message);
}

interface IBookStorage
{
    void Save(string title, string author);

    void Remove(string title);
}

#endregion

#region Логгеры

class ConsoleLogger : ILogger
{
    public void Log(string message)
    {
        Console.WriteLine($"[LOG] {message}");
    }
}

class FileLogger : ILogger
{
    private string _filePath;

    public FileLogger(string filePath)
    {
        _filePath = filePath;
    }

    public void Log(string message)
    {
        File.AppendAllText(_filePath, $"[LOG] {message}\n");
    }
}

class NullLogger : ILogger
{
    public void Log(string message) { }
}

#endregion

#region Хранилище

class InMemoryBookStorage : IBookStorage
{
    private readonly ILogger _logger;
    private readonly List<string> _books = new();

    public InMemoryBookStorage(ILogger logger)
    {
        _logger = logger;
    }

    public void Save(string title, string author)
    {
        string book = $"«{title}» — {author}";

        _books.Add(book);

        _logger.Log($"[STORAGE] Сохранено: {book}");
    }
    public void Remove(string title)
    {
        string? foundBook = _books
            .FirstOrDefault(book => book.Contains(title));

        if (foundBook != null)
        {
            _books.Remove(foundBook);

            _logger.Log($"[STORAGE] Удалено: {foundBook}");
        }
        else
        {
            _logger.Log($"[STORAGE] Книга не найдена: {title}");
        }
    }
}

#endregion

#region Внедрение через конструктор

class BookCatalogService_DI_Constructor
{
    private readonly ILogger _logger;

    public BookCatalogService_DI_Constructor(ILogger logger)
    {
        _logger = logger;
    }

    public void AddBook(string title, string author)
    {
        _logger.Log($"Добавлена книга: «{title}» — {author}");
    }

    public void RemoveBook(string title)
    {
        _logger.Log($"Удалена книга: «{title}»");
    }
}

#endregion

#region Внедрение через свойство

class BookCatalogService_DI_Property
{
    public ILogger Logger { get; set; } = new NullLogger();

    public void AddBook(string title, string author)
    {
        Logger.Log($"Добавлена книга: «{title}» — {author}");
    }

    public void RemoveBook(string title)
    {
        Logger.Log($"Удалена книга: «{title}»");
    }
}

#endregion

#region Внедрение через метод

class BookCatalogService_DI_Method
{
    public void AddBook(string title, string author, ILogger logger)
    {
        logger.Log($"Добавлена книга: «{title}» — {author}");
    }
}

#endregion

#region Полноценный сервис

class BookCatalogService
{
    private readonly ILogger _logger;
    private readonly IBookStorage _storage;

    public BookCatalogService(
        ILogger logger,
        IBookStorage storage)
    {
        _logger = logger;
        _storage = storage;
    }

    public void AddBook(string title, string author)
    {
        _storage.Save(title, author);

        _logger.Log($"Добавлена книга: «{title}» — {author}");
    }

    public void RemoveBook(string title)
    {
        _storage.Remove(title);

        _logger.Log($"Удалена книга: «{title}»");
    }
}

#endregion

#region Main

class Program
{
    static void Main()
    {
        Console.WriteLine("=== DI через конструктор ===");

        BookCatalogService_DI_Constructor service1 =
            new BookCatalogService_DI_Constructor(
                new ConsoleLogger());

        service1.AddBook(
            "Евгений Онегин",
            "Пушкин");

        Console.WriteLine();


        Console.WriteLine("=== DI через свойство ===");

        BookCatalogService_DI_Property service2 =
            new BookCatalogService_DI_Property();

        service2.AddBook(
            "Тихая книга",
            "Автор");

        service2.Logger = new ConsoleLogger();

        service2.AddBook(
            "Сборник стихов",
            "Пушкин");

        Console.WriteLine();


        Console.WriteLine("=== DI через метод ===");

        BookCatalogService_DI_Method service3 =
            new BookCatalogService_DI_Method();

        service3.AddBook(
            "Руслан и Людмила",
            "Пушкин",
            new ConsoleLogger());

        Console.WriteLine();


        Console.WriteLine("=== Pure DI ===");

        ILogger logger = new ConsoleLogger();

        IBookStorage storage =
            new InMemoryBookStorage(logger);

        BookCatalogService service4 =
            new BookCatalogService(
                logger,
                storage);

        service4.AddBook(
            "Капитанская дочка",
            "Пушкин");

        Console.WriteLine();


        Console.WriteLine("=== DI Container ===");

        ServiceCollection services =
            new ServiceCollection();

        services.AddSingleton<ILogger, ConsoleLogger>();

        services.AddSingleton<
            IBookStorage,
            InMemoryBookStorage>();

        services.AddTransient<BookCatalogService>();

        ServiceProvider provider =
            services.BuildServiceProvider(
                new ServiceProviderOptions
                {
                    ValidateOnBuild = true
                });

        BookCatalogService service5 =
            provider.GetRequiredService<BookCatalogService>();

        service5.AddBook(
            "Пиковая дама",
            "Пушкин");

        Console.WriteLine();


        Console.WriteLine("=== Scoped пример ===");

        ServiceCollection scopedServices =
            new ServiceCollection();

        scopedServices.AddScoped<
            IBookStorage,
            InMemoryBookStorage>();

        scopedServices.AddSingleton<
            ILogger,
            ConsoleLogger>();

        ServiceProvider scopedProvider =
            scopedServices.BuildServiceProvider();

        using (IServiceScope scope1 =
               scopedProvider.CreateScope())
        {
            IBookStorage s1 =
                scope1.ServiceProvider
                    .GetRequiredService<IBookStorage>();

            IBookStorage s2 =
                scope1.ServiceProvider
                    .GetRequiredService<IBookStorage>();

            Console.WriteLine(
                $"Один объект в scope1: " +
                $"{ReferenceEquals(s1, s2)}");
        }

        using (IServiceScope scope2 =
               scopedProvider.CreateScope())
        {
            IBookStorage s3 =
                scope2.ServiceProvider
                    .GetRequiredService<IBookStorage>();

            Console.WriteLine(
                "Создан новый объект для scope2");
        }
    }
}

#endregion
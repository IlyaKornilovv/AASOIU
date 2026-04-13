/// <summary>
/// Кафедра (справочная таблица, сторона «один»)
/// </summary>
public class Chair
{
    /// <summary>Идентификатор кафедры</summary>
    public int Id { get; set; }

    /// <summary>Название кафедры</summary>
    public string Name { get; set; }

    /// <summary>Конструктор с параметрами</summary>
    public Chair(int id, string name)
    {
        Id = id;
        Name = name;
    }

    /// <summary>Конструктор по умолчанию</summary>
    public Chair() : this(0, string.Empty) { }

    public override string ToString() => $"[{Id}] {Name}";
}

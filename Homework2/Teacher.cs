/// <summary>
/// Преподаватель (основная таблица, сторона «много»)
/// </summary>
public class Teacher
{
    /// <summary>Идентификатор преподавателя</summary>
    public int Id { get; set; }

    /// <summary>Идентификатор кафедры (внешний ключ)</summary>
    public int ChairId { get; set; }

    /// <summary>ФИО преподавателя</summary>
    public string Name { get; set; }

    private int _publications;

    /// <summary>
    /// Количество научных публикаций (не может быть отрицательным)
    /// </summary>
    public int Publications
    {
        get => _publications;
        set
        {
            if (value < 0)
                throw new ArgumentException("Количество публикаций не может быть отрицательным.");
            _publications = value;
        }
    }

    /// <summary>Конструктор с параметрами</summary>
    public Teacher(int id, int chairId, string name, int publications)
    {
        Id = id;
        ChairId = chairId;
        Name = name;
        Publications = publications;
    }

    /// <summary>Конструктор по умолчанию</summary>
    public Teacher() : this(0, 0, string.Empty, 0) { }

    public override string ToString()
        => $"[{Id}] {Name}, кафедра #{ChairId}, публикаций: {Publications}";
}


internal class Teacher
{
    
    public int Id { get; set; }

    
    public int ChairId { get; set; }

    
    public string Name { get; set; }

    private int _publications;

    public int Publications
    {
        get => _publications;
        set
        {
            if (value < 0)
                throw new ArgumentException("Количество публикаций не может быть отрицательным");
            _publications = value;
        }
    }

    
    public Teacher(int id, int chairId, string name, int publications)
    {
        Id = id;
        ChairId = chairId;
        Name = name;
        Publications = publications;
    }

   
    public Teacher() : this(0, 0, "", 0) { }

    public override string ToString()
        => $"[{Id}] {Name}, кафедра #{ChairId}, публикаций: {Publications}";
}

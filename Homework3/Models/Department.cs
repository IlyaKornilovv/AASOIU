using System.ComponentModel.DataAnnotations;

namespace Homework3.Variant18.Models;


public sealed class Department
{
    
    public int Id { get; set; }

    
    [Required(ErrorMessage = "Введите название кафедры.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "Название кафедры должно содержать от 2 до 120 символов.")]
    [Display(Name = "Название кафедры")]
    public string Name { get; set; } = string.Empty;

    
    public ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
}

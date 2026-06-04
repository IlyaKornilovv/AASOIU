using System.ComponentModel.DataAnnotations;

namespace Homework3.Variant18.Models;


public sealed class Teacher
{
    
    public int Id { get; set; }

    
    [Display(Name = "Кафедра")]
    [Range(1, int.MaxValue, ErrorMessage = "Выберите кафедру.")]
    public int DepartmentId { get; set; }

    
    public Department? Department { get; set; }

    
    [Required(ErrorMessage = "Введите ФИО преподавателя.")]
    [StringLength(160, MinimumLength = 3, ErrorMessage = "ФИО должно содержать от 3 до 160 символов.")]
    [Display(Name = "ФИО преподавателя")]
    public string Name { get; set; } = string.Empty;

    
    [Range(0, int.MaxValue, ErrorMessage = "Количество публикаций не может быть отрицательным.")]
    [Display(Name = "Количество научных публикаций")]
    public int Publications { get; set; }
}

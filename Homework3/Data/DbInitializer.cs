using Homework3.Variant18.Models;

namespace Homework3.Variant18.Data;

internal static class DbInitializer
{
    internal static void Initialize(AppDbContext context)
    {
        context.Database.EnsureCreated();

        EnsureDepartments(context);
        EnsureTeachers(context);
    }

    private static void EnsureDepartments(AppDbContext context)
    {
        string[] names =
        [
            "Системы обработки информации и управления",
            "Программное обеспечение ЭВМ и информационные технологии",
            "Информационная безопасность",
            "Компьютерные системы и сети"
        ];

        foreach (string name in names)
        {
            if (!context.Departments.Any(department => department.Name == name))
            {
                context.Departments.Add(new Department { Name = name });
            }
        }

        context.SaveChanges();
    }

    private static void EnsureTeachers(AppDbContext context)
    {
        if (context.Teachers.Any())
        {
            return;
        }

        Dictionary<string, int> departmentIds = context.Departments
            .ToDictionary(department => department.Name, department => department.Id);

        context.Teachers.AddRange(
            new Teacher
            {
                DepartmentId = departmentIds["Системы обработки информации и управления"],
                Name = "Иванов Иван Иванович",
                Publications = 24
            },
            new Teacher
            {
                DepartmentId = departmentIds["Системы обработки информации и управления"],
                Name = "Петрова Анна Сергеевна",
                Publications = 18
            },
            new Teacher
            {
                DepartmentId = departmentIds["Системы обработки информации и управления"],
                Name = "Сидоров Павел Олегович",
                Publications = 31
            },
            new Teacher
            {
                DepartmentId = departmentIds["Программное обеспечение ЭВМ и информационные технологии"],
                Name = "Смирнова Елена Андреевна",
                Publications = 12
            },
            new Teacher
            {
                DepartmentId = departmentIds["Программное обеспечение ЭВМ и информационные технологии"],
                Name = "Кузнецов Максим Игоревич",
                Publications = 27
            },
            new Teacher
            {
                DepartmentId = departmentIds["Программное обеспечение ЭВМ и информационные технологии"],
                Name = "Попова Мария Викторовна",
                Publications = 16
            },
            new Teacher
            {
                DepartmentId = departmentIds["Информационная безопасность"],
                Name = "Васильев Артём Николаевич",
                Publications = 22
            },
            new Teacher
            {
                DepartmentId = departmentIds["Информационная безопасность"],
                Name = "Морозова Ольга Дмитриевна",
                Publications = 14
            },
            new Teacher
            {
                DepartmentId = departmentIds["Информационная безопасность"],
                Name = "Новиков Денис Романович",
                Publications = 9
            },
            new Teacher
            {
                DepartmentId = departmentIds["Компьютерные системы и сети"],
                Name = "Фёдорова Наталья Алексеевна",
                Publications = 20
            },
            new Teacher
            {
                DepartmentId = departmentIds["Компьютерные системы и сети"],
                Name = "Волков Сергей Михайлович",
                Publications = 35
            },
            new Teacher
            {
                DepartmentId = departmentIds["Компьютерные системы и сети"],
                Name = "Алексеева Ирина Павловна",
                Publications = 11
            });

        context.SaveChanges();
    }
}

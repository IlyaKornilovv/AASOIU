using Homework3.Variant18.Data;
using Homework3.Variant18.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Homework3.Variant18.Controllers;


public sealed class ReportsController : Controller
{
    
    public IActionResult Index()
    {
        using var context = new AppDbContext();

        List<TeacherReportRow> teachers = context.Teachers
            .Include(teacher => teacher.Department)
            .AsNoTracking()
            .OrderBy(teacher => teacher.Name)
            .Select(teacher => new TeacherReportRow
            {
                TeacherName = teacher.Name,
                DepartmentName = teacher.Department!.Name,
                Publications = teacher.Publications
            })
            .ToList();

        List<DepartmentCountReportRow> departmentCounts = context.Teachers
            .AsNoTracking()
            .GroupBy(teacher => teacher.Department!.Name)
            .Select(group => new DepartmentCountReportRow
            {
                DepartmentName = group.Key,
                TeacherCount = group.Count()
            })
            .OrderBy(row => row.DepartmentName)
            .ToList();

        List<DepartmentAverageReportRow> departmentAverages = context.Teachers
            .AsNoTracking()
            .GroupBy(teacher => teacher.Department!.Name)
            .Select(group => new DepartmentAverageReportRow
            {
                DepartmentName = group.Key,
                AveragePublications = group.Average(teacher => teacher.Publications)
            })
            .OrderByDescending(row => row.AveragePublications)
            .ToList();

        return View(new ReportsViewModel
        {
            Teachers = teachers,
            DepartmentCounts = departmentCounts,
            DepartmentAverages = departmentAverages
        });
    }
}

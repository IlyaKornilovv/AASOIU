using Homework3.Variant18.Data;
using Homework3.Variant18.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Homework3.Variant18.Controllers;


public sealed class TeachersController : Controller
{
    
    public IActionResult Index()
    {
        using var context = new AppDbContext();
        List<Teacher> teachers = context.Teachers
            .Include(teacher => teacher.Department)
            .AsNoTracking()
            .OrderBy(teacher => teacher.Name)
            .ToList();

        return View(teachers);
    }

    
    public IActionResult Create()
    {
        using var context = new AppDbContext();
        ViewBag.Departments = BuildDepartmentOptions(context);
        return View(new Teacher());
    }

    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create([Bind("Name,Publications,DepartmentId")] Teacher teacher)
    {
        teacher.Name = teacher.Name?.Trim() ?? string.Empty;

        using var context = new AppDbContext();
        ValidateDepartment(context, teacher.DepartmentId);
        if (!ModelState.IsValid)
        {
            ViewBag.Departments = BuildDepartmentOptions(context, teacher.DepartmentId);
            return View(teacher);
        }

        context.Teachers.Add(teacher);
        context.SaveChanges();
        TempData["SuccessMessage"] = "Преподаватель добавлен.";
        return RedirectToAction(nameof(Index));
    }

    
    public IActionResult Edit(int id)
    {
        using var context = new AppDbContext();
        Teacher? teacher = context.Teachers.Find(id);
        if (teacher is null)
        {
            return NotFound();
        }

        ViewBag.Departments = BuildDepartmentOptions(context, teacher.DepartmentId);
        return View(teacher);
    }

    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, [Bind("Id,Name,Publications,DepartmentId")] Teacher teacher)
    {
        if (id != teacher.Id)
        {
            return BadRequest();
        }

        teacher.Name = teacher.Name?.Trim() ?? string.Empty;

        using var context = new AppDbContext();
        ValidateDepartment(context, teacher.DepartmentId);
        if (!ModelState.IsValid)
        {
            ViewBag.Departments = BuildDepartmentOptions(context, teacher.DepartmentId);
            return View(teacher);
        }

        context.Teachers.Update(teacher);
        context.SaveChanges();
        TempData["SuccessMessage"] = "Изменения сохранены.";
        return RedirectToAction(nameof(Index));
    }

    
    public IActionResult Delete(int id)
    {
        using var context = new AppDbContext();
        Teacher? teacher = context.Teachers
            .Include(item => item.Department)
            .AsNoTracking()
            .FirstOrDefault(item => item.Id == id);

        return teacher is null ? NotFound() : View(teacher);
    }

    
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        using var context = new AppDbContext();
        Teacher? teacher = context.Teachers.Find(id);
        if (teacher is null)
        {
            return NotFound();
        }

        context.Teachers.Remove(teacher);
        context.SaveChanges();
        TempData["SuccessMessage"] = "Преподаватель удалён.";
        return RedirectToAction(nameof(Index));
    }

    private void ValidateDepartment(AppDbContext context, int departmentId)
    {
        if (!context.Departments.Any(department => department.Id == departmentId))
        {
            ModelState.AddModelError(nameof(Teacher.DepartmentId), "Выберите существующую кафедру.");
        }
    }

    private static List<SelectListItem> BuildDepartmentOptions(AppDbContext context, int? selectedDepartmentId = null)
    {
        return context.Departments
            .AsNoTracking()
            .OrderBy(department => department.Name)
            .Select(department => new SelectListItem
            {
                Value = department.Id.ToString(),
                Text = department.Name,
                Selected = selectedDepartmentId.HasValue && department.Id == selectedDepartmentId.Value
            })
            .ToList();
    }
}

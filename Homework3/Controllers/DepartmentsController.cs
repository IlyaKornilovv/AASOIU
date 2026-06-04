using Homework3.Variant18.Data;
using Homework3.Variant18.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Homework3.Variant18.Controllers;


public sealed class DepartmentsController : Controller
{
    
    public IActionResult Index()
    {
        using var context = new AppDbContext();
        List<Department> departments = context.Departments
            .AsNoTracking()
            .OrderBy(department => department.Name)
            .ToList();

        return View(departments);
    }

    
    public IActionResult Create() => View(new Department());

    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create([Bind("Name")] Department department)
    {
        department.Name = department.Name?.Trim() ?? string.Empty;

        if (!ModelState.IsValid)
        {
            return View(department);
        }

        using var context = new AppDbContext();
        if (context.Departments.Any(item => item.Name == department.Name))
        {
            ModelState.AddModelError(nameof(Department.Name), "Кафедра с таким названием уже существует.");
            return View(department);
        }

        context.Departments.Add(department);
        context.SaveChanges();
        TempData["SuccessMessage"] = "Кафедра добавлена.";
        return RedirectToAction(nameof(Index));
    }

    
    public IActionResult Edit(int id)
    {
        using var context = new AppDbContext();
        Department? department = context.Departments.Find(id);
        return department is null ? NotFound() : View(department);
    }

    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, [Bind("Id,Name")] Department department)
    {
        if (id != department.Id)
        {
            return BadRequest();
        }

        department.Name = department.Name?.Trim() ?? string.Empty;
        if (!ModelState.IsValid)
        {
            return View(department);
        }

        using var context = new AppDbContext();
        if (context.Departments.Any(item => item.Name == department.Name && item.Id != department.Id))
        {
            ModelState.AddModelError(nameof(Department.Name), "Кафедра с таким названием уже существует.");
            return View(department);
        }

        context.Departments.Update(department);
        context.SaveChanges();
        TempData["SuccessMessage"] = "Изменения сохранены.";
        return RedirectToAction(nameof(Index));
    }

    
    public IActionResult Delete(int id)
    {
        using var context = new AppDbContext();
        Department? department = context.Departments.AsNoTracking().FirstOrDefault(item => item.Id == id);
        if (department is null)
        {
            return NotFound();
        }

        ViewBag.HasTeachers = context.Teachers.Any(teacher => teacher.DepartmentId == id);
        return View(department);
    }

    
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        using var context = new AppDbContext();
        Department? department = context.Departments.Find(id);
        if (department is null)
        {
            return NotFound();
        }

        if (context.Teachers.Any(teacher => teacher.DepartmentId == id))
        {
            TempData["ErrorMessage"] = "Удаление запрещено: у кафедры есть связанные преподаватели.";
            return RedirectToAction(nameof(Index));
        }

        context.Departments.Remove(department);
        context.SaveChanges();
        TempData["SuccessMessage"] = "Кафедра удалена.";
        return RedirectToAction(nameof(Index));
    }
}

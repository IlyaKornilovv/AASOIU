using System.Diagnostics;
using Homework3.Variant18.Models;
using Microsoft.AspNetCore.Mvc;

namespace Homework3.Variant18.Controllers;


public sealed class HomeController : Controller
{
    
    public IActionResult Index() => View();

    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel
    {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
    });
}

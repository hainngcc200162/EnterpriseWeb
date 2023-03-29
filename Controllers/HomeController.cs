using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EnterpriseWeb.Models;
using EnterpriseWeb.Areas.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


namespace EnterpriseWeb.Controllers;

[Authorize(Roles = "Admin, Staff, QAManager, QACoordinator")]
public class HomeController : Controller
{
    private readonly EnterpriseWebIdentityDbContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger, EnterpriseWebIdentityDbContext context)
    {
        _context = context;        
        _logger = logger;
    }

    public async Task<IActionResult> Index(string sortOrder, string searchString)
    {
        ViewData["MostView"] = String.IsNullOrEmpty(sortOrder) ? "mostView" : "";
        ViewData["MostRating"] = sortOrder == "mostView" ? "mostRating" : "mostRating";
        ViewData["Department"] = sortOrder == "mostView" ? "mostRating" : "department";
        ViewData["CurrentFilter"] = searchString;
        var enterpriseWebContext = from e in _context.Idea
                       .Include(i => i.ClosureDate)
                       .Include(i => i.Department)
                       .Include(i => i.Viewings)
                       .Include(i => i.IdeaUser)
                       .Include(i => i.Ratings)
                       .Include(i => i.Comments)
                                   where e.Status == 1
                                   select e;

        if (!String.IsNullOrEmpty(searchString))
        {
            enterpriseWebContext = enterpriseWebContext.Where(e => e.Title.Contains(searchString)
                                    || e.Description.Contains(searchString)
                                    || e.Department.Name.Contains(searchString));

        }

        switch (sortOrder)
        {
            case "mostView":
                enterpriseWebContext = enterpriseWebContext.OrderByDescending(e => e.Viewings.Count);
                break;
            case "mostRating":
                enterpriseWebContext = enterpriseWebContext.OrderByDescending(e => e.Ratings
                    .Where(r => r.IdeaID == e.Id)
                    .AsEnumerable()
                    .Sum(r => r.RatingUp));
                break;
            case "department":
                enterpriseWebContext = enterpriseWebContext.OrderByDescending(e => e.Department.Name);
                break;
            default:
                break;
        }
        ViewData["DepartmentID"] = new SelectList(_context.Set<Department>(), "Name", "Name");
        ViewBag.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return View(await enterpriseWebContext.AsNoTracking().ToListAsync());
    }
    public IActionResult About()
    {
        return View();
    }
    public IActionResult Contact()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [Authorize(Roles = "Admin")]
    public IActionResult AdminHomePage()
    {
        ViewBag.Layout = "_ViewAdmin";
        return View();
    }

    [Authorize(Roles = "QAManager")]
    public IActionResult QAManagerHomePage()
    {
        ViewBag.Layout = "_QAManager";
        return View();
    }
    [Authorize(Roles = "QACoordinator")]
    public IActionResult QACoordinatorHomePage()
    {
        ViewBag.Layout = "_QACoordinator";
        return View();
    }    
}

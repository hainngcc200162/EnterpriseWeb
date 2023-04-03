using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EnterpriseWeb.Models;
using EnterpriseWeb.Areas.Identity.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace EnterpriseWeb.Controllers
{
    public class DepartmentController : Controller
    {
        private string Layout = "_ViewAdmin";
        private string Layout2 = "_QAManager";
        private readonly EnterpriseWebIdentityDbContext _context;

        public DepartmentController(EnterpriseWebIdentityDbContext context)
        {
            _context = context;
        }
        [Authorize(Roles = "Admin")]
        // GET: Department
        public async Task<IActionResult> Index(string currentFilter, string searchString, int? pageNumber)
        {
            ViewBag.Layout = Layout;
            // var enterpriseWebContext = _context.Department.Include(d => d.QACoordinator);
            // return View(await enterpriseWebContext.ToListAsync());
            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            ViewData["CurrentFilter"] = searchString;
            // Retrieve message from TempData, if any
            var message = TempData["message"]?.ToString();
            var messageClass = TempData["messageClass"]?.ToString();
            ViewData["message"] = message;
            ViewData["messageClass"] = messageClass;
            var department = from m in _context.Department select m;
            if (!String.IsNullOrEmpty(searchString))
            {
                department = department.Where(s => s.Name.Contains(searchString));
            }
            int pageSize = 5;
            return View(await PaginatedList<Department>.CreateAsync(department.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        [Authorize(Roles = "QAManager")]
        //ViewQA in this is sued by QA Manager 
        public async Task<IActionResult> ViewQA(string currentFilter, string searchString, int? pageNumber)
        {
            ViewBag.Layout = Layout2;
            // var enterpriseWebContext = _context.Department.Include(d => d.QACoordinator);
            // return View(await enterpriseWebContext.ToListAsync());
            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            ViewData["CurrentFilter"] = searchString;
            var department = from m in _context.Department select m;
            if (!String.IsNullOrEmpty(searchString))
            {
                department = department.Where(s => s.Name.Contains(searchString));
            }
            int pageSize = 5;
            return View(await PaginatedList<Department>.CreateAsync(department.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        [Authorize(Roles = "Admin, QAManager")]
        // GET: Department/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Department
                .FirstOrDefaultAsync(m => m.Id == id);
            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        [Authorize(Roles = "Admin")]
        // GET: Department/Create
        public IActionResult Create()
        {
            ViewBag.Layout = Layout;
            return View();
        }

        // POST: Department/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description")] Department department)
        {
            if (ModelState.IsValid)
            {
                var existingDepartment = _context.Department.FirstOrDefault(i => i.Name == department.Name);

                if (existingDepartment != null)
                {
                    ModelState.AddModelError("Name", "Department with this name already exists.");
                    ViewBag.Layout = Layout;
                    return View(department);
                }

                _context.Add(department);
                await _context.SaveChangesAsync();
                TempData["message"] = "Create successfully.";
                TempData["messageClass"] = "alert-success";
                return RedirectToAction(nameof(Index));
            }

            return View(department);
        }
        [Authorize(Roles = "Admin")]
        // GET: Department/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            ViewBag.Layout = Layout;
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Department.FindAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            return View(department);
        }

        // POST: Department/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Department department)
        {
            if (id != department.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingDepartment = _context.Department.FirstOrDefault(i => i.Name == department.Name && i.Id != department.Id);

                    if (existingDepartment != null)
                    {
                        ModelState.AddModelError("Name", "Department with this name already exists.");
                        ViewBag.Layout = Layout;
                        return View(department);
                    }

                    _context.Update(department);
                    await _context.SaveChangesAsync();
                    TempData["message"] = "Edit successfully.";
                    TempData["messageClass"] = "alert-success";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DepartmentExists(department.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(department);
        }

        [Authorize(Roles = "Admin")]
        // GET: Department/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            ViewBag.Layout = Layout2;
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Department
                .FirstOrDefaultAsync(m => m.Id == id);
            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            ViewBag.Layout = Layout2;
            var department = await _context.Department.FindAsync(id);
            var users = await _context.Users.Where(o => o.Department == department).ToListAsync();

            // Check if the department is being used by any user
            if (users.Any())
            {
                TempData["message"] = "This department cannot be deleted because it has been assigned to one or more users.";
                TempData["messageClass"] = "alert-danger";
                return RedirectToAction(nameof(Index));
            }

            _context.Department.Remove(department);
            await _context.SaveChangesAsync();

            TempData["message"] = "Department deleted successfully.";
            TempData["messageClass"] = "alert-success";
            return RedirectToAction(nameof(Index));
        }


        private bool DepartmentExists(int id)
        {
            return _context.Department.Any(e => e.Id == id);
        }
    }
}

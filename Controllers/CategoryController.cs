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
    public class CategoryController : Controller
    {
        private string Layout = "_ViewAdmin";
        private string Layout1 = "_QACoordinator";
        private string Layout2 = "_QAManager";

        private readonly EnterpriseWebIdentityDbContext _context;

        public CategoryController(EnterpriseWebIdentityDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        // GET: Category
        public async Task<IActionResult> Index()
        {
            ViewBag.Layout = Layout;
            return View(await _context.Category.ToListAsync());
        }

        [Authorize(Roles = "QAManager, QACoordinator")]
        // ViewQA is Index of QA Manager 
        public async Task<IActionResult> ViewQA(string currentFilter, string searchString, int? pageNumber)
        {
            ViewBag.Layout = Layout2;
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

            var ideas = from m in _context.Category select m;
            if (!String.IsNullOrEmpty(searchString))
            {
                ideas = ideas.Where(s => s.Name.Contains(searchString));
            }
            int pageSize = 5;

            // Pass message and messageClass to View
            ViewData["message"] = message;
            ViewData["messageClass"] = messageClass;

            return View(await PaginatedList<Category>.CreateAsync(ideas.AsNoTracking(), pageNumber ?? 1, pageSize));
        }


        [Authorize(Roles = "Admin, QAManager, QACoordinator")]
        // ViewCategory is The Index of Admin Page
        public async Task<IActionResult> ViewCategory(string currentFilter, string searchString, int? pageNumber)
        {
            ViewBag.Layout = Layout;
            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            ViewData["CurrentFilter"] = searchString;
            var ideas = from m in _context.Category select m;
            if (!String.IsNullOrEmpty(searchString))
            {
                ideas = ideas.Where(s => s.Name.Contains(searchString));
            }
            int pageSize = 5;
            return View(await PaginatedList<Category>.CreateAsync(ideas.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        [Authorize(Roles = "QAManager, QACoordinator")]
        // ViewCategoryQA is the index of QACoordinator
        public async Task<IActionResult> ViewCategoryQA(string currentFilter, string searchString, int? pageNumber)
        {
            ViewBag.Layout = Layout1;
            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            ViewData["CurrentFilter"] = searchString;
            var ideas = from m in _context.Category select m;
            if (!String.IsNullOrEmpty(searchString))
            {
                ideas = ideas.Where(s => s.Name.Contains(searchString));
            }
            int pageSize = 5;
            return View(await PaginatedList<Category>.CreateAsync(ideas.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        [Authorize(Roles = "Admin, QAManager")]
        // GET: Category/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Category
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [Authorize(Roles = "QAManager")]
        // GET: Category/Create
        // Create is in QA Manager
        public IActionResult Create()
        {
            ViewBag.Layout = Layout2;
            return View();
        }

        // POST: Category/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Status")] Category category)
        {
            ViewBag.Layout = Layout2;
            if (ModelState.IsValid)
            {
                var existingCategory = _context.Category.FirstOrDefault(c => c.Name == category.Name);

                if (existingCategory != null)
                {
                    ModelState.AddModelError("Name", "Category with this name already exists.");
                    return View(category);
                }

                TempData["message"] = "Create successfully.";
                TempData["messageClass"] = "alert-success";
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ViewQA));
            }

            return View(category);
        }

        [Authorize(Roles = "QAManager")]
        // GET: Category/Edit/5
        // Edit is in QA Manager
        public async Task<IActionResult> Edit(int? id)
        {
            ViewBag.Layout = Layout2;
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Category.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }
        // POST: Category/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Status")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    TempData["message"] = "Edit successfully.";
                    TempData["messageClass"] = "alert-success";
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(ViewQA));
            }
            return View(category);
        }

        [Authorize(Roles = "QAManager")]
        // GET: Category/Delete/5
        //Delete is the page of QA Manager
        public async Task<IActionResult> Delete(int? id)
        {
            ViewBag.Layout = Layout2;
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Category
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            ViewBag.Layout = Layout2;
            var category = await _context.Category.FindAsync(id);
            var ideacategories = await _context.IdeaCategory.Where(o => o.Category == category).ToListAsync();

            // Check if the category is being used by any idea
            if (ideacategories.Any())
            {
                TempData["message"] = "This category cannot be deleted because it has been used by one or more ideas.";
                TempData["messageClass"] = "alert-danger";
                return RedirectToAction(nameof(ViewQA));
            }

            foreach (var ideacategory in ideacategories)
            {
                _context.IdeaCategory.Remove(ideacategory);
            }

            _context.Category.Remove(category);
            await _context.SaveChangesAsync();

            TempData["message"] = "Category deleted successfully.";
            TempData["messageClass"] = "alert-success";
            return RedirectToAction(nameof(ViewQA));
        }



        private bool CategoryExists(int id)
        {
            return _context.Category.Any(e => e.Id == id);
        }
    }
}

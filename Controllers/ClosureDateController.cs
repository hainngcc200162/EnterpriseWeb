using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EnterpriseWeb.Models;
using EnterpriseWeb.Areas.Identity.Data;

namespace EnterpriseWeb.Controllers
{
    public class ClosureDateController : Controller
    {
        private string Layout = "_ViewAdmin";
        private readonly EnterpriseWebIdentityDbContext _context;

        public ClosureDateController(EnterpriseWebIdentityDbContext context)
        {
            _context = context;
        }

        // GET: ClosureDate
        public async Task<IActionResult> Index(string currentFilter, string searchString, int? pageNumber)
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
            var closuredate = from m in _context.ClosureDate select m;
            if (!String.IsNullOrEmpty(searchString))
            {
                closuredate = closuredate.Where(s => s.Name.Contains(searchString));
            }
            int pageSize = 5;
            return View(await PaginatedList<ClosureDate>.CreateAsync(closuredate.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: ClosureDate/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var closureDate = await _context.ClosureDate
                .FirstOrDefaultAsync(m => m.Id == id);
            if (closureDate == null)
            {
                return NotFound();
            }

            return View(closureDate);
        }

        // GET: ClosureDate/Create
        public IActionResult Create()
        {
            ViewBag.Layout = Layout;
            return View();
        }

        // POST: ClosureDate/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,AcademicYear,ClousureDate,FinalDate")] ClosureDate closureDate)
        {
            if (ModelState.IsValid)
            {
                _context.Add(closureDate);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(closureDate);
        }

        // GET: ClosureDate/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            ViewBag.Layout = Layout;
            if (id == null)
            {
                return NotFound();
            }

            var closureDate = await _context.ClosureDate.FindAsync(id);
            if (closureDate == null)
            {
                return NotFound();
            }
            return View(closureDate);
        }

        // POST: ClosureDate/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,AcademicYear,ClousureDate,FinalDate")] ClosureDate closureDate)
        {
            if (id != closureDate.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(closureDate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClosureDateExists(closureDate.Id))
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
            return View(closureDate);
        }

        // GET: ClosureDate/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            ViewBag.Layout = Layout;
            if (id == null)
            {
                return NotFound();
            }

            var closureDate = await _context.ClosureDate
                .FirstOrDefaultAsync(m => m.Id == id);
            if (closureDate == null)
            {
                return NotFound();
            }

            return View(closureDate);
        }

        // POST: ClosureDate/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var closureDate = await _context.ClosureDate.FindAsync(id);
            _context.ClosureDate.Remove(closureDate);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ClosureDateExists(int id)
        {
            return _context.ClosureDate.Any(e => e.Id == id);
        }
    }
}

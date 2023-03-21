using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EnterpriseWeb.Areas.Identity.Data;
using EnterpriseWeb.Models;

namespace EnterpriseWeb.Controllers
{   
    
    public class ViewingController : Controller
    {
        private readonly EnterpriseWebIdentityDbContext _context;

        public ViewingController(EnterpriseWebIdentityDbContext context)
        {
            _context = context;
        }

        // GET: Viewing
        public async Task<IActionResult> Index()
        {
            var enterpriseWebIdentityDbContext = _context.Viewing.Include(v => v.Idea);
            return View(await enterpriseWebIdentityDbContext.ToListAsync());
        }

        // GET: Viewing/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var viewing = await _context.Viewing
                .Include(v => v.Idea)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (viewing == null)
            {
                return NotFound();
            }

            return View(viewing);
        }

        // GET: Viewing/Create
        public IActionResult Create()
        {
            ViewData["IdeaId"] = new SelectList(_context.Idea, "Id", "Id");
            return View();
        }

        // POST: Viewing/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Count,ViewDate,UserId,IdeaId")] Viewing viewing)
        {
            if (ModelState.IsValid)
            {
                _context.Add(viewing);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdeaId"] = new SelectList(_context.Idea, "Id", "Id", viewing.IdeaId);
            return View(viewing);
        }

        // GET: Viewing/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var viewing = await _context.Viewing.FindAsync(id);
            if (viewing == null)
            {
                return NotFound();
            }
            ViewData["IdeaId"] = new SelectList(_context.Idea, "Id", "Id", viewing.IdeaId);
            return View(viewing);
        }

        // POST: Viewing/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Count,ViewDate,UserId,IdeaId")] Viewing viewing)
        {
            if (id != viewing.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(viewing);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ViewingExists(viewing.Id))
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
            ViewData["IdeaId"] = new SelectList(_context.Idea, "Id", "Id", viewing.IdeaId);
            return View(viewing);
        }

        // GET: Viewing/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var viewing = await _context.Viewing
                .Include(v => v.Idea)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (viewing == null)
            {
                return NotFound();
            }

            return View(viewing);
        }

        // POST: Viewing/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var viewing = await _context.Viewing.FindAsync(id);
            _context.Viewing.Remove(viewing);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ViewingExists(int id)
        {
            return _context.Viewing.Any(e => e.Id == id);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EnterpriseWeb.Models;

namespace EnterpriseWeb.Controllers
{
    public class RatingController : Controller
    {
        private readonly EnterpriseWebContext _context;

        public RatingController(EnterpriseWebContext context)
        {
            _context = context;
        }

        // GET: Rating
        public async Task<IActionResult> Index()
        {
            var enterpriseWebContext = _context.Rating.Include(r => r.Idea).Include(r => r.User);
            return View(await enterpriseWebContext.ToListAsync());
        }

        // GET: Rating/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rating = await _context.Rating
                .Include(r => r.Idea)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rating == null)
            {
                return NotFound();
            }

            return View(rating);
        }

        // GET: Rating/Create
        public IActionResult Create()
        {
            ViewData["IdeaID"] = new SelectList(_context.Idea, "Id", "Id");
            ViewData["UserId"] = new SelectList(_context.User, "Id", "Id");
            return View();
        }

        // POST: Rating/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdeaID,UserId,RatingUp,RatingDown,SubmitionDate")] Rating rating)
        {
            if (ModelState.IsValid)
            {
                _context.Add(rating);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdeaID"] = new SelectList(_context.Idea, "Id", "Id", rating.IdeaID);
            ViewData["UserId"] = new SelectList(_context.User, "Id", "Id", rating.UserId);
            return View(rating);
        }

        // GET: Rating/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rating = await _context.Rating.FindAsync(id);
            if (rating == null)
            {
                return NotFound();
            }
            ViewData["IdeaID"] = new SelectList(_context.Idea, "Id", "Id", rating.IdeaID);
            ViewData["UserId"] = new SelectList(_context.User, "Id", "Id", rating.UserId);
            return View(rating);
        }

        // POST: Rating/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdeaID,UserId,RatingUp,RatingDown,SubmitionDate")] Rating rating)
        {
            if (id != rating.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rating);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RatingExists(rating.Id))
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
            ViewData["IdeaID"] = new SelectList(_context.Idea, "Id", "Id", rating.IdeaID);
            ViewData["UserId"] = new SelectList(_context.User, "Id", "Id", rating.UserId);
            return View(rating);
        }

        // GET: Rating/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rating = await _context.Rating
                .Include(r => r.Idea)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rating == null)
            {
                return NotFound();
            }

            return View(rating);
        }

        // POST: Rating/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rating = await _context.Rating.FindAsync(id);
            _context.Rating.Remove(rating);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RatingExists(int id)
        {
            return _context.Rating.Any(e => e.Id == id);
        }
    }
}

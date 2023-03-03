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
    public class IdeaController : Controller
    {
        private readonly EnterpriseWebIdentityDbContext _context;

        public IdeaController(EnterpriseWebIdentityDbContext context)
        {
            _context = context;
        }

        // GET: Idea
        public async Task<IActionResult> Index()
        {
            var enterpriseWebContext = _context.Idea.Include(i => i.ClosureDate).Include(i => i.Department);
            return View(await enterpriseWebContext.ToListAsync());
        }
        public async Task<IActionResult> Search(string bookCategory, string search){
            IQueryable<string> bookQuery = from m in _context.IdeaCategory orderby m.Idea.Title select m.Idea.Title;
            var books = from m in _context.IdeaCategory.Include(n => n.Idea).Include(a => a.Category) select m;
            var FPTBOOK_STOREIdentityDbContext = from m in _context.IdeaCategory.Include(a => a.Idea).Include(b => b.Category) select m;
            
            if (!string.IsNullOrEmpty(search))
            {
                books = books.Where(s => s.Idea.Title!.Contains(search));
            }
          
            var bookcategoryVM = new IdeaCategoryView
            {
                IdeaCategories = await books.ToListAsync(),
            };
            return View(bookcategoryVM);

        }
        public async Task<IActionResult> SearchCategory(string bookCategory, string search){
            IQueryable<string> bookQuery = from m in _context.IdeaCategory orderby m.Idea.Title select m.Idea.Title;
            var books = from m in _context.IdeaCategory.Include(n => n.Idea).Include(a => a.Category) select m;
            var FPTBOOK_STOREIdentityDbContext = from m in _context.IdeaCategory.Include(a => a.Idea).Include(b => b.Category) select m;
            
            if (!string.IsNullOrEmpty(search))
            {
                books = books.Where(s => s.Category.Name!.Contains(search));
            }

          
            var categorysearchVM = new CategorySearchView
            {
                IdeaCategories = await books.ToListAsync(),
            };
            return View(categorysearchVM);

        }

        // GET: Idea/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var idea = await _context.Idea
                .Include(i => i.ClosureDate)
                .Include(i => i.Department)
                // .Include(i => i.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (idea == null)
            {
                return NotFound();
            }

            return View(idea);
        }

        // GET: Idea/Create
        public IActionResult Create()
        {
            ViewData["ClosureDateID"] = new SelectList(_context.Set<ClosureDate>(), "Id", "Id");
            ViewData["DepartmentID"] = new SelectList(_context.Set<Department>(), "Id", "Id");
            // ViewData["UserID"] = new SelectList(_context.User, "Id", "Id");
            return View();
        }

        // POST: Idea/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,UserID,SupportingDocuments,DepartmentID,ClosureDateID")] Idea idea)
        {
            if (ModelState.IsValid)
            {
                idea.SubmissionDate = DateTime.Now;
                _context.Add(idea);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClosureDateID"] = new SelectList(_context.Set<ClosureDate>(), "Id", "Id", idea.ClosureDateID);
            ViewData["DepartmentID"] = new SelectList(_context.Set<Department>(), "Id", "Id", idea.DepartmentID);
            // ViewData["UserID"] = new SelectList(_context.User, "Id", "Id", idea.UserID);
            return View(idea);
        }

        // GET: Idea/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var idea = await _context.Idea.FindAsync(id);
            if (idea == null)
            {
                return NotFound();
            }
            ViewData["ClosureDateID"] = new SelectList(_context.Set<ClosureDate>(), "Id", "Id", idea.ClosureDateID);
            ViewData["DepartmentID"] = new SelectList(_context.Set<Department>(), "Id", "Id", idea.DepartmentID);
            // ViewData["UserID"] = new SelectList(_context.User, "Id", "Id", idea.UserID);
            return View(idea);
        }

        // POST: Idea/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,UserID,SupportingDocuments,DepartmentID,ClosureDateID")] Idea idea)
        {
            if (id != idea.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    idea.SubmissionDate = DateTime.Now;
                    _context.Update(idea);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IdeaExists(idea.Id))
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
            ViewData["ClosureDateID"] = new SelectList(_context.Set<ClosureDate>(), "Id", "Id", idea.ClosureDateID);
            ViewData["DepartmentID"] = new SelectList(_context.Set<Department>(), "Id", "Id", idea.DepartmentID);
            // ViewData["UserID"] = new SelectList(_context.User, "Id", "Id", idea.UserID);
            return View(idea);
        }

        // GET: Idea/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var idea = await _context.Idea
                .Include(i => i.ClosureDate)
                .Include(i => i.Department)
                // .Include(i => i.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (idea == null)
            {
                return NotFound();
            }

            return View(idea);
        }

        // POST: Idea/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var idea = await _context.Idea.FindAsync(id);
            _context.Idea.Remove(idea);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool IdeaExists(int id)
        {
            return _context.Idea.Any(e => e.Id == id);
        }

        //Thumpup and thumpdown in index view
        public async Task<IActionResult> CreateUp(int id)
        {
            var userId = 1; // replace with code to get the current user ID
            //Check closure date
            var idea = await _context.Idea.SingleOrDefaultAsync(r => r.Id == id);
            // var closure = await _context.ClosureDate.FindAsync(idea.ClosureDateID);
            // if ( closure.ClousureDate > DateTime.Now)
            // {
            var rating = await _context.Rating.SingleOrDefaultAsync(r => r.IdeaID == id && r.UserId == userId);

            if (rating == null)
            {
                rating = new Rating
                {
                    IdeaID = id,
                    UserId = userId,
                    RatingUp = 1,
                    RatingDown = 0,
                    SubmitionDate = DateTime.Now
                };
                _context.Rating.Add(rating);
            }
            else
            {
                rating.RatingUp += 1;
                rating.SubmitionDate = DateTime.Now;
            }
            // }
            // else
            // {
            //     return Content("<script>alert('The closure date has passed.');</script>");
            // }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> CreateDown(int id)
        {
            var userId = 1; // replace with code to get the current user ID
            var rating = await _context.Rating.SingleOrDefaultAsync(r => r.IdeaID == id && r.UserId == userId);
            if (rating == null)
            {
                rating = new Rating
                {
                    IdeaID = id,
                    UserId = userId,
                    RatingUp = 0,
                    RatingDown = 1,
                    SubmitionDate = DateTime.Now
                };
                _context.Rating.Add(rating);
            }
            else
            {
                rating.RatingDown += 1;
                rating.SubmitionDate = DateTime.Now;
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        //Thumpup and thumpdown in detail view
        public async Task<IActionResult> DetailUp(int id)
        {
            var userId = 1; // replace with code to get the current user ID
            var rating = await _context.Rating.SingleOrDefaultAsync(r => r.IdeaID == id && r.UserId == userId);
            if (rating == null)
            {
                rating = new Rating
                {
                    IdeaID = id,
                    UserId = userId,
                    RatingUp = 1,
                    RatingDown = 0,
                    SubmitionDate = DateTime.Now
                };
                _context.Rating.Add(rating);
            }
            else
            {
                rating.RatingUp += 1;
                rating.SubmitionDate = DateTime.Now;
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = id });
        }

        public async Task<IActionResult> DetailDown(int id)
        {
            var userId = 1; // replace with code to get the current user ID
            var rating = await _context.Rating.SingleOrDefaultAsync(r => r.IdeaID == id && r.UserId == userId);
            if (rating == null)
            {
                rating = new Rating
                {
                    IdeaID = id,
                    UserId = userId,
                    RatingUp = 0,
                    RatingDown = 1,
                    SubmitionDate = DateTime.Now
                };
                _context.Rating.Add(rating);
            }
            else
            {
                rating.RatingDown += 1;
                rating.SubmitionDate = DateTime.Now;
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = id });
        }
    }
}

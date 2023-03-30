using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EnterpriseWeb.Models;
using EnterpriseWeb.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Microsoft.AspNetCore.Identity;
using EnterpriseWeb.Areas.Identity.Services;
using Microsoft.AspNetCore.StaticFiles;
using System.IO;
using System.Threading.Tasks;
using System.IO.Compression;


namespace EnterpriseWeb.Controllers
{
    public class CommentController : Controller
    {
        private string Layout1 = "_QACoordinator";
        private readonly UserManager<IdeaUser> _userManager;
        private readonly EnterpriseWebIdentityDbContext _context;

        public CommentController(EnterpriseWebIdentityDbContext context, UserManager<IdeaUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "QACoordinator")]
        // GET: Comment
        public async Task<IActionResult> Index(string currentFilter, string searchString, int? pageNumber)
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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);
            var department = await _context.Department.FindAsync(user.DepartmentID);
            var comment = from m in _context.Comment.Include(c => c.Idea).Include(i => i.IdeaUser) select m;
            if (department != null)
            {
                var comment2 = from m in _context.Comment.Include(c => c.Idea).Include(i => i.IdeaUser).Where(u => u.Idea.DepartmentID == department.Id) select m;
                comment = comment2;
            }
            if (!String.IsNullOrEmpty(searchString))
            {
                comment = comment.Where(s => s.Idea.Title.Contains(searchString));
            }
            int pageSize = 20;
            return View(await PaginatedList<Comment>.CreateAsync(comment.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        [Authorize(Roles = "QACoordinator")]
        // GET: Comment/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comment
                .Include(c => c.Idea)
                .Include(i => i.IdeaUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        [Authorize(Roles = "Staff")]
        // GET: Comment/Create
        public IActionResult Create()
        {
            ViewData["IdeaId"] = new SelectList(_context.Set<Idea>(), "Id", "Id");
            return View();
        }

        // POST: Comment/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CommentText,SubmitDate,UserId,IdeaId,status")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(comment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdeaId"] = new SelectList(_context.Set<Idea>(), "Id", "Id", comment.IdeaId);
            return View(comment);
        }
        [Authorize(Roles = "QACoordinator, Staff")]
        // GET: Comment/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // ViewBag.Layout = Layout1;
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comment.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }
            ViewData["IdeaId"] = new SelectList(_context.Set<Idea>(), "Id", "Id", comment.IdeaId);
            return View(comment);
        }

        // POST: Comment/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CommentText,SubmitDate,UserId,IdeaId,status")] Comment comment)
        {
            if (id != comment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(comment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentExists(comment.Id))
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
            ViewData["IdeaId"] = new SelectList(_context.Set<Idea>(), "Id", "Id", comment.IdeaId);
            return View(comment);
        }

        [Authorize(Roles = "QACoordinator, Staff")]
        // GET: Comment/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            ViewBag.Layout = Layout1;
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comment
                .Include(c => c.Idea)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // POST: Comment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var comment = await _context.Comment.FindAsync(id);
            _context.Comment.Remove(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CommentExists(int id)
        {
            return _context.Comment.Any(e => e.Id == id);
        }

        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> Commented()
        {
            var enterpriseWebContext = _context.Comment.Include(c => c.Idea)
                                                        .Include(i => i.IdeaUser);
            var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return _context.Comment != null ?
                    View(await _context.Comment
                    .Include(i => i.IdeaUser).Include(i => i.Idea)
                    .Where(o => o.UserId == userID)
                    .ToListAsync()) :
                    Problem("Entity set 'EnterpriseWebContext'  is null.");

        }
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> CommentedDetail(int id)
        {
            var enterpriseWebContext = _context.Comment.Where(e => e.Idea.Id == id).Include(b => b.IdeaUser);
            return View(await enterpriseWebContext.ToListAsync());
        }
    }
}


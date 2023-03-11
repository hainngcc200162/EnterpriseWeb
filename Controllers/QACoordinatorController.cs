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
    public class QACoordinatorController : Controller
    {
        private string Layout = "_ViewAdmin";
        private readonly EnterpriseWebIdentityDbContext _context;

        public QACoordinatorController(EnterpriseWebIdentityDbContext context)
        {
            _context = context;
        }

        // GET: QACoordinator
        public async Task<IActionResult> Index()
        {
            ViewBag.Layout = Layout;
            return View(await _context.QACoordinator.ToListAsync());
        }

        // GET: QACoordinator/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var qACoordinator = await _context.QACoordinator
                .FirstOrDefaultAsync(m => m.Id == id);
            if (qACoordinator == null)
            {
                return NotFound();
            }

            return View(qACoordinator);
        }

        // GET: QACoordinator/Create
        public IActionResult Create()
        {
            ViewBag.Layout = Layout;
            return View();
        }

        // POST: QACoordinator/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] QACoordinator qACoordinator)
        {
            if (ModelState.IsValid)
            {
                _context.Add(qACoordinator);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(qACoordinator);
        }

        // GET: QACoordinator/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var qACoordinator = await _context.QACoordinator.FindAsync(id);
            if (qACoordinator == null)
            {
                return NotFound();
            }
            return View(qACoordinator);
        }

        // POST: QACoordinator/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("Id,Name")] QACoordinator qACoordinator)
        {
            if (id != qACoordinator.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(qACoordinator);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QACoordinatorExists(qACoordinator.Id))
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
            return View(qACoordinator);
        }

        // GET: QACoordinator/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var qACoordinator = await _context.QACoordinator
                .FirstOrDefaultAsync(m => m.Id == id);
            if (qACoordinator == null)
            {
                return NotFound();
            }

            return View(qACoordinator);
        }

        // POST: QACoordinator/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var qACoordinator = await _context.QACoordinator.FindAsync(id);
            _context.QACoordinator.Remove(qACoordinator);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool QACoordinatorExists(int? id)
        {
            return _context.QACoordinator.Any(e => e.Id == id);
        }
    }
}

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
namespace EnterpriseWeb.Controllers
{
    public class IdeaController : Controller
    {
        private readonly EnterpriseWebIdentityDbContext _context;
        private readonly UserManager<IdeaUser> _userManager;
        private NotificationSender _notificationSender;

        private readonly IWebHostEnvironment hostEnvironment;


        public IdeaController(EnterpriseWebIdentityDbContext context, UserManager<IdeaUser> userManager, NotificationSender notificationSender, IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _notificationSender = notificationSender;
            hostEnvironment = environment;
        }

        public IActionResult ExportIdeaList(){
            List<Idea> ideas = _context.Idea.ToList<Idea>();
            var stream = new MemoryStream();
            using (var xlPackage = new ExcelPackage(stream))
            {
                var worksheet = xlPackage.Workbook.Worksheets.Add("Ideas");
                worksheet.Cells["A1"].Value = "List Idea";
                worksheet.Cells["A3"].Value = "Title";
                worksheet.Cells["B3"].Value = "Description";
                worksheet.Cells["C3"].Value = "Submission Date";
                worksheet.Cells["D3"].Value = "Department";
                worksheet.Cells["E3"].Value = "ClosureDate";


                int row = 4;
                foreach (var idea in ideas)
                {
                    worksheet.Cells[row, 1].Value = idea.Title;
                    worksheet.Cells[row, 2].Value = idea.Description;
                    worksheet.Cells[row, 3].Value = idea.SubmissionDate;
                    worksheet.Cells[row, 4].Value = idea.Department;
                    worksheet.Cells[row, 5].Value = idea.ClosureDate;


                    row++;
                }
                xlPackage.Save();
            }
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = "IdeaList.xlsx";
            stream.Position = 0;
            return File(stream, contentType, fileName);
        }

        public async Task<IActionResult> Comment(int id, string commenttext, string incognito)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);
            var idea = _context.Idea.FirstOrDefault(i => i.Id == id);

            //get url to detail idea
            var url = Url.Action("Details", "Idea", new { id = idea.Id }, protocol: HttpContext.Request.Scheme);

            //get email of author idea
            var email = await _userManager.GetEmailAsync(_userManager.Users.FirstOrDefault(u => u.Id == idea.UserId));

            if (incognito.Equals("no"))
            {
                var comment = new Comment
                {
                    CommentText = commenttext,
                    IdeaId = id,
                    UserId = userId,
                    IdeaUser = user,
                    SubmitDate = DateTime.Now,
                    status = 1,
                };
                _context.Comment.Add(comment);
                await _context.SaveChangesAsync();

                //Send notification to author of idea
                await _notificationSender.SendEmailAsync(
                    email,
                    "Comment Notification",
                    $"<strong>{user.Name}</strong> commented on <a href='{url}'> your idea </a> at {comment.SubmitDate}:<br><strong>{comment.CommentText}</strong>");
            }
            else if (incognito.Equals("yes"))
            {
                var comment = new Comment
                {
                    CommentText = commenttext,
                    IdeaId = id,
                    UserId = userId,
                    IdeaUser = user,
                    SubmitDate = DateTime.Now,
                    status = 0,
                };
                _context.Comment.Add(comment);
                await _context.SaveChangesAsync();

                //Send notification to author of idea
                await _notificationSender.SendEmailAsync(
                    email,
                    "Comment Notification",
                    $"<strong>Someone</strong> commented on <a href='{url}'> your idea </a> at {comment.SubmitDate}:<br><strong>{comment.CommentText}</strong>");
            }

            return RedirectToAction("Details", new { id = id });
        }

        // GET: Idea
        public async Task<IActionResult> Index()
        {
            var enterpriseWebContext = _context.Idea.Include(i => i.ClosureDate).Include(i => i.Department);
            return View(await enterpriseWebContext.ToListAsync());
        }
        public async Task<IActionResult> Filter(string currentFilter, string searchString, int? pageNumber)
        {
            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            ViewData["CurrentFilter"] = searchString;
            var ideas = from m in _context.IdeaCategory.Include(s => s.Idea).Include(i => i.Category) select m;
            if (!String.IsNullOrEmpty(searchString))
            {
                ideas = ideas.Where(s => s.Idea.Title.Contains(searchString));
            }
            int pageSize = 5;
            return View(await PaginatedList<IdeaCategory>.CreateAsync(ideas.AsNoTracking(), pageNumber ?? 1, pageSize));
        }
        public async Task<IActionResult> FilterCategory(string currentFilter, string searchString, int? pageNumber)
        {
            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            ViewData["CurrentFilter"] = searchString;
            var ideas = from m in _context.IdeaCategory.Include(s => s.Idea).Include(i => i.Category) select m;
            if (!String.IsNullOrEmpty(searchString))
            {
                ideas = ideas.Where(s => s.Category.Name.Contains(searchString));
            }
            int pageSize = 5;
            return View(await PaginatedList<IdeaCategory>.CreateAsync(ideas.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Idea/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ViewData["UserID"] = new SelectList(_context.Set<IdentityUser>(), "Id", "Name");

            var idea = await _context.Idea
                .Include(i => i.ClosureDate)
                .Include(i => i.Department)
                .Include(i => i.Comments)
                .ThenInclude(i => i.IdeaUser)
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
            ;
            // ViewData["uid"] = _userManager.GetUserId(User);
            // var users = await _userManager.Users.Where(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier)).ToListAsync();
            // var userRolesViewModel = new List<UserRolesViewModel>();
            // foreach (IdeaUser user in users)
            // {
            //     var thisViewModel = new UserRolesViewModel();
            //     thisViewModel.UserId = user.Id;
            //     thisViewModel.Name = user.Name;
            //     thisViewModel.PhoneNumber = user.PhoneNumber;
            //     thisViewModel.Address = user.Address;
            //     userRolesViewModel.Add(thisViewModel);
            // }
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
        public async Task<IActionResult> Create([Bind("Id,Title,Description,UserID,SupportingDocuments,DepartmentID,ClosureDateID")] Idea idea, IFormFile myfile)
        {
            if (ModelState.IsValid)
            {
                string filename=Path.GetFileName(myfile.FileName);
                var filePath = Path.Combine(hostEnvironment.WebRootPath, "uploads");
                string fullPath=filePath+"\\"+filename;
                // Copy files to FileSystem using Streams
                using (var stream = new FileStream(fullPath, FileMode.Create))
                    {	
                    await myfile.CopyToAsync(stream);
                    }
                idea.SupportingDocuments = filename;
                idea.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                idea.SubmissionDate = DateTime.Now;
                idea.IdeaUser = _userManager.Users.FirstOrDefault(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));
                _context.Add(idea);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClosureDateID"] = new SelectList(_context.Set<ClosureDate>(), "Id", "Id", idea.ClosureDateID);
            ViewData["DepartmentID"] = new SelectList(_context.Set<Department>(), "Id", "Id", idea.DepartmentID);
            ViewData["UserID"] = new SelectList(_context.Set<IdentityUser>(), "Id", "Id", idea.UserId);
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
                    idea.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
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

        //Thumbsup and thumbsdown in index view
        public async Task<IActionResult> CreateRating(int id, bool isUp)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // replace with code to get the current user ID
            var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);
            var idea = _context.Idea.FirstOrDefault(i => i.Id == id);

            //get url to detail idea
            var url = Url.Action("Details", "Idea", new { id = idea.Id }, protocol: HttpContext.Request.Scheme);

            //get email of author idea
            var email = await _userManager.GetEmailAsync(_userManager.Users.FirstOrDefault(u => u.Id == idea.UserId));

            var rating = await _context.Rating.SingleOrDefaultAsync(r => r.IdeaID == id && r.UserId.Equals(userId));

            if (rating == null)
            {
                if (isUp)
                {
                    rating = new Rating
                    {
                        IdeaID = id,
                        UserId = userId,
                        IdeaUser = user,
                        RatingUp = 1,
                        RatingDown = 0,
                        SubmitionDate = DateTime.Now
                    };
                    _context.Rating.Add(rating);
                    //Send notification to author of idea
                    await _notificationSender.SendEmailAsync(
                        email,
                        "Rating Notification",
                        $"<strong>{user.Name}</strong> liked on <a href='{url}'> your idea </a> at {rating.SubmitionDate}.");
                }
                else
                {
                    rating = new Rating
                    {
                        IdeaID = id,
                        UserId = userId,
                        IdeaUser = user,
                        RatingUp = 0,
                        RatingDown = 1,
                        SubmitionDate = DateTime.Now
                    };
                    _context.Rating.Add(rating);
                    //Send notification to author of idea
                    await _notificationSender.SendEmailAsync(
                        email,
                        "Rating Notification",
                        $"<strong>{user.Name}</strong> disliked on <a href='{url}'> your idea </a> at {rating.SubmitionDate}.");
                }
            }
            else
            {
                if (isUp)
                {
                    if(rating.RatingUp == 1)
                    {
                        rating.RatingUp = 0;
                        // _context.Rating.Remove(rating);
                    }
                    else
                    {
                        rating.RatingUp = 1;
                        rating.RatingDown = 0;
                    }
                }
                else
                {
                    if(rating.RatingDown == 1)
                    {
                        rating.RatingDown = 0;
                        // _context.Rating.Remove(rating);
                    }
                    else
                    {
                        rating.RatingDown = 1;
                        rating.RatingUp = 0;
                    }
                }
                rating.SubmitionDate = DateTime.Now;
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }



        //Thumbsup and thumbsdown in detail view
        public async Task<IActionResult> DetailRating(int id, bool isUp)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // replace with code to get the current user ID
            var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);
            var idea = _context.Idea.FirstOrDefault(i => i.Id == id);

            //get url to detail idea
            var url = Url.Action("Details", "Idea", new { id = idea.Id }, protocol: HttpContext.Request.Scheme);

            //get email of author idea
            var email = await _userManager.GetEmailAsync(_userManager.Users.FirstOrDefault(u => u.Id == idea.UserId));

            var rating = await _context.Rating.SingleOrDefaultAsync(r => r.IdeaID == id && r.UserId.Equals(userId));
            if (rating == null)
            {
                if (isUp)
                {
                    rating = new Rating
                    {
                        IdeaID = id,
                        UserId = userId,
                        IdeaUser = user,
                        RatingUp = 1,
                        RatingDown = 0,
                        SubmitionDate = DateTime.Now
                    };
                    _context.Rating.Add(rating);
                    //Send notification to author of idea
                    await _notificationSender.SendEmailAsync(
                        email,
                        "Rating Notification",
                        $"<strong>{user.Name}</strong> liked on <a href='{url}'> your idea </a> at {rating.SubmitionDate}.");
                }
                else
                {
                    rating = new Rating
                    {
                        IdeaID = id,
                        UserId = userId,
                        IdeaUser = user,
                        RatingUp = 0,
                        RatingDown = 1,
                        SubmitionDate = DateTime.Now
                    };
                    _context.Rating.Add(rating);
                    //Send notification to author of idea
                    await _notificationSender.SendEmailAsync(
                        email,
                        "Rating Notification",
                        $"<strong>{user.Name}</strong> disliked on <a href='{url}'> your idea </a> at {rating.SubmitionDate}.");
                }
            }
            else
            {
                if (isUp)
                {
                    if(rating.RatingUp == 1)
                    {
                        rating.RatingUp = 0;
                        // _context.Rating.Remove(rating);
                    }
                    else
                    {
                        rating.RatingUp = 1;
                        rating.RatingDown = 0;
                    }
                }
                else
                {
                    if(rating.RatingDown == 1)
                    {
                        rating.RatingDown = 0;
                        // _context.Rating.Remove(rating);
                    }
                    else
                    {
                        rating.RatingDown = 1;
                        rating.RatingUp = 0;
                    }
                }
                rating.SubmitionDate = DateTime.Now;
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = id });
        }

    }
}

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
    public class IdeaController : Controller
    {
        private string Layout = "_ViewAdmin";
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
        public IActionResult Blog()
        {
            return View();
        }
        public async Task<IActionResult> ViewIdea(string currentFilter, string searchString, int? pageNumber)
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
            var ideas = from m in _context.Idea.Include(i => i.ClosureDate)
                                        .Include(i => i.Department).Include(i => i.Viewings)
                        select m;
            if (!String.IsNullOrEmpty(searchString))
            {
                ideas = ideas.Where(s => s.Title.Contains(searchString));
            }
            int pageSize = 5;
            return View(await PaginatedList<Idea>.CreateAsync(ideas.AsNoTracking(), pageNumber ?? 1, pageSize));
        }
        public IActionResult Chart()
        {
            ViewBag.Layout = Layout;
            var data = _context.Rating.Include(s => s.Idea)
                        .GroupBy(s => s.Idea.Title)
                        .Select(g => new { Title = g.Key, RatingUp = g.Sum(s => s.RatingUp), RatingDown = g.Sum(s => s.RatingDown) })
                        .ToList();

            string[] labels = new string[data.Count];
            string[] ratingdown = new string[data.Count];
            string[] ratingup = new string[data.Count];


            for (int i = 0; i < data.Count; i++)
            {
                labels[i] = data[i].Title;
                ratingdown[i] = data[i].RatingDown.ToString();
                ratingup[i] = data[i].RatingUp.ToString();

            }

            ViewData["labels"] = string.Format("'{0}'", String.Join("','", labels));
            ViewData["ratingdown"] = String.Join(",", ratingdown);
            ViewData["ratingup"] = String.Join(",", ratingup);

            return View(data);
        }

        //Download files
        [HttpPost]
        public IActionResult Download(string fileName)
        {
            var filePath = Path.Combine(hostEnvironment.WebRootPath, "uploads", fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var memory = new MemoryStream();

            using (var archive = new ZipArchive(memory, ZipArchiveMode.Create, true))
            {
                var fileEntry = archive.CreateEntry(fileName);

                using (var originalFileStream = new FileStream(filePath, FileMode.Open))
                using (var compressedFileStream = fileEntry.Open())
                {
                    originalFileStream.CopyTo(compressedFileStream);
                }
            }

            memory.Position = 0;

            return File(memory, "application/octet-stream", $"{fileName}.zip");
        }



        public IActionResult ExportIdeaList()
        {
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
            var enterpriseWebContext = _context.Idea.Include(i => i.ClosureDate)
                                        .Include(i => i.Department).Include(i => i.Viewings);
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
        public async Task<IActionResult> Details(int id)
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
                .Include(i => i.Viewings)
                .ThenInclude(i => i.IdeaUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (idea == null)
            {
                return NotFound();
            }
            await ViewingIdea(id);

            return View(idea);
        }

        // GET: Idea/Create
        public IActionResult Create()
        {
            ViewData["ClosureDateID"] = new SelectList(_context.Set<ClosureDate>(), "Id", "Id");
            ViewData["DepartmentID"] = new SelectList(_context.Set<Department>(), "Id", "Id");
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
                //Getting FileName
                var fileName = Path.GetFileName(myfile.FileName);
                //Getting file Extension
                var fileExtension = Path.GetExtension(fileName);
                // concatenating  FileName + FileExtension
                var newFileName = String.Concat(Convert.ToString(Guid.NewGuid()), fileExtension);
                using (var target = new MemoryStream())
                { 
                    myfile.CopyTo(target);
                    idea.DataFiles = target.ToArray();
                }
                idea.SupportingDocuments = fileName;
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
            return View(idea);
        }

        // POST: Idea/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,UserID,DepartmentID,ClosureDateID,SupportingDocuments")] Idea idea, IFormFile newfile, string currentfile)
        {
            if (id != idea.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (newfile != null)
                    {
                        string filename = Path.GetFileName(newfile.FileName);
                        var filePath = Path.Combine(hostEnvironment.WebRootPath, "uploads");
                        string fullPath = Path.Combine(filePath, filename);
                        if (!filename.Equals(currentfile))
                        {
                            string oldFilePath = Path.Combine(filePath, currentfile);
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                            using (var stream = new FileStream(fullPath, FileMode.Create))
                            {
                                await newfile.CopyToAsync(stream);
                            }
                            idea.SupportingDocuments = filename;
                        }
                        else
                        {
                            idea.SupportingDocuments = filename;
                        }
                    }
                    else
                    {
                        idea.SupportingDocuments = currentfile;
                    }
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
            var filePath = Path.Combine(hostEnvironment.WebRootPath, "uploads", idea.SupportingDocuments);
            System.IO.File.Delete(filePath);
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
                    if (rating.RatingUp == 1)
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
                    if (rating.RatingDown == 1)
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
            await ViewingIdea(id);
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
                    if (rating.RatingUp == 1)
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
                    if (rating.RatingDown == 1)
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

        public async Task ViewingIdea(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // replace with code to get the current user ID
            var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);
            var idea = _context.Idea.FirstOrDefault(i => i.Id == id);

            var viewing = await _context.Viewing.SingleOrDefaultAsync(r => r.IdeaId == id && r.UserId.Equals(userId));
            if (viewing == null)
            {
                viewing = new Viewing
                {
                    IdeaId = id,
                    UserId = userId,
                    IdeaUser = user,
                    Count = 1,
                    ViewDate = DateTime.Now
                };
                _context.Viewing.Add(viewing);
                await _context.SaveChangesAsync();
            }

        }

    }
}

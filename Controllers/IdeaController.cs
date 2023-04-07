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
        private string Layout1 = "_QACoordinator";
        private string Layout2 = "_QAManager";
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
        [Authorize(Roles = "QAManager")]
        // ViewQA in Idea Controller is used by QA Manager to show Idea
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
            var ideas = from m in _context.Idea.Include(i => i.ClosureDate)
                                        .Include(i => i.Department).Include(i => i.Viewings)
                                        .Include(i => i.IdeaUser).Include(i => i.IdeaCategories)
                        select m;
            if (!String.IsNullOrEmpty(searchString))
            {
                ideas = ideas.Where(s => s.Title.Contains(searchString));
            }
            int pageSize = 5;
            return View(await PaginatedList<Idea>.CreateAsync(ideas.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ViewIdeaAdmin(string currentFilter, string searchString, int? pageNumber)
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
                                        .Include(i => i.IdeaUser).Include(i => i.IdeaCategories)
                        select m;
            if (!String.IsNullOrEmpty(searchString))
            {
                ideas = ideas.Where(s => s.Title.Contains(searchString));
            }
            int pageSize = 5;
            return View(await PaginatedList<Idea>.CreateAsync(ideas.AsNoTracking(), pageNumber ?? 1, pageSize));
        }
        [Authorize(Roles = "Admin, QAManager, QACoordinator")]
        // ViewIdea is used to accept or reject the idea of users, it is the page of QA Coordinator
        public async Task<IActionResult> ViewIdea(string currentFilter, string searchString, int? pageNumber, string filterIdea)
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
            var ideas = from m in _context.Idea.Include(i => i.ClosureDate)
                                           .Include(i => i.Department).Include(i => i.Viewings).Include(i => i.IdeaCategories)
                                           .Include(i => i.IdeaUser)
                        select m;
            var department = await _context.Department.FindAsync(user.DepartmentID);
            if (department != null)
            {
                var ideas2 = from m in _context.Idea.Include(i => i.ClosureDate)
                                            .Include(i => i.Department).Include(i => i.Viewings).Include(i => i.IdeaCategories)
                                            .Include(i => i.IdeaUser).Where(u => u.DepartmentID == department.Id)
                             select m;
                ideas = ideas2;
            }
            if (!String.IsNullOrEmpty(searchString))
            {
                ideas = ideas.Where(s => s.Title.Contains(searchString));
            }
            switch (filterIdea)
            {
                case "all":
                    // no filtering by status
                    break;
                case "approve":
                    ideas = ideas.Where(i => i.Status == 1);
                    break;
                case "reject":
                    ideas = ideas.Where(i => i.Status == 2);
                    break;
                case "underreview":
                    ideas = ideas.Where(i => i.Status == 0);
                    break;
                default:
                    break;
            }
            var message = TempData["message"]?.ToString();
            var messageClass = TempData["messageClass"]?.ToString();
            ViewData["message"] = message;
            ViewData["messageClass"] = messageClass;
            int pageSize = 5;
            return View(await PaginatedList<Idea>.CreateAsync(ideas.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        [Authorize(Roles = "Admin, QAManager")]
        // ViewCategory is used to display category in Admin Page
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
        [Authorize(Roles = "Admin, QAManager")]
        //Download files
        [HttpPost]
        public IActionResult Download(string fileName, byte[] dataFile)
        {
            var fileData = dataFile;
            var memory = new MemoryStream();

            using (var archive = new ZipArchive(memory, ZipArchiveMode.Create, true))
            {
                var fileEntry = archive.CreateEntry(fileName);

                using (var compressedFileStream = fileEntry.Open())
                {
                    compressedFileStream.Write(fileData, 0, fileData.Length);
                }
            }

            memory.Position = 0;

            return File(memory, "application/octet-stream", $"{fileName}.zip");
        }


        [Authorize(Roles = "Admin, QAManager")]
        public IActionResult ExportIdeaList()
        {
            List<Idea> ideas = _context.Idea.Include(i => i.IdeaUser)
                                            .Include(i => i.Department)
                                            .Include(i => i.ClosureDate)
                                            .Include(i => i.Ratings)
                                            .Include(i => i.Viewings)
                                            .ToList<Idea>();
            var stream = new MemoryStream();
            using (var xlPackage = new ExcelPackage(stream))
            {
                var worksheet = xlPackage.Workbook.Worksheets.Add("Ideas");
                worksheet.Cells["E1"].Value = "List Idea";
                worksheet.Cells["A3"].Value = "Staff Name";
                worksheet.Cells["B3"].Value = "Title";
                worksheet.Cells["C3"].Value = "Description";
                worksheet.Cells["D3"].Value = "Rating Up";
                worksheet.Cells["E3"].Value = "Rating Down";
                worksheet.Cells["F3"].Value = "View";
                worksheet.Cells["G3"].Value = "Submission Date";
                worksheet.Cells["H3"].Value = "Department";
                worksheet.Cells["I3"].Value = "ClosureDate";


                int row = 4;
                foreach (var idea in ideas)
                {
                    worksheet.Cells[row, 1].Value = idea.IdeaUser.Name;
                    worksheet.Cells[row, 2].Value = idea.Title;
                    worksheet.Cells[row, 3].Value = idea.Description;
                    worksheet.Cells[row, 4].Value = idea.Ratings.GroupBy(u => u.IdeaID).Sum(g => g.Sum(u => u.RatingUp));
                    worksheet.Cells[row, 5].Value = idea.Ratings.GroupBy(u => u.IdeaID).Sum(g => g.Sum(u => u.RatingDown));
                    worksheet.Cells[row, 6].Value = idea.Viewings.GroupBy(v => v.IdeaId).Sum(g => g.Sum(v => v.Count));
                    worksheet.Cells[row, 7].Value = idea.SubmissionDate.ToString();
                    worksheet.Cells[row, 8].Value = idea.Department.Name;
                    worksheet.Cells[row, 9].Value = idea.ClosureDate.ClousureDate.ToString();


                    row++;
                }
                xlPackage.Save();
            }
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = "IdeaList.xlsx";
            stream.Position = 0;
            return File(stream, contentType, fileName);
        }

        [Authorize(Roles = "Staff")]
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






        [Authorize(Roles = "Admin,Staff")]
        // GET: Idea
        public async Task<IActionResult> Index(string sortOrder, string searchString)
        {
            ViewData["CurrentFilter"] = searchString;
            var enterpriseWebContext = from e in _context.Idea
                           .Include(i => i.ClosureDate)
                           .Include(i => i.Department)
                           .Include(i => i.Viewings)
                           .Include(i => i.IdeaUser)
                           .Include(i => i.Ratings)
                           .Include(i => i.Comments)
                                       where e.Status == 1
                                       select e;

            if (!String.IsNullOrEmpty(searchString))
            {
                enterpriseWebContext = enterpriseWebContext.Where(e => e.Title.Contains(searchString)
                                        || e.Description.Contains(searchString)
                                        || e.Department.Name.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "mostView":
                    enterpriseWebContext = enterpriseWebContext.OrderByDescending(e => e.Viewings.Count);
                    break;
                case "mostRating":
                    enterpriseWebContext = enterpriseWebContext.OrderByDescending(e => e.Ratings
                        .Where(r => r.IdeaID == e.Id)
                        .AsEnumerable()
                        .Sum(r => r.RatingUp));
                    break;
                case "department":
                    enterpriseWebContext = enterpriseWebContext.OrderByDescending(e => e.Department.Name);
                    break;
                case "latest":
                    enterpriseWebContext = enterpriseWebContext.OrderByDescending(e => e.SubmissionDate);
                    break;
                default:
                    break;
            }
            var message = TempData["message"]?.ToString();
            var messageClass = TempData["messageClass"]?.ToString();
            ViewData["message"] = message;
            ViewData["messageClass"] = messageClass;
            ViewBag.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return View(await enterpriseWebContext.AsNoTracking().ToListAsync());
        }
        [Authorize(Roles = "Admin, QACoordinator, QAManager, Staff")]
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
        [Authorize(Roles = "Admin, QACoordinator, QAManager, Staff")]
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

        [Authorize(Roles = "Admin, Staff, QACoordinator")]
        // GET: Idea/Details/5
        public async Task<IActionResult> Details(int id, string sortOrder)
        {
            if (id == null)
            {
                return NotFound();
            }
            ViewData["UserID"] = new SelectList(_context.Set<IdentityUser>(), "Id", "Name");
            ViewData["Latest"] = String.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            var idea = await _context.Idea
                .Include(i => i.ClosureDate)
                .Include(i => i.Department)
                .Include(i => i.IdeaUser)
                .Include(i => i.Viewings)
                .Include(i => i.Ratings)
                .Include(i => i.Comments)
                .ThenInclude(i => i.IdeaUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (idea == null)
            {
                return NotFound();
            }

            // Sorting comments by latest date
            ViewBag.SubmitDateSortParam = String.IsNullOrEmpty(sortOrder) ? "submit_date_desc" : "";

            switch (sortOrder)
            {
                case "date_desc":
                    idea.Comments = idea.Comments.OrderByDescending(c => c.SubmitDate).ToList();
                    break;
                default:
                    break;
            }

            await ViewingIdea(id);
            ViewBag.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return View(idea);
        }

        [Authorize(Roles = "Staff, QACoordinator")]
        // GET: Idea/Create
        public IActionResult Create()
        {
            ViewData["ClosureDateID"] = new SelectList(_context.Set<ClosureDate>(), "Id", "Name");
            return View();
        }

        // POST: Idea/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdeaStatus,Id,Title,Description,UserId,SupportingDocuments,ClosureDateID")] Idea idea, IFormFile myfile, string istatus)
        {

            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);
                var department = await _context.Department.FindAsync(user.DepartmentID);


                //Getting FileName
                if (myfile != null && myfile.Length > 0)
                {


                    var fileName = Path.GetFileName(myfile.FileName);
                    using (var target = new MemoryStream())
                    {
                        myfile.CopyTo(target);
                        idea.DataFile = target.ToArray();
                    }
                    idea.SupportingDocuments = fileName;
                }
                else if (myfile != null && myfile.Length <= 0)
                {
                    ModelState.AddModelError("DataFile", "The file is empty. Please select a new file!");
                    ViewData["ClosureDateID"] = new SelectList(_context.Set<ClosureDate>(), "Id", "ClousureDate", idea.ClosureDateID);
                    ViewData["DepartmentID"] = new SelectList(_context.Set<Department>(), "Id", "Id", idea.DepartmentID);
                    ViewData["UserID"] = new SelectList(_context.Set<IdentityUser>(), "Id", "Id", idea.UserId);
                    return View(idea);
                }

                idea.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                idea.SubmissionDate = DateTime.Now;
                idea.Department = department;
                idea.DepartmentID = user.DepartmentID;
                idea.IdeaUser = user;
                idea.Status = 0;

                //Getting IdeaStatus
                if (istatus.Equals("no"))
                {
                    idea.IdeaStatus = 1;
                }
                else if (istatus.Equals("yes"))
                {
                    idea.IdeaStatus = 0;
                }
                _context.Add(idea);
                await _context.SaveChangesAsync();
                //Send email to QAcoor
                var url = Url.Action("EditQA", "Idea", new { id = idea.Id }, protocol: HttpContext.Request.Scheme);
                //get email of author idea
                var coorlist = _userManager.Users
                             .Where(u => u.DepartmentID == idea.DepartmentID)
                             .ToList();
                foreach (var coor in coorlist)
                {
                    if (await _userManager.IsInRoleAsync(coor, "QACoordinator"))
                    {
                        var email = await _userManager.GetEmailAsync(coor);
                        if (idea.Department != null)
                        {
                            await _notificationSender.SendEmailAsync(
                                email,
                                "Idea Notification",
                                $"<strong>{user.Name}</strong> posted a <a href='{url}'> new idea </a> at {idea.SubmissionDate} in the {idea.Department.Name}");
                        }
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClosureDateID"] = new SelectList(_context.Set<ClosureDate>(), "Id", "Id", idea.ClosureDateID);
            ViewData["DepartmentID"] = new SelectList(_context.Set<Department>(), "Id", "Id", idea.DepartmentID);
            ViewData["UserID"] = new SelectList(_context.Set<IdentityUser>(), "Id", "Id", idea.UserId);
            return View(idea);
        }

        [Authorize(Roles = "Staff, QACoordinator")]
        // GET: Idea/Edit/5
        // Edit is used by Staff
        public async Task<IActionResult> Edit(int? id, [Bind("IdeaStatus,Id,Title,Description,UserId,DepartmentID,ClosureDateID,SupportingDocuments,DataFile,Status,SubmissionDate")] IFormFile newfile, string istatus, int[] selectedCategoryIds)
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
            ViewBag.Categories = new SelectList(_context.Category, "Id", "Name");
            return View(idea);
        }

        // POST: Idea/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdeaStatus,Id,Title,Description,UserId,DepartmentID,ClosureDateID,SupportingDocuments,DataFile,Status,SubmissionDate")] Idea idea, IFormFile newfile, string istatus, int[] selectedCategoryIds)
        {
            if (id != idea.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (newfile != null && newfile.Length > 0)
                    {
                        string filename = Path.GetFileName(newfile.FileName);
                        // var filePath = Path.Combine(hostEnvironment.WebRootPath, "uploads");
                        // string fullPath = Path.Combine(filePath, filename);

                        using (var dataStream = new MemoryStream())
                        {
                            await newfile.CopyToAsync(dataStream);
                            idea.DataFile = dataStream.ToArray();
                        }
                        idea.SupportingDocuments = filename;
                    }
                    else if (newfile != null && newfile.Length <= 0)
                    {
                        ModelState.AddModelError("DataFile", "The file is empty. Please select a new file!");
                        ViewData["ClosureDateID"] = new SelectList(_context.Set<ClosureDate>(), "Id", "ClousureDate", idea.ClosureDateID);
                        ViewData["DepartmentID"] = new SelectList(_context.Set<Department>(), "Id", "Id", idea.DepartmentID);
                        ViewData["UserID"] = new SelectList(_context.Set<IdentityUser>(), "Id", "Id", idea.UserId);
                        return View(idea);
                    }
                    if (istatus.Equals("no"))
                    {
                        idea.IdeaStatus = 1;
                    }
                    else if (istatus.Equals("yes"))
                    {
                        idea.IdeaStatus = 0;
                    }
                    _context.Update(idea);
                    await _context.SaveChangesAsync();

                    // Add idea categories
                    foreach (var categoryId in selectedCategoryIds)
                    {
                        var ideaCategory = new IdeaCategory { IdeaID = idea.Id, CategoryID = categoryId };
                        _context.IdeaCategory.Add(ideaCategory);
                    }

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

        [Authorize(Roles = "QACoordinator")]
        // EditQA is used by QA coordinator
        public async Task<IActionResult> EditQA(int? id)
        {
            ViewBag.Layout = Layout1;
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
            ViewBag.Categories = new SelectList(_context.Category, "Id", "Name");
            return View(idea);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditQA(int id, [Bind("IdeaStatus,Id,Title,Description,Status,UserId,DepartmentID,ClosureDateID,SupportingDocuments,DataFile,Status,SubmissionDate")] Idea idea, IFormFile newfile, string istatus, int[] selectedCategoryIds)
        {
            ViewBag.Layout = Layout1;
            if (id != idea.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (newfile != null && newfile.Length > 0)
                    {
                        string filename = Path.GetFileName(newfile.FileName);
                        // var filePath = Path.Combine(hostEnvironment.WebRootPath, "uploads");
                        // string fullPath = Path.Combine(filePath, filename);

                        using (var dataStream = new MemoryStream())
                        {
                            await newfile.CopyToAsync(dataStream);
                            idea.DataFile = dataStream.ToArray();
                        }
                        idea.SupportingDocuments = filename;
                    }
                    else if (newfile != null && newfile.Length <= 0)
                    {
                        ModelState.AddModelError("DataFile", "The file is empty. Please select a new file!");
                        ViewData["ClosureDateID"] = new SelectList(_context.Set<ClosureDate>(), "Id", "ClousureDate", idea.ClosureDateID);
                        ViewData["DepartmentID"] = new SelectList(_context.Set<Department>(), "Id", "Id", idea.DepartmentID);
                        ViewData["UserID"] = new SelectList(_context.Set<IdentityUser>(), "Id", "Id", idea.UserId);
                        ViewBag.Categories = new SelectList(_context.Category, "Id", "Name");
                        return View(idea);
                    }
                    _context.Update(idea);
                    await _context.SaveChangesAsync();

                    // Add idea categories
                    foreach (var categoryId in selectedCategoryIds)
                    {
                        var ideaCategory = new IdeaCategory { IdeaID = idea.Id, CategoryID = categoryId };
                        _context.IdeaCategory.Add(ideaCategory);
                    }
                    TempData["message"] = "Edit successfully.";
                    TempData["messageClass"] = "alert-success";

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
                return RedirectToAction(nameof(ViewIdea));
            }
            ViewData["ClosureDateID"] = new SelectList(_context.Set<ClosureDate>(), "Id", "Id", idea.ClosureDateID);
            ViewData["DepartmentID"] = new SelectList(_context.Set<Department>(), "Id", "Id", idea.DepartmentID);
            return View(idea);
        }

        [Authorize(Roles = "Staff , QACoordinator")]
        // GET: Idea/Delete/5
        // it is used by staff
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
        [Authorize(Roles = "Staff, QACoordinator")]
        // POST: Idea/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var idea = await _context.Idea.FindAsync(id);
            var ideacategories = await _context.IdeaCategory.Where(o => o.Idea == idea).ToListAsync();
            foreach (var ideacategory in ideacategories)
            {
                _context.IdeaCategory.Remove(ideacategory);
            }
            _context.Idea.Remove(idea);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Record));
        }

        [Authorize(Roles = "Staff , QACoordinator")]
        // GET: Idea/Delete/5
        // it is used by staff
        public async Task<IActionResult> DeleteQA(int? id)
        {
            ViewBag.Layout = Layout1;
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
        [Authorize(Roles = "Staff, QACoordinator")]
        // POST: Idea/Delete/5
        [HttpPost, ActionName("DeleteQA")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmedQA(int id)
        {
            ViewBag.Layout = Layout1;
            var idea = await _context.Idea.FindAsync(id);
            var ideacategories = await _context.IdeaCategory.Where(o => o.Idea == idea).ToListAsync();
            foreach (var ideacategory in ideacategories)
            {
                _context.IdeaCategory.Remove(ideacategory);
            }
            TempData["message"] = "Delete successfully.";
            TempData["messageClass"] = "alert-success";
            _context.Idea.Remove(idea);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ViewIdea));
        }

        private bool IdeaExists(int id)
        {
            return _context.Idea.Any(e => e.Id == id);
        }

        [Authorize(Roles = "Staff")]
        //Thumbsup and thumbsdown in index view
        public async Task<IActionResult> CreateRating(int id, int isUp)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // replace with code to get the current user ID
            var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);
            var idea = _context.Idea.FirstOrDefault(i => i.Id == id);

            //get url to detail idea
            var url = Url.Action("Details", "Idea", new { id = idea.Id }, protocol: HttpContext.Request.Scheme);

            //get email of author idea
            var email = await _userManager.GetEmailAsync(_userManager.Users.FirstOrDefault(u => u.Id == idea.UserId));

            var rating = await _context.Rating.FirstOrDefaultAsync(r => r.IdeaID == id && r.UserId.Equals(userId));

            if (rating == null)
            {
                if (isUp == 0)
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
                if (isUp == 0)
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
            var ideaRatings = _context.Idea
                            .Include(i => i.Ratings)
                            .Include(i => i.Viewings)
                            .Where(i => i.Id == id)
                            .Select(i => new
                            {
                                upvotes = i.Ratings.Sum(r => r.RatingUp),
                                downvotes = i.Ratings.Sum(r => r.RatingDown),
                                views = i.Viewings.Count()
                            }).FirstOrDefault();
            return new JsonResult(ideaRatings);
        }


        [Authorize(Roles = "Staff,Admin,QAManager")]
        //Thumbsup and thumbsdown in detail view
        public async Task<IActionResult> DetailRating(int id, int isUp)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // replace with code to get the current user ID
            var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);
            var idea = _context.Idea.FirstOrDefault(i => i.Id == id);

            //get url to detail idea
            var url = Url.Action("Details", "Idea", new { id = idea.Id }, protocol: HttpContext.Request.Scheme);

            //get email of author idea
            var email = await _userManager.GetEmailAsync(_userManager.Users.FirstOrDefault(u => u.Id == idea.UserId));

            var rating = await _context.Rating.FirstOrDefaultAsync(r => r.IdeaID == id && r.UserId.Equals(userId));
            if (rating == null)
            {
                if (isUp == 0)
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
                if (isUp == 0)
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
            var ideaRatings = _context.Idea
                            .Include(i => i.Ratings)
                            .Include(i => i.Viewings)
                            .Where(i => i.Id == id)
                            .Select(i => new
                            {
                                upvotes = i.Ratings.Sum(r => r.RatingUp),
                                downvotes = i.Ratings.Sum(r => r.RatingDown),
                                views = i.Viewings.Count()
                            }).FirstOrDefault();
            return new JsonResult(ideaRatings);
        }

        [Authorize(Roles = "Admin, QAManager")]
        public async Task ViewingIdea(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // replace with code to get the current user ID
            var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);
            var idea = _context.Idea.FirstOrDefault(i => i.Id == id);

            var viewing = await _context.Viewing.FirstOrDefaultAsync(r => r.IdeaId == id && r.UserId.Equals(userId));
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

        [Authorize(Roles = "QAManager")]
        public IActionResult Chart()
        {
            ViewBag.Layout = Layout2;
            var totalIdeas = _context.Idea.Count();
            var data = _context.Department
                .Select(d => new
                {
                    DepartmentName = d.Name,
                    ContributorCount = _context.Idea
                        .Where(i => i.DepartmentID == d.Id)
                        .Select(i => i.UserId)
                        .Distinct()
                        .Count(),
                    NumberIdea = _context.Idea
                        .Where(i => i.DepartmentID == d.Id)
                        .Select(i => i.Id)
                        .Count(),
                    PercentageIdea = totalIdeas > 0
                        ? _context.Idea
                            .Where(i => i.DepartmentID == d.Id)
                            .Select(i => i.Id)
                            .Count() * 100 / totalIdeas
                        : 0
                })
                .ToList();

            string[] labels = new string[data.Count];
            string[] numberidea = new string[data.Count];
            string[] percentageidea = new string[data.Count];
            string[] counts = new string[data.Count];

            for (int i = 0; i < data.Count; i++)
            {
                labels[i] = data[i].DepartmentName;
                numberidea[i] = data[i].NumberIdea.ToString();
                percentageidea[i] = data[i].PercentageIdea.ToString();
                counts[i] = data[i].ContributorCount.ToString();
            }

            ViewData["labels"] = string.Format("'{0}'", String.Join("','", labels));
            ViewData["counts"] = String.Join(",", counts);
            ViewData["numberidea"] = String.Join(",", numberidea);
            ViewData["percentageidea"] = String.Join(",", percentageidea);

            return View(data);

        }
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> Record()
        {
            var enterpriseWebContext = _context.Idea.Include(i => i.ClosureDate);
            var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);

            ViewBag.Categories = new SelectList(_context.Category, "Id", "Name");

            return _context.Idea != null ?
                        View(await _context.Idea.Include(i => i.ClosureDate)
                                                .Include(i => i.Department)
                                                .Include(i => i.Viewings)
                                                .Include(i => i.IdeaUser)
                                                .Include(i => i.Ratings)
                                                .Where(o => o.UserId == userID)
                                                .ToListAsync()) :
                        Problem("Entity set 'EnterpriseWebContext.Order'  is null.");
        }

    }
}

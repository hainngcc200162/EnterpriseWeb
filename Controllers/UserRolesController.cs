using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EnterpriseWeb.Areas.Identity.Data;
using EnterpriseWeb.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace EnterpriseWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserRolesController : Controller
    {
        private string Layout = "_ViewAdmin";
        private readonly UserManager<IdeaUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly EnterpriseWebIdentityDbContext _context;


        public UserRolesController(UserManager<IdeaUser> userManager, RoleManager<IdentityRole> roleManager, EnterpriseWebIdentityDbContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;

        }
        public async Task<IActionResult> Index(string currentFilter, string searchString, int? pageNumber)
        {
            ViewBag.Layout = Layout;
            var users = await _userManager.Users.ToListAsync();
            var userRolesViewModel = new List<UserRolesViewModel>();
            var message = TempData["message"]?.ToString();
            var messageClass = TempData["messageClass"]?.ToString();
            foreach (IdeaUser user in users)
            {
                var thisViewModel = new UserRolesViewModel();
                thisViewModel.UserId = user.Id;
                thisViewModel.Email = user.Email;
                thisViewModel.Confirm = user.EmailConfirmed;
                thisViewModel.Name = user.Name;
                thisViewModel.DOB = user.DOB;
                thisViewModel.Address = user.Address;
                thisViewModel.Roles = await GetUserRoles(user);
                userRolesViewModel.Add(thisViewModel);
            }

            ViewData["message"] = message;
            ViewData["messageClass"] = messageClass;

            return View(userRolesViewModel);
        }
        public async Task<IActionResult> Manage(string userId)
        {
            ViewBag.Layout = Layout;
            ViewBag.userId = userId;
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {userId} cannot be found";
                return View("NotFound");
            }
            ViewBag.UserName = user.UserName;
            var model = new List<ManageUserRolesViewModel>();
            foreach (var role in _roleManager.Roles.ToList())
            {
                var userRolesViewModel = new ManageUserRolesViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name
                };
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    userRolesViewModel.Selected = true;
                }
                else
                {
                    userRolesViewModel.Selected = false;
                }
                model.Add(userRolesViewModel);
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Manage(List<ManageUserRolesViewModel> model, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View();
            }
            var roles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, roles);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing roles");
                return View(model);
            }
            TempData["message"] = "Update successfully.";
            TempData["messageClass"] = "alert-success";

            result = await _userManager.AddToRolesAsync(user, model.Where(x => x.Selected).Select(y => y.RoleName));
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add selected roles to user");
                return View(model);
            }
            return RedirectToAction("Index");
        }
        private async Task<List<string>> GetUserRoles(IdeaUser user)
        {
            return new List<string>(await _userManager.GetRolesAsync(user));
        }

        public async Task<IActionResult> Delete(string id)
        {
            ViewBag.Layout = Layout;
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            TempData["message"] = "Delete successfully.";
            TempData["messageClass"] = "alert-success";
            var comments = await _context.Comment.Where(o => o.IdeaUser == user).ToListAsync();
            var views = await _context.Viewing.Where(o => o.IdeaUser == user).ToListAsync();
            var ratings = await _context.Rating.Where(o => o.IdeaUser == user).ToListAsync();
            var ideas = await _context.Idea.Where(o => o.IdeaUser == user).ToListAsync();

            foreach (var comment in comments)
            {
                _context.Comment.Remove(comment);
            }
            foreach (var view in views)
            {
                _context.Viewing.Remove(view);
            }
            foreach (var rating in ratings)
            {
                _context.Rating.Remove(rating);
            }
            foreach (var idea in ideas)
            {
                var ideacategories = await _context.IdeaCategory.Where(o => o.Idea == idea).ToListAsync();
                foreach (var ideacategory in ideacategories)
                {
                    _context.IdeaCategory.Remove(ideacategory);
                }
                _context.Idea.Remove(idea);
            }
            await _context.SaveChangesAsync();
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(user);
        }
        public async Task<IActionResult> Edit(string id)
        {
            ViewBag.Layout = Layout;
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("UserId,Name,DOB,Address,Gmail")] IdeaUser users)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user != null)
                {
                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "User not found");
                }
            }
            return View(users);
        }
    }
}
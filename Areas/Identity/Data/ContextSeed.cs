using Microsoft.AspNetCore.Identity;
using EnterpriseWeb.Models;
using EnterpriseWeb.Controllers;
using EnterpriseWeb.Enums;

namespace EnterpriseWeb.Areas.Identity.Data
{
    public static class ContextSeed
    {
        public static async Task SeedRolesAsync(UserManager<IdeaUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            //Seed Roles
            await roleManager.CreateAsync(new IdentityRole(Enums.Roles.Admin.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Enums.Roles.Staff.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Enums.Roles.QAManager.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Enums.Roles.QACoordinator.ToString()));
        }
        public static async Task SeedSuperAdminAsync(UserManager<IdeaUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            //Seed Default User
            var defaultUserAdmin = new IdeaUser
            {
                UserName = "Admin@gmail.com",
                Email = "Admin@gmail.com",
                Name = "Admin",
                Address = "Can Tho",
                DOB = new DateTime(2008, 3, 9, 16, 5, 7, 123),
                EmailConfirmed = true,
                PhoneNumber = "0909090909",
                PhoneNumberConfirmed = true,
                ProfilePicture = await transferPic("././wwwroot/img/admin.jpg"),

            };
            if (userManager.Users.All(u => u.Id != defaultUserAdmin.Id))
            {
                var user = await userManager.FindByEmailAsync(defaultUserAdmin.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUserAdmin, "Admin@123");
                    await userManager.AddToRoleAsync(defaultUserAdmin, Enums.Roles.Admin.ToString());
                }
            }

            //Seed Default User
            var defaultUserStaff = new IdeaUser
            {
                UserName = "Staff@gmail.com",
                Email = "Staff@gmail.com",
                Name = "Staff",
                Address = "Can Tho",
                DOB = new DateTime(2008, 2, 9, 16, 5, 7, 123),
                EmailConfirmed = true,
                PhoneNumber = "0909090908",
                PhoneNumberConfirmed = true,
                ProfilePicture = await transferPic("././wwwroot/img/staff.jpg")

            };
            if (userManager.Users.All(u => u.Id != defaultUserStaff.Id))
            {
                var user = await userManager.FindByEmailAsync(defaultUserStaff.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUserStaff, "Admin@123");
                    await userManager.AddToRoleAsync(defaultUserStaff, Enums.Roles.Staff.ToString());

                }
            }

            //Seed Default User
            var defaultUserQAManager = new IdeaUser
            {
                UserName = "QA@gmail.com",
                Email = "QA@gmail.com",
                Name = "QAManager",
                Address = "Can Tho",
                DOB = new DateTime(2008, 2, 8, 16, 5, 7, 123),
                EmailConfirmed = true,
                PhoneNumber = "0909090907",
                PhoneNumberConfirmed = true,
                ProfilePicture = await transferPic("././wwwroot/img/qamanager.jpg")

            };
            if (userManager.Users.All(u => u.Id != defaultUserQAManager.Id))
            {
                var user = await userManager.FindByEmailAsync(defaultUserQAManager.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUserQAManager, "Admin@123");
                    await userManager.AddToRoleAsync(defaultUserQAManager, Enums.Roles.QAManager.ToString());
                }
            }

            var defaultUserQACoordinator = new IdeaUser
            {
                UserName = "QAcoor@gmail.com",
                Email = "QAcoor@gmail.com",
                Name = "QACoordinator",
                Address = "Can Tho",
                DOB = new DateTime(2008, 2, 8, 16, 5, 7, 123),
                EmailConfirmed = true,
                PhoneNumber = "0909090907",
                PhoneNumberConfirmed = true,
                ProfilePicture = await transferPic("././wwwroot/img/qamanager.jpg"),
                DepartmentID = 1, // DefaultID of Department
                Department = new Department { Name = "Default Department" , Description = "Default Department"}

            };
            if (userManager.Users.All(u => u.Id != defaultUserQACoordinator.Id))
            {
                var user = await userManager.FindByEmailAsync(defaultUserQACoordinator.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUserQACoordinator, "Admin@123");
                    await userManager.AddToRoleAsync(defaultUserQACoordinator, Enums.Roles.QACoordinator.ToString());
                }
            }
        }
        public static async Task<byte[]> transferPic(string path)
        {
            using var image = System.Drawing.Image.FromFile(path);
            using var mStream = new MemoryStream();
            image.Save(mStream, image.RawFormat);
            return mStream.ToArray();
        }
    }
}
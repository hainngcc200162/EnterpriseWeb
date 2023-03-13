using Microsoft.AspNetCore.Identity;
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
        }
        public static async Task SeedSuperAdminAsync(UserManager<IdeaUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            //Seed Default User
            var defaultUserAdmin = new IdeaUser
            {
                UserName = "Admin@gmail.com",
                Email = "Admin@gmail.com",
                Name = "NGUYEN NGOC HAI",
                Address = "Can Tho",
                DOB = new DateTime(2008, 3, 9, 16, 5, 7, 123),
                EmailConfirmed = true,
                PhoneNumber = "0909090909",
                PhoneNumberConfirmed = true,

            };
            if (userManager.Users.All(u => u.Id != defaultUserAdmin.Id))
            {
                var user = await userManager.FindByEmailAsync(defaultUserAdmin.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUserAdmin, "Admin@123");
                    await userManager.AddToRoleAsync(defaultUserAdmin, Enums.Roles.Admin.ToString());
                    await userManager.AddToRoleAsync(defaultUserAdmin, Enums.Roles.Staff.ToString());
                    await userManager.AddToRoleAsync(defaultUserAdmin, Enums.Roles.QAManager.ToString());
                }
            }
        }
    }
}
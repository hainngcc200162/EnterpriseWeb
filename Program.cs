using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Diagnostics;

using EnterpriseWeb.Areas.Identity.Data;
using EnterpriseWeb.Areas.Identity.Services;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<EnterpriseWebIdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("EnterpriseWebIdentityDbContextConnection") ?? throw new InvalidOperationException("Connection string 'FPTBOOK_STOREIdentityDbContextConnection' not found.")));
builder.Services.AddDefaultIdentity<IdeaUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<EnterpriseWebIdentityDbContext>();
builder.Services.AddDbContext<EnterpriseWebIdentityDbContext>(options =>
options.UseSqlServer("EnterpriseWebIdentityDbContextConnection"));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.Configure<AuthMessageSenderOptions>(builder.Configuration);
builder.Services.AddScoped<NotificationSender>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    // var services = scope.ServiceProvider;
    // var context = services.GetRequiredService<IdeaUser>();
    // var userManager = services.GetRequiredService<UserManager<IdeaUser>>();
    // var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    // await ContextSeed.SeedRolesAsync(userManager, roleManager);

    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<IdeaUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    await ContextSeed.SeedRolesAsync(userManager, roleManager);
    await ContextSeed.SeedSuperAdminAsync(userManager, roleManager);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500; // or another Status code
        context.Response.ContentType = "text/html";

        await context.Response.WriteAsync("<html><body>\r\n");
        await context.Response.WriteAsync("Error Page - Customized for Duplicate Data!<br><br>\r\n");

        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        if (exceptionHandlerPathFeature?.Error is DbUpdateException)
        {
            await context.Response.WriteAsync("<h2>Duplicate data found!</h2>\r\n");
            await context.Response.WriteAsync("<p>Please enter unique data.</p>\r\n");
            // You can also add a link to redirect the user back to the input form.
        }

        await context.Response.WriteAsync("</body></html>\r\n");
        await context.Response.CompleteAsync();
    });
});
}

// if (!app.Environment.IsDevelopment())
// {
    
// }



app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
app.Run();

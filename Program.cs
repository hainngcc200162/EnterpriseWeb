using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

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

using (var scope = app.Services.CreateScope()){
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
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

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

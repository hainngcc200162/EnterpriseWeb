using EnterpriseWeb.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EnterpriseWeb.Models;

namespace EnterpriseWeb.Areas.Identity.Data;

public class EnterpriseWebIdentityDbContext : IdentityDbContext<IdeaUser>
{
    public EnterpriseWebIdentityDbContext(DbContextOptions<EnterpriseWebIdentityDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Department>()
            .HasIndex(p => p.Name)
            .IsUnique();
        builder.Entity<Category>()
            .HasIndex(p => p.Name)
            .IsUnique();


        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
    }

    public DbSet<EnterpriseWeb.Models.Rating> Rating { get; set; }

    public DbSet<EnterpriseWeb.Models.Comment> Comment { get; set; }

    public DbSet<EnterpriseWeb.Models.Idea> Idea { get; set; } = default!;

    public DbSet<EnterpriseWeb.Models.IdeaCategory> IdeaCategory { get; set; }

    public DbSet<EnterpriseWeb.Models.Category> Category { get; set; }

    public DbSet<EnterpriseWeb.Models.ClosureDate> ClosureDate { get; set; }

    public DbSet<EnterpriseWeb.Models.Department> Department { get; set; }

    public DbSet<EnterpriseWeb.Models.Viewing> Viewing { get; set; }
}

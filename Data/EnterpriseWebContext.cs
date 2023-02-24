using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EnterpriseWeb.Models;

    public class EnterpriseWebContext : DbContext
    {
        public EnterpriseWebContext (DbContextOptions<EnterpriseWebContext> options)
            : base(options)
        {
        }

        public DbSet<EnterpriseWeb.Models.User> User { get; set; }

        public DbSet<EnterpriseWeb.Models.Rating> Rating { get; set; }

        public DbSet<EnterpriseWeb.Models.Comment> Comment { get; set; }

        public DbSet<EnterpriseWeb.Models.Idea> Idea { get; set; }

        public DbSet<EnterpriseWeb.Models.IdeaCategory> IdeaCategory { get; set; }

        public DbSet<EnterpriseWeb.Models.Category> Category { get; set; }

        public DbSet<EnterpriseWeb.Models.ClosureDate> ClosureDate { get; set; }

        public DbSet<EnterpriseWeb.Models.Department> Department { get; set; }

        public DbSet<EnterpriseWeb.Models.QACoordinator> QACoordinator { get; set; }
    }

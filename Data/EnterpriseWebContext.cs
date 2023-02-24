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
    }

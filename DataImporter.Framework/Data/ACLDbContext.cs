using DataImporter.Framework.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataImporter.Framework.Data
{
    public class ACLDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ACLDbContext(DbContextOptions<ACLDbContext> options)
            : base(options)
        {
            
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var environmentName = Environment.GetEnvironmentVariable("CORE_ENVIRONMENT");

            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true);

            var configuration = builder.Build();

            optionsBuilder.UseSqlServer(configuration.GetConnectionString("ACLConnection"));

            base.OnConfiguring(optionsBuilder);
        }
        
        public DbSet<Company> Companies { get; set; }

        public DbSet<CompanyClaims> CompanyClaims { get; set; }

        public DbSet<UserZohoContact> UserZohoContacts { get; set; }
    }
}

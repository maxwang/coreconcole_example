using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using DataImporter.Framework.Models;

namespace DataImporter.Framework.Data
{
    public class ZohoCRMDbContext : DbContext
    {
        public ZohoCRMDbContext(DbContextOptions<ZohoCRMDbContext> options) : base(options)
        {
        }

        public DbSet<ZohoCRMPartnerPortal> PartnerPortals { get; set; }
        public DbSet<ZohoTableStatus> TableStatus { get; set; }

        public DbSet<ZohoAccount> Accounts { get; set; }
        public DbSet<ZohoContact> Contacts { get; set; }

        public DbSet<ZohoActionLog> ActionLogs { get; set; }

        public DbSet<ZohoBitdefender> Bitdefenders { get; set; }
    }
}

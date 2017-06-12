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
        public ZohoCRMDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<ZohoCRMPartnerPortal> PartnerPortals { get; set; }
    }
}

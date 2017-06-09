using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace DataImporter.Framework.Data
{
    public class ZohoCRMDbContext : DbContext
    {
        public ZohoCRMDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}

using DataImporter.Framework.Data;
using DataImporter.Framework.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataImporter.Framework.Repository
{
   
    public class ZohoCRMDbRepository : IZohoCRMDataRepository
    {
        private readonly ZohoCRMDbContext _db;
        public ZohoCRMDbRepository(ZohoCRMDbContext dbContext)
        {
            _db = dbContext;
        }

        public IEnumerable<ZohoCRMPartnerPortal> PartnerPortals => _db.PartnerPortals.AsNoTracking();
    }
}

using DataImporter.Framework.Data;
using DataImporter.Framework.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DataImporter.Framework.Repository
{
   
    public class ZohoCRMDbRepository : IZohoCRMDataRepository
    {
        private readonly ZohoCRMDbContext _db;
        public ZohoCRMDbRepository(ZohoCRMDbContext dbContext)
        {
            _db = dbContext;
        }

        public IEnumerable<ZohoPartnerPortal> PartnerPortals => _db.PartnerPortals.AsNoTracking();
        public IEnumerable<ZohoProduct> Products => _db.ZohoProducts.AsNoTracking();

        public IEnumerable<ZohoTableStatus> TableStatus => _db.TableStatus.AsNoTracking();

        public IEnumerable<ZohoBitdefender> Bitdefenders => _db.Bitdefenders.AsNoTracking();

        public IEnumerable<ZohoContact> Contacts => _db.Contacts.AsNoTracking();
        public IEnumerable<ZohoAccount> Accounts => _db.Accounts.AsNoTracking();

        public async Task<int> AddActionLogAsync(ZohoActionLog log)
        {
            var result = await _db.ActionLogs.AddAsync(log);
            await _db.SaveChangesAsync();
            return log.ActionLogId;

        }

        public async Task<bool> UpdateTableStatusAsync(ZohoTableStatus status)
        {
            var record = await _db.TableStatus.SingleOrDefaultAsync(x => x.TableStatusId == status.TableStatusId);
            if(record == null)
            {
                return false;
            }



            record.PortalAction = status.PortalAction;
            record.PortalActionResult = status.PortalActionResult;
            record.PortalActionTime = DateTime.Now;
            await _db.SaveChangesAsync();

            return true;
        }
    }
}

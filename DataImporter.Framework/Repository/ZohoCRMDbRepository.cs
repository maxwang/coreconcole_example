using DataImporter.Framework.Data;
using DataImporter.Framework.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public IEnumerable<ZohoProductMyobConfiguration> ZohoProductMyobConfigurations => _db.ZohoProductMyobConfigurations.AsNoTracking();

        public async Task<bool> UpdateProductMyobUuidAsync(ZohoProductMyobConfiguration config)
        {
            var record = await _db.ZohoProductMyobConfigurations.FirstOrDefaultAsync(x => x.Id == config.Id);
            if (record == null)
            {
                return false;
            }

            record.MyobUuid = config.MyobUuid;
            record.ModifiedBy = config.ModifiedBy;
            record.ModifiedTime = config.ModifiedTime;
            
            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<int> AddActionLogAsync(ZohoActionLog log)
        {
            var result = await _db.ActionLogs.AddAsync(log);
            await _db.SaveChangesAsync();
            return log.ActionLogId;

        }

        public async Task<IList<ZohoProductMyobConfiguration>> GetProductMyobConfigurations(string productId)
        {
            return await _db.ZohoProductMyobConfigurations
                .Where(x => x.ProductId.Equals(productId, StringComparison.CurrentCultureIgnoreCase)).ToArrayAsync();
        }

        public async Task<bool> UpdateTableStatusAsync(ZohoTableStatus status)
        {
            var record = await _db.TableStatus.FirstOrDefaultAsync(x => x.TableStatusId == status.TableStatusId);
            if(record == null)
            {
                return false;
            }

            record.PortalAction = status.PortalAction.Length < 48
                ? status.PortalAction
                : status.PortalAction.Substring(0, 48);

            if (string.IsNullOrEmpty(status.PortalActionResult))
            {
                record.PortalActionResult = "";
            }
            else
            {
                record.PortalActionResult = status.PortalActionResult.Length > 255
                    ? status.PortalActionResult.Substring(0, 255)
                    : status.PortalActionResult;
            }
            record.PortalActionTime = DateTime.Now;
            await _db.SaveChangesAsync();

            return true;
        }
    }
}

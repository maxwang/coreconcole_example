using DataImporter.Framework.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DataImporter.Framework.Repository
{
    public interface IZohoCRMDataRepository
    {
        IEnumerable<ZohoPartnerPortal> PartnerPortals { get; }
        IEnumerable<ZohoTableStatus> TableStatus { get; }
        IEnumerable<ZohoContact> Contacts { get; }
        IEnumerable<ZohoAccount> Accounts { get; }
        IEnumerable<ZohoBitdefender> Bitdefenders { get; }
        IEnumerable<ZohoProduct> Products { get; }
        IEnumerable<ZohoProductMyobConfiguration> ZohoProductMyobConfigurations { get; }

        Task<IList<ZohoProductMyobConfiguration>> GetProductMyobConfigurations(string productId);
        Task<bool> UpdateTableStatusAsync(ZohoTableStatus status);
        Task<bool> UpdateProductMyobUuidAsync(ZohoProductMyobConfiguration config);

        Task<int> AddActionLogAsync(ZohoActionLog log);
        
    }
}

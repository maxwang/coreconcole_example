using DataImporter.Framework.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DataImporter.Framework.Repository
{
    public interface IZohoCRMDataRepository
    {
        IEnumerable<ZohoCRMPartnerPortal> PartnerPortals { get; }
        IEnumerable<ZohoTableStatus> TableStatus { get; }
        IEnumerable<ZohoContact> Contacts { get; }
        //IEnumerable<ZohoAccount> Accounts { get; }
        IEnumerable<ZohoBitdefender> Bitdefenders { get; }

        Task<bool> UpdateTableStatusAsync(ZohoTableStatus status);

        Task<int> AddActionLogAsync(ZohoActionLog log);
        
    }
}

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

        Task<bool> UpdateTableStatusAsync(ZohoTableStatus status);
        
    }
}

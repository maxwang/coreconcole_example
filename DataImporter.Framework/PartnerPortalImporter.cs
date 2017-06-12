using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DataImporter.Framework.Repository;
using DataImporter.Framework.Services;
using System.Threading;
using System.Linq;

namespace DataImporter.Framework
{
    public class PartnerPortalImporter : ZohoImportBase
    {
        
        public PartnerPortalImporter(IZohoCRMDataRepository zohoRepository, IEmailSender emailSender) : base(zohoRepository, emailSender)
        {
            TableName = "PartnerPortal";
        }

        protected override async Task<string> GetNextUpdatedRecordAsync(string id = "")
        {
54dxf67w23ee3d
                         

            return await (string.IsNullOrEmpty(id) ? 
                Task.FromResult("test") : 
                Task.FromResult(string.Empty));
        }

        protected override async Task UpdateStatus(string id = "")
        {
            System.Threading.Thread.Sleep(10000);
            await Task.FromResult(true);
            Console.WriteLine("Partner Portal Process Update");
        }

        protected override async Task<bool> ProcessImport(string id = "")
        {
            
            System.Threading.Thread.Sleep(10000);
            Console.WriteLine("Partner Portal Process Import");
            return await Task.FromResult(true);
            
        }
    }
}

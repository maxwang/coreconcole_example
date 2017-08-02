using MyobCoreProxy;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Options;
using MyobCoreProxy.Models;
using MyobCoreProxy.Services;
using ZohoCRMProxy;
using ZohoAccount = DataImporter.Framework.Models.ZohoAccount;
using System.Threading.Tasks;

namespace DataImporter.Framework.Services
{
    public class MyobApiService
    {
        private MyobImportOptions _myobImportOptions;

        public Dictionary<string, ProductImport> ProductImportOptions => _myobImportOptions.ProductImport;

        public Dictionary<string, ContactCustomerImportOptions> ContactCustomerImportOptions => _myobImportOptions.ContactCustomerImportOptions;

        public MyobOptions MyobOptions => _myobImportOptions.MyobOptions;

        public string SalesEmail => _myobImportOptions.SalesEmail;

        public MyobApiService(IOptions<MyobImportOptions> options)
        {
            _myobImportOptions = options.Value;
        }

        public string InsertZohoAccountMainContactToContactCustomer(ZohoAccount account, ZohoContact mainContact)
        {
            return string.Empty;
        }

        public bool IsZohoAccountExistInMyob(string zohoAccountUuid, string companyFileKey)
        {
            using (CustomerService service = new CustomerService(_myobImportOptions.MyobOptions, companyFileKey))
            {
                var customers = service.FilterByZohoAccountUuid(zohoAccountUuid);
                return customers?.Count > 0;
            }
        }

        public async Task<InventoryItem> GetInventoryItemByZohoProductIdAsync(string productUuid, string companyFileKey)
        {
            using (InventoryItemService service = new InventoryItemService(_myobImportOptions.MyobOptions,companyFileKey))
            {

                var items = await service.FilterByZohoProductUuidAsync(productUuid);
                return (items != null && items.Count > 0) ? items[0] : null;
            }
        }

        public async Task<List<Account>> GetAccountsByDisplayIdAsync(string displayId,string companyFileKey)
        {
            using (AccountService service = new AccountService(_myobImportOptions.MyobOptions, companyFileKey))
            {
                return await service.FilterByDisplayIdAsync(displayId);
            }
        }

        public bool IsZohoProductExistInMyob(string productUuid, string companyFileKey)
        {
            using (InventoryItemService service = new InventoryItemService(_myobImportOptions.MyobOptions, companyFileKey))
            {

                var items = service.FilterByZohoProductUuid(productUuid);
                return items?.Count > 0;
            }
        }

        public async Task<string> UpdateInventoryItemAsync(InventoryItem item, string companyFileKey)
        {
            using (InventoryItemService service = new InventoryItemService(_myobImportOptions.MyobOptions, companyFileKey))
            {
                return await service.UpdateAsync(item);
            }
        }

        public async Task<string> InsertInventoryItemAsync(InventoryItem item, string companyFileKey)
        {
            using (InventoryItemService service = new InventoryItemService(_myobImportOptions.MyobOptions, companyFileKey))
            {
                return await service.InsertAsync(item);
            }
        }

        public async Task<string> InsertContactCustomerAsync(Customer item, string companyFileKey)
        {
            using (CustomerService service = new CustomerService(_myobImportOptions.MyobOptions, companyFileKey))
            {
                return await service.InsertAsync(item);
            }
        }

    }
}

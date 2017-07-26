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

        public ProductImport ProductImportOptions => _myobImportOptions.ProductImport;

        public ContactCustomerImportOptions ContactCustomerImportOptions => _myobImportOptions.ContactCustomerImportOptions;

        public MyobApiService(IOptions<MyobImportOptions> options)
        {
            _myobImportOptions = options.Value;
        }

        public string InsertZohoAccountMainContactToContactCustomer(ZohoAccount account, ZohoContact mainContact)
        {
            return string.Empty;
        }

        public bool IsZohoAccountExistInMyob(string zohoAccountUuid)
        {
            using (CustomerService service = new CustomerService(_myobImportOptions.MyobOptions))
            {
                var customers = service.FilterByZohoAccountUuid(zohoAccountUuid);
                return customers?.Count > 0;
            }
        }

        public async Task<InventoryItem> GetInventoryItemByZohoProductIdAsync(string productUuid)
        {
            using (InventoryItemService service = new InventoryItemService(_myobImportOptions.MyobOptions))
            {

                var items = await service.FilterByZohoProductUuidAsync(productUuid);
                return (items != null && items.Count > 0) ? items[0] : null;
            }
        }

        public async Task<List<Account>> GetAccountsByDisplayIdAsync(string displayId)
        {
            using (AccountService service = new AccountService(_myobImportOptions.MyobOptions))
            {
                return await service.FilterByDisplayIdAsync(displayId);
            }
        }

        public bool IsZohoProductExistInMyob(string productUuid)
        {
            using (InventoryItemService service = new InventoryItemService(_myobImportOptions.MyobOptions))
            {

                var items = service.FilterByZohoProductUuid(productUuid);
                return items?.Count > 0;
            }
        }

        public async Task<string> UpdateInventoryItemAsync(InventoryItem item)
        {
            using (InventoryItemService service = new InventoryItemService(_myobImportOptions.MyobOptions))
            {
                return await service.UpdateAsync(item);
            }
        }

        public async Task<string> InsertInventoryItemAsync(InventoryItem item)
        {
            using (InventoryItemService service = new InventoryItemService(_myobImportOptions.MyobOptions))
            {
                return await service.InsertAsync(item);
            }
        }

        public async Task<string> InsertContactCustomerAsync(Customer item)
        {
            using (CustomerService service = new CustomerService(_myobImportOptions.MyobOptions))
            {
                return await service.InsertAsync(item);
            }
        }

    }
}

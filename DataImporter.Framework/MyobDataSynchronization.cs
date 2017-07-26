using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataImporter.Framework.Models;
using DataImporter.Framework.Repository;
using DataImporter.Framework.Services;
using MyobCoreProxy.Models;
using Newtonsoft.Json;
using ZohoCRMProxy;
using ZohoProduct = DataImporter.Framework.Models.ZohoProduct;

namespace DataImporter.Framework
{
    public class MyobDataSynchronization : ZohoImportBase
    {
        private readonly MyobApiService _myobApiService;
        private readonly string _zohoToken;
        public MyobDataSynchronization(MyobApiService apiService, IZohoCRMDataRepository zohoRepository, IEmailSender emailSender, string zohoToken) : base(zohoRepository, emailSender)
        {
            TableName = "zcrm_Products";

            PortalAction = "Zoho,Myob Product data Sync";
            _zohoToken = zohoToken;
            _myobApiService = apiService;
        }


        protected override async Task<PortalActionResult> ProcessImport(string id)
        {
            var product =
                ZohoRepository.Products.FirstOrDefault(
                    x => x.ProductID.Equals(id, StringComparison.CurrentCultureIgnoreCase));
            if (product == null)
            {
                return await Task.FromResult(new PortalActionResult
                {
                    IsSuccess = false,
                    Message = $"Could not find product:{id}"
                });
            }

            PortalActionResult result = await ValidateProduct(product);

            if (!result.IsSuccess)
            {
                return result;
            }

            var item = await _myobApiService.GetInventoryItemByZohoProductIdAsync(product.ProductID);

            MyobInventoryItemActionResult inventoryItemResult; 
            if (item == null)
            {
                inventoryItemResult = await CreateNewInventoryItemFromProductAsync(product);
            }
            else
            {
                inventoryItemResult = await UpdateInventoryItemByProductAsync(item, product);
            }

            if (!inventoryItemResult.IsSuccess)
            {
                return inventoryItemResult;
            }

            return await UpdateZohoProductMyobUidIfneededAsync(inventoryItemResult, product);

        }

        private async Task<PortalActionResult> UpdateZohoProductMyobUidIfneededAsync(MyobInventoryItemActionResult inventoryItemResult, ZohoProduct product)
        {
            if (!string.IsNullOrEmpty(product.MyobUuid) && inventoryItemResult.Item.Uid == new Guid(product.MyobUuid))
            {
                return new PortalActionResult
                {
                    IsSuccess = true
                };
            }

            OnDisplayMessage($"[Product:{product.ProductID}] Update Zoho Product Myob Uuid start");

            await ZohoRepository.AddActionLogAsync(new ZohoActionLog
            {
                TableName = TableName,
                Action = "[Start] Update Zoho Product Myob Uuid",
                ActionData =product.ProductID,
                ActionResult = string.Empty,
                CreatedBy = LoggerName,
                CreatedTime = DateTime.Now,
                Stageindicator = 1
            });

            var result = new PortalActionResult();

            using (ZohoCRMProxy.ZohoCRMProduct request = new ZohoCRMProduct(_zohoToken))
            {
                try
                {
                    var contactResult = request.UpdateProductMyobUuid(product.ProductID, inventoryItemResult.Item.Uid.ToString());
                    result.IsSuccess = true;
                    result.Message = "Update Zoho Product Myob Uuid Updated.";
                }
                catch (Exception e)
                {
                    result.IsSuccess = false;
                    result.Message = e.Message;

                }

            }

            OnDisplayMessage($"[Product:{product.ProductID}] Update Zoho Product Myob Uuid finished");

            await ZohoRepository.AddActionLogAsync(new ZohoActionLog
            {
                TableName = TableName,
                Action = string.Format("[{0}] {1}", result.IsSuccess == true ? "Finished" : "Error", "Update Zoho Product Myob Uuid"),
                ActionData = product.ProductID,
                ActionResult = JsonConvert.SerializeObject(new { ProductId = product.ProductID, MyobUid = inventoryItemResult.Item.Uid }),
                CreatedBy = LoggerName,
                CreatedTime = DateTime.Now,
                Stageindicator = 1
            });

            return result;
        }

        private async Task<MyobInventoryItemActionResult> UpdateInventoryItemByProductAsync(InventoryItem item, ZohoProduct product)
        {
            OnDisplayMessage($"[Product:{product.ProductID}] Update Inventory Item start");
            await ZohoRepository.AddActionLogAsync(new ZohoActionLog
            {
                TableName = TableName,
                Action = "[Start] Update Inventory Item by Product",
                ActionData = product.ProductID,
                ActionResult = string.Empty,
                CreatedBy = LoggerName,
                CreatedTime = DateTime.Now,
                Stageindicator = 1
            });

            item.Number = product.ProductCode;
            item.Name = product.MyobNameId;
            item.IsActive = !string.IsNullOrEmpty(product.ProductActive) && product.ProductActive.Trim().ToLower() == "true";

            item.CustomField1 = new Identifier
            {
                Label = "Zoho Product UUID",
                Value = product.ProductID
            };

            item.IsSold = true;

            var accounts = await _myobApiService.GetAccountsByDisplayIdAsync(product.MyobLinkedGl);
            var account = accounts[0];

            item.IncomeAccount = new AccountLink {UID = account.Uid};

            if (item.SellingDetails == null)
            {
                item.SellingDetails = new ItemSellingDetails
                {
                    IsTaxInclusive = false,
                    TaxCode = new TaxCodeLink {UID = new Guid(_myobApiService.ProductImportOptions.SellingTaxUid)}
                };
            }
            else
            {
                item.SellingDetails.IsTaxInclusive = false;
                item.SellingDetails.TaxCode =
                    new TaxCodeLink {UID = new Guid(_myobApiService.ProductImportOptions.SellingTaxUid)};
            }

            var result = await _myobApiService.UpdateInventoryItemAsync(item);

            await ZohoRepository.AddActionLogAsync(new ZohoActionLog
            {
                TableName = TableName,
                Action = string.Format("[{0}] {1}", "Finished", "Update Zoho Product Myob Uuid"),
                ActionData = product.ProductID,
                ActionResult = result,
                CreatedBy = LoggerName,
                CreatedTime = DateTime.Now,
                Stageindicator = 1
            });

            OnDisplayMessage($"[Product:{product.ProductID}] Update Inventory Item finished");

            return new MyobInventoryItemActionResult
            {
                IsSuccess = true,
                Item = item
            };
           
        }

        private async Task<MyobInventoryItemActionResult> CreateNewInventoryItemFromProductAsync(ZohoProduct product)
        {
            InventoryItem item = new InventoryItem();

            OnDisplayMessage($"[Product:{product.ProductID}] Insert Inventory Item start");

            await ZohoRepository.AddActionLogAsync(new ZohoActionLog
            {
                TableName = TableName,
                Action = "[Start] Insert Inventory Item by Product",
                ActionData = product.ProductID,
                ActionResult = string.Empty,
                CreatedBy = LoggerName,
                CreatedTime = DateTime.Now,
                Stageindicator = 1
            });

            item.Number = product.ProductCode;
            item.Name = product.MyobNameId;
            item.IsActive = !string.IsNullOrEmpty(product.ProductActive) &&
                            product.ProductActive.Trim().ToLower() == "true";
            item.CustomField1 = new Identifier
            {
                Label = "Zoho Product UUID",
                Value = product.ProductID
            };

            item.IsSold = true;

            var accounts = await _myobApiService.GetAccountsByDisplayIdAsync(product.MyobLinkedGl);
            var account = accounts[0];

            item.IncomeAccount = new AccountLink {UID = account.Uid};

            item.SellingDetails = new ItemSellingDetails
            {
                IsTaxInclusive = false,
                TaxCode =
                    new TaxCodeLink { UID = new Guid(_myobApiService.ProductImportOptions.SellingTaxUid) }
            };
            

            var result = await _myobApiService.InsertInventoryItemAsync(item);

            await ZohoRepository.AddActionLogAsync(new ZohoActionLog
            {
                TableName = TableName,
                Action = "[Finish] Update Inventory Item by Product",
                ActionData = product.ProductID,
                ActionResult = result,
                CreatedBy = LoggerName,
                CreatedTime = DateTime.Now,
                Stageindicator = 1
            });

            OnDisplayMessage($"[Product:{product.ProductID}] Insert Inventory Item finished");

            item.Uid = new Guid(result);

            return new MyobInventoryItemActionResult
            {
                IsSuccess = true,
                Item = item
            };
        }

        private async Task<PortalActionResult> ValidateProduct(ZohoProduct product)
        {
            var message = new StringBuilder();

            var result = new PortalActionResult
            {
                IsSuccess =  true
            };

            if (string.IsNullOrEmpty(product.ProductCode))
            {
                result.IsSuccess = false;
                message.AppendLine($"[product:{product.ProductID}]: prodcut code is empty");
            }

            if (string.IsNullOrEmpty(product.MyobNameId))
            {
                result.IsSuccess = false;
                message.AppendLine($"[product:{product.ProductID}]: myob name id is empty");
            }

            if (string.IsNullOrEmpty(product.Description))
            {
                result.IsSuccess = false;
                message.AppendLine($"[product:{product.ProductID}]: description is empty");
            }

            if (string.IsNullOrEmpty(product.MyobLinkedGl))
            {
                result.IsSuccess = false;
                message.AppendLine($"[product:{product.ProductID}]: myob linked GL is empty");
            }
            else
            {

                var items = await _myobApiService.GetAccountsByDisplayIdAsync(product.MyobLinkedGl);
                if (items == null)
                {
                    result.IsSuccess = false;
                    message.AppendLine(
                        $"[product:{product.ProductID}]: find more than 1  Myob GL account by displayid {product.MyobLinkedGl}");
                }
                else
                {
                    if (items.Count > 1)
                    {
                        result.IsSuccess = false;
                        message.AppendLine(
                            $"[product:{product.ProductID}]: could not find Myob GL account by displayid {product.MyobLinkedGl}");
                    }
                }
            }

            result.Message = message.ToString();

            return result;
            
        }
    }
}

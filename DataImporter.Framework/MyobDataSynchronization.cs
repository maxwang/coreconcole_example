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

            var myobConfigurations = ZohoRepository.ZohoProductMyobConfigurations.Where(x => x.ProductId == id).ToList();

            if (myobConfigurations == null || myobConfigurations.Count == 0)
            {
                return await Task.FromResult(new PortalActionResult
                {
                    IsSuccess = false,
                    Message = $"[Product:{id}]Could not find product myob configuration"
                });
            }


            StringBuilder message = new StringBuilder();

            foreach (var config in myobConfigurations)
            {
                PortalActionResult importResult = await ImportProdubtbyTaxAsync(product, config);
                result.IsSuccess = result.IsSuccess && importResult.IsSuccess;
                message.AppendLine(importResult.Message);
            }

            result.Message = message.ToString();


            return result;

        }

        private async Task<PortalActionResult> ImportProdubtbyTaxAsync(ZohoProduct product, ZohoProductMyobConfiguration config)
        {
            if (!_myobApiService.ProductImportOptions.ContainsKey(config.TaxCode))
            {
                return new PortalActionResult
                {
                    IsSuccess = false,
                    Message = $"[Product:{product.ProductID}] {config.TaxCode} Could not find configuration"
                };
            }

            var productImportOption = _myobApiService.ProductImportOptions[config.TaxCode];

            if (!_myobApiService.MyobOptions.MyobCompanyFileOptions.ContainsKey(productImportOption.MyobCompanyFileKey))
            {
                return new PortalActionResult
                {
                    IsSuccess = false,
                    Message = $"[Product:{product.ProductID}] {config.TaxCode} Could not find company file"
                };
            }
            
            var item = await _myobApiService.GetInventoryItemByZohoProductIdAsync(product.ProductID,
                productImportOption.MyobCompanyFileKey);

            MyobInventoryItemActionResult inventoryItemResult;
            if (item == null)
            {
                inventoryItemResult = await CreateNewInventoryItemFromProductAsync(product, config, productImportOption.MyobCompanyFileKey);
            }
            else
            {
                inventoryItemResult = await UpdateInventoryItemByProductAsync(item, product, config, productImportOption.MyobCompanyFileKey);
            }

            if (!inventoryItemResult.IsSuccess)
            {
                return inventoryItemResult;
            }

            return await UpdateZohoProductMyobUidIfneededAsync(inventoryItemResult, config);
        }

        private async Task<PortalActionResult> UpdateZohoProductMyobUidIfneededAsync(MyobInventoryItemActionResult inventoryItemResult, ZohoProductMyobConfiguration config)
        {
            if (!string.IsNullOrEmpty(config.MyobUuid) && inventoryItemResult.Item.Uid == new Guid(config.MyobUuid))
            {
                return new PortalActionResult
                {
                    IsSuccess = true
                };
            }

            OnDisplayMessage($"[Product:{config.ProductId}] {config.TaxCode} Update Zoho Product Myob Uuid start");

            await ZohoRepository.AddActionLogAsync(new ZohoActionLog
            {
                TableName = TableName,
                Action = "[Start] Update Zoho Product Myob Uuid",
                ActionData = $"{config.ProductId}:{config.TaxCode}",
                ActionResult = string.Empty,
                CreatedBy = LoggerName,
                CreatedTime = DateTime.Now,
                Stageindicator = 1
            });

            var result = new PortalActionResult();

            config.MyobUuid = inventoryItemResult.Item.Uid.ToString();
            config.ModifiedTime = DateTime.Now;
            config.ModifiedBy = LoggerName;

            result.IsSuccess = await ZohoRepository.UpdateProductMyobUuidAsync(config);

            //Not Uupdate Zoho, update product link table
            //using (ZohoCRMProxy.ZohoCRMProduct request = new ZohoCRMProduct(_zohoToken))
            //{
            //    try
            //    {
            //        var contactResult = request.UpdateProductMyobUuid(product.ProductID, inventoryItemResult.Item.Uid.ToString());
            //        result.IsSuccess = true;
            //        result.Message = "Update Zoho Product Myob Uuid Updated.";
            //    }
            //    catch (Exception e)
            //    {
            //        result.IsSuccess = false;
            //        result.Message = e.Message;

            //    }

            //}

            OnDisplayMessage($"[Product:{config.ProductId}] {config.TaxCode} Update Zoho Product Myob Uuid finished");

            await ZohoRepository.AddActionLogAsync(new ZohoActionLog
            {
                TableName = TableName,
                Action = string.Format("[{0}] {1}", result.IsSuccess == true ? "Finished" : "Error", "Update Zoho Product Myob Uuid"),
                ActionData = config.ProductId,
                ActionResult = JsonConvert.SerializeObject(new { ProductId = config.ProductId, TaxCode = config.TaxCode, MyobUid = inventoryItemResult.Item.Uid }),
                CreatedBy = LoggerName,
                CreatedTime = DateTime.Now,
                Stageindicator = 1
            });

            return result;
        }

        private async Task<MyobInventoryItemActionResult> UpdateInventoryItemByProductAsync(InventoryItem item, ZohoProduct product, ZohoProductMyobConfiguration config, string myobCompanyFileKey)
        {
            OnDisplayMessage($"[Product:{product.ProductID}] {config.TaxCode} Update Inventory Item start");

            //need generate products by TAX code

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
            item.Name = config.MyobNamedId;
            item.IsActive = !string.IsNullOrEmpty(product.ProductActive) && product.ProductActive.Trim().ToLower() == "true";

            item.CustomField1 = new Identifier
            {
                Label = "Zoho Product UUID",
                Value = product.ProductID
            };

            item.IsSold = true;

            var accounts = await _myobApiService.GetAccountsByDisplayIdAsync(config.MyobLinkedGl, myobCompanyFileKey);
            var account = accounts[0];

            item.IncomeAccount = new AccountLink {UID = account.Uid};

            if (item.SellingDetails == null)
            {
                item.SellingDetails = new ItemSellingDetails
                {
                    IsTaxInclusive = false,
                    TaxCode = new TaxCodeLink {UID = new Guid(_myobApiService.ProductImportOptions[config.TaxCode].SellingTaxUid)}
                };
            }
            else
            {
                item.SellingDetails.IsTaxInclusive = false;
                item.SellingDetails.TaxCode =
                    new TaxCodeLink {UID = new Guid(_myobApiService.ProductImportOptions[config.TaxCode].SellingTaxUid)};
            }

            var result = await _myobApiService.UpdateInventoryItemAsync(item, myobCompanyFileKey);

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

        private async Task<MyobInventoryItemActionResult> CreateNewInventoryItemFromProductAsync(ZohoProduct product, ZohoProductMyobConfiguration config, string myobCompanyFileKey)
        {
            InventoryItem item = new InventoryItem();

            OnDisplayMessage($"[Product:{product.ProductID}] {config.TaxCode} Insert Inventory Item start");

            await ZohoRepository.AddActionLogAsync(new ZohoActionLog
            {
                TableName = TableName,
                Action = "[Start] Insert Inventory Item by Product",
                ActionData = $"Taxcode:{config.TaxCode}, ProductId:{product.ProductID}",
                ActionResult = string.Empty,
                CreatedBy = LoggerName,
                CreatedTime = DateTime.Now,
                Stageindicator = 1
            });

            item.Number = product.ProductCode;
            item.Name = config.MyobNamedId;
            item.IsActive = !string.IsNullOrEmpty(product.ProductActive) &&
                            product.ProductActive.Trim().ToLower() == "true";
            item.CustomField1 = new Identifier
            {
                Label = "Zoho Product UUID",
                Value = product.ProductID
            };

            item.IsSold = true;

            var accounts = await _myobApiService.GetAccountsByDisplayIdAsync(config.MyobLinkedGl, myobCompanyFileKey);
            var account = accounts[0];

            item.IncomeAccount = new AccountLink {UID = account.Uid};

            item.SellingDetails = new ItemSellingDetails
            {
                IsTaxInclusive = false,
                TaxCode =
                    new TaxCodeLink { UID = new Guid(_myobApiService.ProductImportOptions[config.TaxCode].SellingTaxUid) }
            };
            

            var result = await _myobApiService.InsertInventoryItemAsync(item, myobCompanyFileKey);

            await ZohoRepository.AddActionLogAsync(new ZohoActionLog
            {
                TableName = TableName,
                Action = "[Finish] Update Inventory Item by Product",
                ActionData = product.ProductID,
                ActionResult = $"Taxcode:{config.TaxCode}, Result: {result}",
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

            var configurations = await ZohoRepository.GetProductMyobConfigurations(product.ProductID);

            if (string.IsNullOrEmpty(product.Description))
            {
                result.IsSuccess = false;
                message.AppendLine($"[product:{product.ProductID}]: description is empty");
            }

            foreach (var config in configurations)
            {
                if (string.IsNullOrEmpty(config.MyobNamedId))
                {
                    result.IsSuccess = false;
                    message.AppendLine($"[product:{product.ProductID}]: {config.TaxCode} myob name id is empty");
                }

                if (string.IsNullOrEmpty(config.MyobLinkedGl))
                {
                    result.IsSuccess = false;
                    message.AppendLine($"[product:{product.ProductID}]: {config.TaxCode} myob linked GL is empty");
                }

            }
            

            //if (string.IsNullOrEmpty(product.MyobLinkedGl))
            //{
            //    result.IsSuccess = false;
            //    message.AppendLine($"[product:{product.ProductID}]: myob linked GL is empty");
            //}
            //else
            //{

            //    var items = await _myobApiService.GetAccountsByDisplayIdAsync(product.MyobLinkedGl);
            //    if (items == null)
            //    {
            //        result.IsSuccess = false;
            //        message.AppendLine(
            //            $"[product:{product.ProductID}]: find more than 1  Myob GL account by displayid {product.MyobLinkedGl}");
            //    }
            //    else
            //    {
            //        if (items.Count > 1)
            //        {
            //            result.IsSuccess = false;
            //            message.AppendLine(
            //                $"[product:{product.ProductID}]: could not find Myob GL account by displayid {product.MyobLinkedGl}");
            //        }
            //    }
            //}

            result.Message = message.ToString();

            return result;
            
        }
    }
}

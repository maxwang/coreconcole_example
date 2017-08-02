using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DataImporter.Framework.Repository;
using DataImporter.Framework.Services;
using System.Threading;
using System.Linq;
using DataImporter.Framework.Models;
using Website.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using DataImporter.Framework.Extensions;
using DataImporter.Framework.Data;
using Microsoft.EntityFrameworkCore;
using MyobCoreProxy.Models;
using Newtonsoft.Json;
using ZohoCRMProxy;
using ZohoAccount = DataImporter.Framework.Models.ZohoAccount;
using ZohoContact = DataImporter.Framework.Models.ZohoContact;

namespace DataImporter.Framework
{
    public class PartnerPortalImporter : ZohoImportBase
    {
        private readonly MyobApiService _myobApiService;
        private readonly SMSUserManager<ApplicationUser> _userManager;
        private readonly string _tempPassword;
        private readonly string _zohoToken;

        public PartnerPortalImporter(MyobApiService apiService, IZohoCRMDataRepository zohoRepository, IEmailSender emailSender, string zohoToken) : base(zohoRepository, emailSender)
        {
            TableName = "zcrm_PartnerPortal";

            PortalAction = "Add User and Company to ACL";

            _tempPassword = "Abcde23$";
            _zohoToken = zohoToken;
            _myobApiService = apiService;

            var pwdValidators = new List<PasswordValidator<ApplicationUser>>();
            var pwdValidator = new PasswordValidator<ApplicationUser>();

            pwdValidators.Add(pwdValidator);

            var userValidators = new List<IUserValidator<ApplicationUser>>();
            var validator = new UserValidator<ApplicationUser>();
            userValidators.Add(validator);

            var log = new LoggerFactory();

            _userManager = new SMSUserManager<ApplicationUser>(
                    new SMSUserStore<ApplicationUser>(new ACLDbContext(new DbContextOptions<ACLDbContext>
                    {

                    })),
                    null,
                    new PasswordHasher<ApplicationUser>(),
                    userValidators,
                    pwdValidators,
                    new UpperInvariantLookupNormalizer(),
                    new IdentityErrorDescriber(),
                    null,
                    new Logger<UserManager<ApplicationUser>>(log));
        }
                
        
        protected override async Task<PortalActionResult> ProcessImport(string id = "")
        {
            var partnerPortal = ZohoRepository.PartnerPortals.FirstOrDefault(x => x.PartnerPortalID.Equals(id));
            if(partnerPortal == null)
            {
                return new PortalActionResult
                {
                    IsSuccess = false,
                    Message = string.Format("Could not find partner portal record for id:{0}", id)
                };
            }

            var accountId = partnerPortal.PartnerAccount;

            var contactId = partnerPortal.PortalAdmin;

            //check User already has a PortalAdmin 
            var count = ZohoRepository.PartnerPortals.Count(x => x.PartnerAccount == accountId);
            if (count > 1)
            {
                return new PortalActionResult
                {
                    IsSuccess = false,
                    Message = string.Format("Only allow one Portal Admin for Account :{0}", accountId)
                };
            }

            //check only one contact should have portal admin enabled
            var adminContact = ZohoRepository.Contacts.FirstOrDefault(x => x.AccountID == accountId && x.PortalAdmin == true);
            if (adminContact != null)
            {
                return new PortalActionResult
                {
                    IsSuccess = false,
                    Message = string.Format("Account :{0} already have a portal admin {1} ignore this update",
                        accountId, adminContact.ContactID)
                };
            }


            //in ACL, we do not care about Account information, we only link Company to Account using contact id
            //var account = _zohoRepository.Accounts.FirstOrDefault(x => x.AccountID == accountId);
            //if (account == null)
            //{
            //    return new PortalActionResult
            //    {
            //        IsSuccess = false,
            //        Resutl = string.Format("Could not find partner account:{0}", accountId)
            //    };
            //}

            var contact = ZohoRepository.Contacts.FirstOrDefault(x => x.ContactID == contactId);
            if (contact == null)
            {
                return new PortalActionResult
                {
                    IsSuccess = false,
                    Message = string.Format("Could not find partner portal admin contact:{0}", contactId)
                };
            }

            PortalActionResult myobResult = await MyobContactCustomerDataSyncAysnc(accountId, id);
            if (!myobResult.IsSuccess)
            {
                return myobResult;
            }


            var subject = myobResult.IsSuccess
                ? $"[Account:{accountId}] added to Myob"
                : $"[Account:{accountId}] could not add to Myob";

            var tos = _myobApiService.SalesEmail.Split(new char[] {';'}).ToList();
            var body = string.IsNullOrEmpty(myobResult.Message) ? "Success" : myobResult.Message;

            await EmailSender.SendEmailAsync(subject, body, tos);

            //check contact created in company
            var company = await _userManager.GetCompanyByZohoAccountIdAsync(accountId);
            int companyId;
            if (company == null)
            {
                companyId = await _userManager.CreateCompanyAsync(new Company
                {
                    CompanyZohoAccountId = accountId,
                    CreatedBy = "importer",
                    CreatedTime = DateTime.Now
                });
            }
            else 
            {
                companyId = company.Id;
            }


            //check user information

            var user = await _userManager.FindByEmailAsync(contact.Email);
            var message = string.Empty;
            var isSuccess = false;
            if (user == null)
            {
                //create user
                var now = DateTime.Now;
                var tempuser = new ApplicationUser() { UserName = contact.Email, Email = contact.Email, LastPasswordChangedDate = now, CreationDate = now, CompanyId = companyId };
                var result = await _userManager.CreateAsync(tempuser, _tempPassword);

                if (result.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(tempuser, DefaultRoleName);

                    var userContact = await _userManager.CreateUserZohoContactAsync(new UserZohoContact
                    {
                        UserId = tempuser.Id,
                        ZohoContactId = contactId
                    });

                    if (roleResult.Succeeded)
                    {
                        isSuccess = true;
                        message = string.Format("User Created for:{0}", tempuser.UserName);
                    }
                    else
                    {
                        message = roleResult.ToString();
                    }

                }
                else
                {
                    
                    message = result.ToString();
                }
            }
            else 
            {
                if(!user.CompanyId.HasValue || user.CompanyId.Value < 1)
                {
                    message = string.Format("User {0} already exist in ACL but Could not find Company infomraiton for User:", user.UserName);
                }
                else
                {
                    var companyInfo = await _userManager.GetCompanyAsync(user.CompanyId.Value);
                    if(companyInfo == null)
                    {
                        message = string.Format("User {0} already exist in ACL but Could not find Company infomraiton for User:", user.UserName);
                    }
                    else
                    {
                        if(!companyInfo.CompanyZohoAccountId.Equals(accountId))
                        {
                            message = string.Format("User {0} already exist in ACL but belongs to another account:{1}", user.UserName, companyInfo.CompanyZohoAccountId);
                        }
                        else
                        {
                            var userContact = await _userManager.GetUserZohoContactAsync(user.Id);
                            if (userContact == null)
                            {
                                message = string.Format("User {0} already exist in ACL, but could not find contact id with this use", user.UserName);
                            }
                            else if (userContact.ZohoContactId != contactId)
                            {
                                message = string.Format("User {0} already exist in ACL, with different contact id:{1}[should be {2}]", user.UserName, userContact.ZohoContactId, contactId);
                            }
                            else
                            {

                                if (await _userManager.IsInRoleAsync(user, DefaultRoleName) == false)
                                {
                                    var roleAdded = await _userManager.AddToRoleAsync(user, DefaultRoleName);
                                    if (roleAdded.Succeeded)
                                    {
                                        isSuccess = true;
                                        message = string.Format("User role added for user :{0}", user.UserName);
                                    }
                                }
                                else
                                {
                                    isSuccess = true;
                                    message = string.Format("user exist in system, do not import", user.UserName);
                                }
                            }
                        }
                    }
                }    
            }

            if (isSuccess)
            {
                var updateResult = UpdateZohoIfNeeded(contact, partnerPortal);
                return new PortalActionResult
                {
                    IsSuccess = updateResult.IsSuccess,
                    Message = string.Format("{0}\r\n{1}", message, updateResult.Message)
                };
            }
            else
            {
                return new PortalActionResult
                {
                    IsSuccess = false,
                    Message = message
                };

            }
            
        }

        private async Task<PortalActionResult> MyobContactCustomerDataSyncAysnc(string accountId, string partnerPortalId)
        {
            var account = ZohoRepository.Accounts.FirstOrDefault(x => x.AccountId == accountId);
            if (account == null)
            {
                return new PortalActionResult
                {
                    IsSuccess = false,
                    Message = string.Format($"Could not find partner account:{0}", accountId)
                };
            }

            PortalActionResult result = ValidateAccount(account);

            if (!result.IsSuccess)
            {
                return result;
            }

            var accountExist = _myobApiService.IsZohoAccountExistInMyob(account.AccountId, account.MyobDataFile);
            if (accountExist)
            {
                return new PortalActionResult
                {
                    IsSuccess = true,
                    Message = $"[Account:{account.AccountId}]: {account.AccountName} already exist in Myob"
                };
            }
            
            PortalActionResult insertResult = await InsertMyobContactCustomer(account, partnerPortalId);

            return insertResult;
            
        }

        private async Task<PortalActionResult> InsertMyobContactCustomer(ZohoAccount account, string partnerPortalId)
        {
            Customer item = new Customer();

            OnDisplayMessage($"[PartnerPortal:{partnerPortalId}] Insert Myob Contact->Customer");

            await ZohoRepository.AddActionLogAsync(new ZohoActionLog
            {
                TableName = TableName,
                Action = "[Start] Partner Portal Insert Myob Customer",
                ActionData = account.AccountId,
                ActionResult = string.Empty,
                CreatedBy = LoggerName,
                CreatedTime = DateTime.Now,
                Stageindicator = 1
            });

            item.IsIndividual = false;
            item.IsActive = true;
            item.CompanyName =  account.AccountName;
            
            item.CustomField1 = new Identifier
            {
                Label = "Zoho Acc UUID",
                Value = account.AccountId
            };

            item.Addresses = new List<Address>
            {
                new Address
                {
                    Location = 1,
                    Street = account.BillingStreet,
                    City = account.BillingCity,
                    State = account.BillingState,
                    PostCode = account.BillingCode,
                    Phone1 = account.Phone,
                    Fax = account.Fax,
                    Email = account.CompanyEmail,
                    Website = account.Website
                }
            };

            var sellingDetailsOptions = _myobApiService.ContactCustomerImportOptions[account.MyobDataFile].SellingDetailsOptions;

            item.SellingDetails = new CustomerSellingDetails
            {
                SaleLayout = (InvoiceLayoutType)Enum.Parse(typeof(InvoiceLayoutType),  sellingDetailsOptions.SaleLayout),
                PrintedForm = sellingDetailsOptions.PrintedForm,
                InvoiceDelivery = (DocumentAction)Enum.Parse(typeof(DocumentAction), sellingDetailsOptions.InvoiceDelivery),
                ABN = account.AbnCompanyNum,
                TaxCode = new TaxCodeLink
                    { UID = new Guid(sellingDetailsOptions.TaxCode) },
                FreightTaxCode = new TaxCodeLink
                { UID = new Guid(sellingDetailsOptions.FreightTaxCode)  },
                UseCustomerTaxCode = true,
                Terms = new CustomerTerms
                {
                    PaymentIsDue = (TermsPaymentType)Enum.Parse(typeof(TermsPaymentType), sellingDetailsOptions.TermsPaymentIsDue)
                }
            };
            
            var result = await _myobApiService.InsertContactCustomerAsync(item, account.MyobDataFile);

            OnDisplayMessage($"[PartnerPortal:{partnerPortalId}] Insert Myob Contact->Customer Finished");

            await ZohoRepository.AddActionLogAsync(new ZohoActionLog
            {
                TableName = TableName,
                Action = "[Finish] Partner Portal Insert Myob Customer",
                ActionData = partnerPortalId,
                ActionResult = JsonConvert.SerializeObject(new {Request = item, Response = result}),
                CreatedBy = LoggerName,
                CreatedTime = DateTime.Now,
                Stageindicator = 1
            });

            item.Uid = new Guid(result);

            return new PortalActionResult
            {
                IsSuccess = true
            };
        }


        private PortalActionResult ValidateAccount(Models.ZohoAccount account)
        {
            var message = new StringBuilder();

            var result = new PortalActionResult
            {
                IsSuccess = true
            };

            if (string.IsNullOrEmpty(account.AccountName))
            {
                result.IsSuccess = false;
                message.AppendLine("Account Name is empty");
            }

            if (string.IsNullOrEmpty(account.Phone))
            {
                result.IsSuccess = false;
                message.AppendLine("Phone is empty");
            }
            
            //if (string.IsNullOrEmpty(account.Fax))
            //{
            //    result.IsSuccess = false;
            //    message.AppendLine("Fax is empty");
            //}
            
            //if (string.IsNullOrEmpty(account.Website))
            //{
            //    result.IsSuccess = false;
            //    message.AppendLine("Website is empty");
            //}

            //
            if (string.IsNullOrEmpty(account.BillingStreet))
            {
                result.IsSuccess = false;
                message.AppendLine("Billing Street is empty");
            }

            if (string.IsNullOrEmpty(account.BillingCity))
            {
                result.IsSuccess = false;
                message.AppendLine("Billing City is empty");
            }

            if (string.IsNullOrEmpty(account.BillingState))
            {
                result.IsSuccess = false;
                message.AppendLine("Billing State is empty");
            }

            if (string.IsNullOrEmpty(account.BillingCode))
            {
                result.IsSuccess = false;
                message.AppendLine("Billing Postcode is empty");
            }

            if (string.IsNullOrEmpty(account.BillingCountry))
            {
                result.IsSuccess = false;
                message.AppendLine("Billing Country is empty");
            }

            //if (string.IsNullOrEmpty(account.AbnCompanyNum))
            //{
            //    result.IsSuccess = false;
            //    message.AppendLine("Company ABN is empty");
            //}

            if (string.IsNullOrEmpty(account.MyobDataFile) ||
                !_myobApiService.MyobOptions.MyobCompanyFileOptions.ContainsKey(account.MyobDataFile))
            {
                result.IsSuccess = false;
                message.AppendLine($"Account {account.AccountId} myob Data file {account.MyobDataFile} mapping not exist");
            }

            if (string.IsNullOrEmpty(account.MyobDataFile) ||
                !_myobApiService.ContactCustomerImportOptions.ContainsKey(account.MyobDataFile))
            {
                result.IsSuccess = false;
                message.AppendLine($"Account {account.AccountId} myob Contact Customer options {account.MyobDataFile} not exist");
            }


            result.Message = message.ToString();
            return result;
        }

        private PortalActionResult UpdateZohoIfNeeded(ZohoContact contact, Models.ZohoPartnerPortal partnerPortal)
        {
            PortalActionResult result = new PortalActionResult();

            StringBuilder message = new StringBuilder();

            result.IsSuccess = false;

            var needUpdateContact = true;
            var needUpdatePartnerPortal = true;

            if (contact.PortalAdmin)
            {
                needUpdateContact = false;
                message.AppendLine("Portal Admin is true in Database, ignore update");
            }

            if (partnerPortal.ACLCreated.Equals("true", StringComparison.CurrentCultureIgnoreCase))
            {
                needUpdatePartnerPortal = false;
                message.AppendLine("Portal Portal ACL Created is true in Database, ignore update");
            }

            if (needUpdateContact)
            {
                var zohoContact = new ZohoCRMProxy.ZohoContact
                {
                    Id = contact.ContactID,
                    PortalAdmin = true,
                    PartnerPortalEnabled =
                        contact.PartnerPortalEnabled.Equals("true", StringComparison.CurrentCultureIgnoreCase),
                    SAPUnqualified = contact.SAPUnqualified.Equals("true", StringComparison.CurrentCultureIgnoreCase),
                    EmailOptOut = contact.EmailOptOut.Equals("true", StringComparison.CurrentCultureIgnoreCase),
                };

                using (ZohoCRMProxy.ZohoCRMContact request = new ZohoCRMContact(_zohoToken))
                {
                    try
                    {
                        var contactResult = request.Update(zohoContact);
                        result.IsSuccess = true;
                        message.AppendLine("Zoho Contact Portal Admin Updated.");
                    }
                    catch (Exception e)
                    {
                        result.IsSuccess = false;
                        message.AppendLine(e.Message);
             
                    }

                }
            }

            if (needUpdatePartnerPortal && result.IsSuccess)
            {
                var portal = new ZohoCRMProxy.ZohoPartnerPortal
                {
                    Id = partnerPortal.PartnerPortalID,
                    ACLCreated = true
                };
                using (ZohoCRMProxy.ZohoCRMPartnerPortal portalRequest = new ZohoCRMProxy.ZohoCRMPartnerPortal(_zohoToken))
                {
                    try
                    {
                        portalRequest.Update(portal);
                        result.IsSuccess = true;
                        message.AppendLine("Zoho Partner Portal ACL Created Updated.");

                    }
                    catch (Exception e)
                    {
                        result.IsSuccess = false;
                        message.AppendLine(e.Message);
                    }
                }
            }

            result.Message = message.ToString();

            return result;
        }


    }
}

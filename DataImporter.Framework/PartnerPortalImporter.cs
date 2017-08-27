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
using Newtonsoft.Json;
using ZohoCRMProxy;
using ZohoAccount = DataImporter.Framework.Models.ZohoAccount;
using ZohoContact = DataImporter.Framework.Models.ZohoContact;
using MyobProxy.Models;

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

            string validateResult = await ValidatePartnerPortalAsync(id);

            if (!string.IsNullOrEmpty(validateResult))
            {
                return new PortalActionResult
                {
                    IsSuccess = false,
                    Message = validateResult
                };
            }

            var partnerPortal =
                ZohoRepository.PartnerPortals.FirstOrDefault(x => x.PartnerPortalID.Equals(id));

            var accountId = partnerPortal.PartnerAccount;

            var contactId = partnerPortal.PortalAdmin;

            PortalActionResult myobResult = await MyobContactCustomerDataSyncAysnc(accountId, id);
            if (!myobResult.IsSuccess)
            {
                return myobResult;
            }


            var subject = myobResult.IsSuccess
                ? $"[Account:{accountId}] added to Myob"
                : $"[Account:{accountId}] could not add to Myob";

            var tos = _myobApiService.SalesEmail.Split(new char[] { ';' }).ToList();
            var body = string.IsNullOrEmpty(myobResult.Message) ? "Success" : myobResult.Message;

            //do not need wait for email result
            EmailSender.SendEmailAsync(subject, body, tos);


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

            string oldPortalAdminZohoContactId = await UpdateOldPortalAdminIfNeededAsync(companyId, contactId);

            //check user information
            var contact = ZohoRepository.Contacts.FirstOrDefault(x => x.ContactID == contactId);

            var user = await _userManager.FindByEmailAsync(contact.Email);
            var message = string.Empty;
            var isSuccess = false;
            if (user == null)
            {
                PortalActionResult updateResult
                    = await CreateUserAndAddUserAsPortalAdminAsync(contact, companyId);

                isSuccess = updateResult.IsSuccess;
                message = updateResult.Message;

                //create user
                
            }
            else
            {
                var updateResult
                    = await UpdateExistingUserAsPortalAdminAsync(user, companyId);

                isSuccess = updateResult.IsSuccess;
                message = updateResult.Message;
            }

            if (isSuccess)
            {
                var updateResult = UpdateZohoIfNeeded(contact, partnerPortal, oldPortalAdminZohoContactId);
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

        private async Task<PortalActionResult> CreateUserAndAddUserAsPortalAdminAsync(ZohoContact contact,
            int companyId)
        {
            bool isSuccess = false;
            string message;

            var now = DateTime.Now;
            var tempuser = new ApplicationUser() { UserName = contact.Email, Email = contact.Email, LastPasswordChangedDate = now, CreationDate = now, CompanyId = companyId };
            var result = await _userManager.CreateAsync(tempuser, _tempPassword);

            if (result.Succeeded)
            {
                var roleResult = await _userManager.AddToRoleAsync(tempuser, DefaultRoleName);

                var userContact = await _userManager.CreateUserZohoContactAsync(new UserZohoContact
                {
                    UserId = tempuser.Id,
                    ZohoContactId = contact.ContactID
                });

                if (roleResult.Succeeded)
                {
                    isSuccess = true;
                    message = $"User Created for:{tempuser.UserName}";
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

            return new PortalActionResult
            {
                IsSuccess = isSuccess,
                Message =  message
            };
        }

        private async Task<PortalActionResult> UpdateExistingUserAsPortalAdminAsync(ApplicationUser user, int companyId)
        {
            //update company id
            if (user.CompanyId.HasValue)
            {
                if (user.CompanyId != companyId)
                {
                    //double check
                    return new PortalActionResult
                    {
                        IsSuccess = false,
                        Message = $"User {user.Id}: {user.UserName} already exist in ACL and belong to other acount"
                    };
                }
            }
            else
            {
                user.CompanyId = companyId;
                await _userManager.UpdateAsync(user);
            }


            // do not check contact information, validation method handle it

            //check role
            if (await _userManager.IsInRoleAsync(user, DefaultRoleName))
            {
                return new PortalActionResult
                {
                    IsSuccess = true,
                    Message = $"user {user.UserName} exist in system, do not import"
                };
            }

            var roleAdded = await _userManager.AddToRoleAsync(user, DefaultRoleName);

            return roleAdded.Succeeded
                ? new PortalActionResult
                {
                    IsSuccess = true,
                    Message = $"User {user.UserName} Added to External Admin Role"
                }
                : new PortalActionResult
                {
                    IsSuccess = false,
                    Message = $"Could not add user {user.UserName} to {DefaultRoleName}"
                };
        }

        private async Task<string> UpdateOldPortalAdminIfNeededAsync(int companyId, string zohoContactId)
        {
            var users = await _userManager.GetApplicationUsersByCompanyId(companyId);

            string oldPortalAdmin = string.Empty;

            if (users != null && users.Count > 0)
            {
                foreach (var user in users)
                {
                    if (await _userManager.IsInRoleAsync(user, DefaultRoleName))
                    {
                        var contact = await _userManager.GetUserZohoContactAsync(user.Id);
                        if (!string.IsNullOrEmpty(contact?.ZohoContactId) && contact.ZohoContactId != zohoContactId)
                        {
                            oldPortalAdmin = contact.ZohoContactId;
                            await _userManager.RemoveFromRoleAsync(user, DefaultRoleName);

                        }
                    }
                }
            }

            return oldPortalAdmin;
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
            item.CompanyName = account.AccountName;

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
                    Country = account.BillingCountry,
                    Phone1 = account.Phone,
                    Fax = account.Fax,
                    Email = account.CompanyEmail,
                    Website = account.Website
                }
            };

            var sellingDetailsOptions = _myobApiService.ContactCustomerImportOptions[account.MyobDataFile].SellingDetailsOptions;

            item.SellingDetails = new CustomerSellingDetails
            {
                SaleLayout = (InvoiceLayoutType)Enum.Parse(typeof(InvoiceLayoutType), sellingDetailsOptions.SaleLayout),
                PrintedForm = sellingDetailsOptions.PrintedForm,
                InvoiceDelivery = (DocumentAction)Enum.Parse(typeof(DocumentAction), sellingDetailsOptions.InvoiceDelivery),
                ABN = account.AbnCompanyNum,
                TaxCode = new TaxCodeLink
                { UID = new Guid(sellingDetailsOptions.TaxCode) },
                FreightTaxCode = new TaxCodeLink
                { UID = new Guid(sellingDetailsOptions.FreightTaxCode) },
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
                ActionResult = JsonConvert.SerializeObject(new { Request = item, Response = result }),
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

        private async Task<string> ValidatePartnerPortalAsync(string partnerPortalId)
        {

            if (string.IsNullOrEmpty(partnerPortalId))
            {
                return "Could not find Partner Portal Id";
            }

            var partnerPortal =
                ZohoRepository.PartnerPortals.FirstOrDefault(x => x.PartnerPortalID.Equals(partnerPortalId));
            if (partnerPortal == null)
            {
                return $"Could not find partner portal record for id:{partnerPortalId}";
            }

            var accountId = partnerPortal.PartnerAccount;

            var contactId = partnerPortal.PortalAdmin;

            //check User already has a PortalAdmin 
            var count = ZohoRepository.PartnerPortals.Count(x => x.PartnerAccount == accountId);
            if (count > 1)
            {
                return $"Only allow one Portal Admin for Account :{accountId}";

            }

            //check only one contact should have portal admin enabled
            //2017-08-03 we enabled partner portal reassign portal admin
            //var adminContact = ZohoRepository.Contacts.FirstOrDefault(x => x.AccountID == accountId && x.PortalAdmin == true);
            //if (adminContact != null)
            //{
            //    return new PortalActionResult
            //    {
            //        IsSuccess = false,
            //        Message = string.Format("Account :{0} already have a portal admin {1} ignore this update",
            //            accountId, adminContact.ContactID)
            //    };
            //}


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
                return $"[PartnerPortal: {partnerPortalId}] Could not find partner portal admin contact:{contactId}";
            }

            var user = await _userManager.FindByEmailAsync(contact.Email);

            if (user?.CompanyId != null && user?.CompanyId.Value > 0)
            {
                var company = await _userManager.GetCompanyAsync(user.CompanyId.Value);
                if (company.CompanyZohoAccountId != accountId)
                {
                    return
                        $"[PartnerPortal: {partnerPortalId}] Portal Admin already exist in ACL but belong to other Zoho Account";
                }

            }

            if (user != null)
            {
                var userZohoContact = await _userManager.GetUserZohoContactAsync(user.Id);
                if (userZohoContact?.ZohoContactId != contactId)
                {

                    if (userZohoContact != null)
                        return
                            $"[PartnerPortal: {partnerPortalId}] Portal Admin Email already registered in ACL and belong to another contact {userZohoContact.ZohoContactId} ";
                }
            }

            return string.Empty;
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

        private PortalActionResult UpdateZohoIfNeeded(ZohoContact contact, Models.ZohoPartnerPortal partnerPortal, string oldPortalAdminContactId)
        {
            PortalActionResult result = new PortalActionResult();

            StringBuilder message = new StringBuilder();

            result.IsSuccess = false;

            var needUpdateContact = true;
            var needUpdatePartnerPortal = true;
            var needUpdateOldContact = false;

            ZohoContact zohoOldAdminContact = null;

            if (!string.IsNullOrEmpty(oldPortalAdminContactId))
            {
                zohoOldAdminContact =
                    ZohoRepository.Contacts.FirstOrDefault(x => x.ContactID == oldPortalAdminContactId);
                if (zohoOldAdminContact.PortalAdmin)
                {
                    needUpdateOldContact = true;
                }
            }

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

            if (needUpdateContact || needUpdateOldContact)
            {
                var zohoContact = new ZohoCRMProxy.ZohoContact
                {
                    Id = contact.ContactID,
                    PortalAdmin = true,
                    PartnerPortalEnabled = false,
                    SAPUnqualified = contact.SAPUnqualified.Equals("true", StringComparison.CurrentCultureIgnoreCase),
                    EmailOptOut = contact.EmailOptOut.Equals("true", StringComparison.CurrentCultureIgnoreCase),
                };

                using (ZohoCRMProxy.ZohoCRMContact request = new ZohoCRMContact(_zohoToken))
                {
                    try
                    {
                        if (needUpdateContact)
                        {
                            var contactResult = request.Update(zohoContact);

                            result.IsSuccess = true;
                            message.AppendLine("Zoho Contact Portal Admin Updated.");
                        }

                        if (needUpdateOldContact)
                        {
                            var oldAdminContactResult = request.Update(new ZohoCRMProxy.ZohoContact
                            {
                                Id = zohoOldAdminContact.ContactID,
                                PortalAdmin = false,
                                PartnerPortalEnabled = true,
                                SAPUnqualified =
                                    zohoOldAdminContact.SAPUnqualified.Equals("true",
                                        StringComparison.CurrentCultureIgnoreCase),
                                EmailOptOut =
                                    zohoOldAdminContact.EmailOptOut.Equals("true",
                                        StringComparison.CurrentCultureIgnoreCase),

                            });

                            result.IsSuccess = true;
                            message.AppendLine("Zoho Old Portal Admin Contact Updated.");
                        }


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

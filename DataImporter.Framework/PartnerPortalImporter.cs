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

namespace DataImporter.Framework
{
    public class PartnerPortalImporter : ZohoImportBase
    {

        private SMSUserManager<ApplicationUser> _userManager;
        private string _tempPassword;

        public PartnerPortalImporter(IZohoCRMDataRepository zohoRepository, IEmailSender emailSender) : base(zohoRepository, emailSender)
        {
            TableName = "zcrm_PartnerPortal";

            PortalAction = "Add User and Company to ACL";

            _tempPassword = "A1b2@C3d4";
            
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
                    Result = string.Format("Could not find partner portal record for id:{0}", id)
                };
            }

            var accountId = partnerPortal.PartnerAccount;

            var contactId = partnerPortal.PortalAdmin;



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
                    Result = string.Format("Could not find partner portal admin contact:{0}", contactId)
                };
            }

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
            
            return new PortalActionResult
            {
                IsSuccess = isSuccess,
                Result = message
            };
            
            
        }
    }
}

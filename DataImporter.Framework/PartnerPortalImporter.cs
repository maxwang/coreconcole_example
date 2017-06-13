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
                    Resutl = string.Format("Could not find partner portal record for id:{0}", id)
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
                    Resutl = string.Format("Could not find partner portal admin contact:{0}", contactId)
                };
            }

            //check contact created in company
            var company = await _userManager.GetCompanyByZohoAccountIdAsync(accountId);
            if(company == null)
            {
                var companyId = await _userManager.CreateCompanyAsync(new Company
                {
                    CompanyZohoAccountId = accountId,
                    CreatedBy = "importer",
                    CreatedTime = DateTime.Now
                });
            }

            //check user information

            var user = await _userManager.FindByEmailAsync(contact.Email);
            var message = string.Empty;
            var isSuccess = false;
            if (user == null)
            {
                //create user
                var now = DateTime.Now;
                var tempuser = new ApplicationUser() { UserName = contact.Email, Email = contact.Email, LastPasswordChangedDate = now, CreationDate = now };
                var result = await _userManager.CreateAsync(tempuser, _tempPassword);

                if (result.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(tempuser, DefaultRoleName);
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
                if(await _userManager.IsInRoleAsync(user, DefaultRoleName) == false)
                {
                   var roleAdded =  await _userManager.AddToRoleAsync(user, DefaultRoleName);
                    if(roleAdded.Succeeded)
                    {
                        isSuccess = true;
                        message = string.Format("User role added for user :{0}", user.UserName);
                    }
                }
            }
            

            return new PortalActionResult
            {
                IsSuccess = isSuccess,
                Resutl = message
            };
            
            
        }
    }
}

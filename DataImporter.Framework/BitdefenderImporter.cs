using System;
using System.Collections.Generic;
using System.Text;
using DataImporter.Framework.Repository;
using DataImporter.Framework.Services;
using Website.Extensions;
using DataImporter.Framework.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using DataImporter.Framework.Extensions;
using DataImporter.Framework.Data;
using Microsoft.EntityFrameworkCore;

namespace DataImporter.Framework
{
    public class BitdefenderImporter : ZohoImportBase
    {
        private string _bdClarimType;
        private string _bdClarimValue;
        private SMSUserManager<ApplicationUser> _userManager;

        public BitdefenderImporter(IZohoCRMDataRepository zohoRepository, IEmailSender emailSender) : base(zohoRepository, emailSender)
        {
            TableName = "zcrm_Bitdefender";

            PortalAction = "Assign Bitdefender to Company";

            _bdClarimType = "CustomerPortal.Module";
            _bdClarimValue = "BitDefender";

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

        protected async override Task<PortalActionResult> ProcessImport(string id)
        {
            var bitdefender = ZohoRepository.Bitdefenders.SingleOrDefault(x => x.BitdefenderID.Equals(id, StringComparison.CurrentCultureIgnoreCase));

            if (bitdefender == null)
            {
                return new PortalActionResult
                {
                    IsSuccess = false,
                    Result = string.Format("Could not find Bitdefender record for id:{0}", id)
                };
            }

            var accountId = bitdefender.BDPartner;

            var company = await _userManager.GetCompanyByZohoAccountIdAsync(accountId);

            if(company == null)
            {
                return new PortalActionResult
                {
                    IsSuccess = false,
                    Result = string.Format("Could not find Account/Company information for Zoho Account id:{0}", accountId)
                };
            }

            var companyHasClaim = await _userManager.ComanyHasClaimAsync(company.Id, _bdClarimType, _bdClarimValue);
            if(companyHasClaim)
            {
                return new PortalActionResult
                {
                    IsSuccess = true,
                    Result = string.Format("Account {0} already has Bitdefender module permission", accountId)
                };

            }


            var result = _userManager.CreateCompanyClaimAsync(new CompanyClaims
            {
                CompanyId = company.Id,
                ClaimType = _bdClarimType,
                ClaimValue = _bdClarimValue
            });


            return new PortalActionResult
            {
                IsSuccess = true,
                Result = string.Format("Bitdefender Permission added for Account:{0}", accountId)
            };



        }
    }
}

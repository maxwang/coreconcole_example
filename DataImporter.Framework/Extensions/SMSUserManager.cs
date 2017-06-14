using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DataImporter.Framework.Models;
using DataImporter.Framework.Extensions;

namespace Website.Extensions
{
    public class SMSUserManager<TUser> : UserManager<ApplicationUser>
    {
        public SMSUserManager(IUserStore<ApplicationUser> store, 
            IOptions<IdentityOptions> optionsAccessor, 
            IPasswordHasher<ApplicationUser> passwordHasher, 
            IEnumerable<IUserValidator<ApplicationUser>> userValidators, 
            IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators, 
            ILookupNormalizer keyNormalizer, 
            IdentityErrorDescriber errors, 
            IServiceProvider services, 
            ILogger<UserManager<ApplicationUser>> logger) : 
            base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            Options.Password.RequiredLength = 6;
            Options.Password.RequireNonAlphanumeric = false;
            Options.Password.RequireUppercase = false;
            Options.Password.RequireDigit = false;
        }

        public override async Task<IList<string>> GetRolesAsync(ApplicationUser user)
        {
            if(user == null || string.IsNullOrEmpty(user.Id))
            {
                throw new ArgumentException("User could not be empty");
            }

            var uStore = Store as SMSUserStore<ApplicationUser>;
            return await uStore.GetUserRolesAsync(user.Id);
        }

        //do not use role manager here, use
        public override async Task<bool> IsInRoleAsync(ApplicationUser user, string role)
        {
            var uStore = Store as SMSUserStore<ApplicationUser>;
            return await uStore.IsInRoleAsync(user.Id, role);
        }

        public override Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role)
        {
            var uStore = Store as SMSUserStore<ApplicationUser>;
            return uStore.AddToRoleAsync(user, role);
        }

        public override async Task<IdentityResult> AddPasswordAsync(ApplicationUser user, string password)
        {
            var result = await base.AddPasswordAsync(user, password);
            
            if (result.Succeeded)
            {
                var uStore = Store as SMSUserStore<ApplicationUser>;
                user.LastPasswordChangedDate = DateTime.Now;

                uStore.Context.SaveChanges();
            }

            return result;

        }
        public override async Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword)
        {  
            var result =  await base.ChangePasswordAsync(user, currentPassword, newPassword);

            if (result.Succeeded)
            {
                var uStore = Store as SMSUserStore<ApplicationUser>;
                user.LastPasswordChangedDate = DateTime.Now;
                
                uStore.Context.SaveChanges();
            }

            return result;
        }

        public Company GetCompany(int companyId)
        {
            if (companyId < 0)
            {
                throw new ArgumentException("Company Id is wrong");
            }

            var uStore = Store as SMSUserStore<ApplicationUser>;
            return uStore.GetCompany(companyId);
        }

        public async Task<Company> GetCompanyByZohoAccountIdAsync(string zohoAccountId)
        {
            var uStore = Store as SMSUserStore<ApplicationUser>;
            return await uStore.GetCompanyByZohoAccountIdAsync(zohoAccountId);

        }

        public async Task<int> CreateCompanyAsync(Company company)
        {
            var uStore = Store as SMSUserStore<ApplicationUser>;
            return await uStore.CreateCompanyAsync(company);
        }

        public async Task<Company> GetCompanyAsync(int companyId)
        {
            if (companyId < 0)
            {
                throw new ArgumentException("Company Id is wrong");
            }

            var uStore = Store as SMSUserStore<ApplicationUser>;
            return await uStore.GetCompanyAsync(companyId);
        }

        public async Task<int> CreateCompanyClaimAsync(CompanyClaims claim)
        {
            var uStore = Store as SMSUserStore<ApplicationUser>;
            return await uStore.CreateCompanyClaimAsync(claim);
        }

        public async Task<bool> ComanyHasClaimAsync(int companyId, string claimType, string claimValue)
        {
            var uStore = Store as SMSUserStore<ApplicationUser>;
            return await uStore.ComanyHasClaimAsync(companyId, claimType, claimValue);
        }

        public async Task<UserZohoContact> GetUserZohoContactAsync(string userId)
        {
            var uStore = Store as SMSUserStore<ApplicationUser>;
            return await uStore.GetUserZohoContactAsync(userId);
        }

        public async Task<int> CreateUserZohoContactAsync(UserZohoContact userContact)
        {
            var uStore = Store as SMSUserStore<ApplicationUser>;
            return await uStore.CreateUserZohoContactAsync(userContact);
        }


    }
}

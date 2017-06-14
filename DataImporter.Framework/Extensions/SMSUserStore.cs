using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DataImporter.Framework.Models;
using DataImporter.Framework.Data;

namespace DataImporter.Framework.Extensions
{
    public class SMSUserStore<TUser> : UserStore<TUser>
        where TUser : ApplicationUser, new()
    {
        public SMSUserStore(ACLDbContext context, IdentityErrorDescriber describer = null) : base(context, describer)
        {
            
        }
        

        public async Task<IList<string>> GetUserRolesAsync(string userId)
        {
            var aContext = Context as ACLDbContext;
            var results = from ur in aContext.UserRoles
                          join r in aContext.Roles on ur.RoleId equals r.Id
                          where ur.UserId == userId
                          select r.Name;

            return await results.ToListAsync(); 
        }

        public Company GetCompany(int companyId)
        {
            var aContext = Context as ACLDbContext;
            return aContext.Companies.FirstOrDefault(x => x.Id == companyId);
        }


        public async Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string roleName)
        {
            var aContext = Context as ACLDbContext;
            var role = await aContext.Roles.SingleOrDefaultAsync(x => x.Name.Equals(roleName, StringComparison.CurrentCultureIgnoreCase));
            if(role == null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "RoleNotExist",
                    Description = "Could not find role name"
                });
            }

            var result = await aContext.UserRoles.AddAsync(new IdentityUserRole<string> { RoleId = role.Id, UserId = user.Id });
            await aContext.SaveChangesAsync();
            return IdentityResult.Success;

        }

        public async Task<bool> IsInRoleAsync(string userId, string roleName)
        {
            var aContext = Context as ACLDbContext;
            var role = await aContext.Roles.SingleOrDefaultAsync(x => x.Name.Equals(roleName, StringComparison.CurrentCultureIgnoreCase));
            if(role == null)
            {
                return false;
            }

            var userRole = await aContext.UserRoles.SingleOrDefaultAsync(x => x.UserId.Equals(userId) && x.RoleId == role.Id);
            return userRole != null;
        }

        public async Task<Company> GetCompanyAsync(int companyId)
        {
            var aContext = Context as ACLDbContext;
            return await aContext.Companies.FirstOrDefaultAsync(x => x.Id == companyId);
        }

        public async Task<Company> GetCompanyByZohoAccountIdAsync(string zohoAccountId)
        {
            var aContext = Context as ACLDbContext;
            return await aContext.Companies.FirstOrDefaultAsync(x => x.CompanyZohoAccountId == zohoAccountId);
        }

        public async Task<int> CreateCompanyAsync(Company company)
        {
            var aContext = Context as ACLDbContext;
            await aContext.Companies.AddAsync(company);
            await aContext.SaveChangesAsync();
            return company.Id;
        }

        public async Task<int> CreateCompanyClaimAsync(CompanyClaims claim)
        {
            var aContext = Context as ACLDbContext;
            await aContext.CompanyClaims.AddAsync(claim);
            await aContext.SaveChangesAsync();
            return claim.Id;
        }

        public async Task<UserZohoContact> GetUserZohoContactAsync(string userId)
        {
            var aContext = Context as ACLDbContext;
            return await aContext.UserZohoContacts.SingleOrDefaultAsync(x => x.UserId.Equals(userId));
        }

        public async Task<int> CreateUserZohoContactAsync(UserZohoContact userContact)
        {
            var aContext = Context as ACLDbContext;
            await aContext.UserZohoContacts.AddAsync(userContact);
            await aContext.SaveChangesAsync();
            return userContact.Id;
        }


        public async Task<bool> ComanyHasClaimAsync(int companyId, string claimType, string claimValue)
        {
            var aContext = Context as ACLDbContext;
            return await aContext.CompanyClaims.AnyAsync(x => x.CompanyId == companyId
                   && x.ClaimType.Equals(claimType, StringComparison.CurrentCultureIgnoreCase)
                   && x.ClaimValue.Equals(claimValue));
        }
    }
}

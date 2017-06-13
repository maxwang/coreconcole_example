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


    }
}

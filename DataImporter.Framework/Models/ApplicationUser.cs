using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataImporter.Framework.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {

        public int? CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public Company MyCompany { get; set; }


        public DateTime LastPasswordChangedDate { get; set; }

        public DateTime CreationDate { get; set; }
        
        
    }
}

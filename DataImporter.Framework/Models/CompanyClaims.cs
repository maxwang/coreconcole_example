using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DataImporter.Framework.Models
{
    [Table("AspNetCompanyClaims")]
    public class CompanyClaims
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        
        public int CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public Company Company { get; set; }


        /// <summary>
        /// Gets or sets the claim type for this claim.
        /// </summary>
        public string ClaimType { get; set; }

        /// <summary>
        /// Gets or sets the claim value for this claim.
        /// </summary>
        public string ClaimValue { get; set; }
    }
}

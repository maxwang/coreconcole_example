using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DataImporter.Framework.Models
{
    [Table("AspNetCompanies")]
    public class Company
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [MaxLength(255)]
        public string CompanyZohoAccountId { get; set; }

        [DefaultValue("GETDATE()")]
        [Required]
        //[Column(“BlogDescription", TypeName="ntext")]
        //[Column("CreatedTime")]
        public DateTime CreatedTime { get; set; }

        public string CreatedBy { get; set; }
        
    }
}

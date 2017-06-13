using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DataImporter.Framework.Models
{
    [Table("AspNetRoleType")]
    public class ApplicationRoleType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public List<ApplicationRole> Roles { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DataImporter.Framework.Models
{
    [Table("AspNetUserZohoContact")]
    public class UserZohoContact
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string UserId { get; set; }
        public string ZohoContactId { get; set; }
    }
}

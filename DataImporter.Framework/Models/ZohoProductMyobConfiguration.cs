using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DataImporter.Framework.Models
{
    [Table("zcrm_ProductMyobConfiguration")]
    public class ZohoProductMyobConfiguration
    {
        [Key]
        public int Id { get; set; }
        public string ProductId { get; set; }
        public string TaxCode { get; set; }
        public string MyobUuid { get; set; }
        public string MyobLinkedGl { get; set; }
        public string MyobNamedId { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime ModifiedTime { get; set; }
    }
}

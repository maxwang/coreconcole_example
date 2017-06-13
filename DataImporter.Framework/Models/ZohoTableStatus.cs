using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DataImporter.Framework.Models
{
    [Table("zcrm_TableStatus")]
    public class ZohoTableStatus
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TableStatusId { get; set; }

        public string TableName { get; set; }
        public string RecordID { get; set; }
        public string LastAction { get; set; }
        public DateTime LastActionTime { get; set; }
        public string PortalAction { get; set; }
        public DateTime? PortalActionTime { get; set; }
        public string PortalActionResult { get; set; }
        
    }
}

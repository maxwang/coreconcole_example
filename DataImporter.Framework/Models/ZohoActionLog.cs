using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DataImporter.Framework.Models
{
    [Table("zcrm_ActionLogs")]
    public class ZohoActionLog
    {
        [Key]
        public int ActionLogId { get; set; }

        public string TableName { get; set; }
        public string Action { get; set; }
        public string ActionData { get; set; }
        public string ActionResult { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }

        [Column("stageindicator")]
        public int Stageindicator { get; set; }
    }
}

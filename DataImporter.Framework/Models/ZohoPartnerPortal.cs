using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DataImporter.Framework.Models
{

    [Table("zcrm_PartnerPortal")]
    public class ZohoPartnerPortal
    {
        [Key]
        public string PartnerPortalID { get; set; }
        public string PartnerPortal_ID { get; set; }
        public string PartnerPortalOwnerID { get; set; }
        public string Email { get; set; }
        public string SecondaryEmail { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime ModifiedTime { get; set; }
        public DateTime LastActivityTime { get; set; }
        public string Currency { get; set; }
        public double ExchangeRate { get; set; }
        public string EmailOptOut { get; set; }
        public string Layout { get; set; }
        public string PortalAreas { get; set; }
        public string PartnerAccount { get; set; }
        public string PortalAdmin { get; set; }
        public string ACLCreated { get; set; }
    }
}

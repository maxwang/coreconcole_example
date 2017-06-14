using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DataImporter.Framework.Models
{
    [Table("zcrm_Bitdefender")]
    public class ZohoBitdefender
    {
        [Key]
        public string BitdefenderID { get; set; }

        public string BM_Mod_RecID { get; set; }
        public string BitdefenderOwnerID { get; set; }
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
        public string GZAPIUUID { get; set; }
        public string BDPartnerStatus { get; set; }
        public string BDMSPEnabled { get; set; }
        public string BDPartner { get; set; }
        public double? BDLicSpecialDisc { get; set; }
        public string PANUUID { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DataImporter.Framework.Models
{
    [Table("zcrm_Accounts")]
    public class ZohoAccount
    {
        [Key]
        [Column("AccountID")]
        public string AccountId { get; set; }

        [Column("ABNCompanyNum")]
        public string AbnCompanyNum { get; set; }

        public string AccountLinkedin { get; set; }

        //[Column("AccountName")]
        public string AccountName { get; set; }

        public int AccountNumber { get; set; }

        [Column("AccountOwnerID")]
        public string AccountOwnerId { get; set; }
        
        public string AccountSite { get; set; }
        public string AccountType { get; set; }
        public double? AnnualRevenue { get; set; }
        public string BillingCity { get; set; }
        public string BillingCode { get; set; }
        public string BillingCountry { get; set; }
        public string BillingState { get; set; }

        public string BillingStreet { get; set; }
        public string Checkbox1 { get; set; }
        public string CreatedBy { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm:ss}")]
        public DateTime CreatedTime { get; set; }

        [NotMapped]
        public string CreaterName { get; set; }

        public string Currency { get; set; }
        public string CustomerSizeServiced { get; set; }
        public string Description { get; set; }
        public int Employees { get; set; }
        public double? ExchangeRate { get; set; }
        public string Facebook { get; set; }
        public string Fax { get; set; }
        
        public string GooglePlus { get; set; }
        public string IndustriesServiced { get; set; }
        public string Industry { get; set; }
        public string Instagram { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm:ss}")]
        public DateTime LastActivityTime { get; set; }
        

        public string Layout { get; set; }

        public string ModifiedBy { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm:ss}")]
        public DateTime ModifiedTime { get; set; }
        
        public int NegativeScore { get; set; }
        public int NegativeTouchPointScore { get; set; }
        public string NoofEmployees { get; set; }
        public string NoofEndpointsEPP { get; set; }
        public string Ownership { get; set; }
        public string ParentAccount { get; set; }

        public string Phone { get; set; }
        public int PositiveScore { get; set; }
        public int PositiveTouchPointScore { get; set; }

        public string Rating { get; set; }
        public int Score { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingCode { get; set; }
        public string ShippingCountry { get; set; }
        public string ShippingState { get; set; }
        public string ShippingStreet { get; set; }
        public int? SICCode { get; set; }
        public string Territories { get; set; }
        public string TickerSymbol { get; set; }
        public int TouchPointScore { get; set; }
        public string VendorStatus { get; set; }
        public string Website { get; set; }
    }
}

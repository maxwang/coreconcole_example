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
        public string AccountID { get; set; }
        public string AccountOwnerID { get; set; }
        public string Rating { get; set; }
        public string AccountName { get; set; }
        public string Phone { get; set; }
        public string AccountSite { get; set; }
        public string Fax { get; set; }
        public string ParentAccount { get; set; }
        public string Website { get; set; }
        public string AccountNumber { get; set; }
        public string TickerSymbol { get; set; }
        public string AccountType { get; set; }
        public string Ownership { get; set; }
        public string Industry { get; set; }
        public string Employees { get; set; }
        public string AnnualRevenue { get; set; }
        public string SICCode { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string CreatedTime { get; set; }
        public string ModifiedTime { get; set; }
        public string BillingStreet { get; set; }
        public string ShippingStreet { get; set; }
        public string BillingCity { get; set; }
        public string ShippingCity { get; set; }
        public string BillingState { get; set; }
        public string ShippingState { get; set; }
        public string BillingCode { get; set; }
        public string ShippingCode { get; set; }
        public string BillingCountry { get; set; }
        public string ShippingCountry { get; set; }
        public string Description { get; set; }
        public string Currency { get; set; }
        public string ExchangeRate { get; set; }
        public string LastActivityTime { get; set; }
        public string Layout { get; set; }
        public string Territories { get; set; }
        public string NoofEmployees { get; set; }
        public string NoofEndpointsEPP { get; set; }
        public string Checkbox1 { get; set; }
        public string VendorStatus { get; set; }
        public string ABNCompanyNum { get; set; }
        public string AccountLinkedin { get; set; }
        public string Score { get; set; }
        public string PositiveScore { get; set; }
        public string NegativeScore { get; set; }
        public string Facebook { get; set; }
        public string GooglePlus { get; set; }
        public string Instagram { get; set; }
        public string PositiveTouchPointScore { get; set; }
        public string TouchPointScore { get; set; }
        public string NegativeTouchPointScore { get; set; }
        public string CustomerSizeServiced { get; set; }
        public string IndustriesServiced { get; set; }
    }
}

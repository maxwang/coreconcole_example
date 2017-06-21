using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DataImporter.Framework.Models
{

    [Table("zcrm_Contacts")]
    public class ZohoContact
    {
        [Key]
        public string ContactID { get; set; }

        public string ContactOwnerID { get; set; }
        public string LeadSource { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string AccountID { get; set; }
        public string VendorID { get; set; }
        public string Email { get; set; }
        public string Title { get; set; }
        public string Department { get; set; }
        public string Phone { get; set; }
        public string HomePhone { get; set; }
        public string OtherPhone { get; set; }
        public string Fax { get; set; }
        public string Mobile { get; set; }
        public DateTime? DateofBirth { get; set; }
        public string Assistant { get; set; }
        public string AsstPhone { get; set; }
        public string ReportsTo { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime ModifiedTime { get; set; }
        public string FullName { get; set; }
        public string MailingStreet { get; set; }
        public string OtherStreet { get; set; }
        public string MailingCity { get; set; }
        public string OtherCity { get; set; }
        public string MailingState { get; set; }
        public string OtherState { get; set; }

        //when import to csv, mailing zip changed to int
        //will use this as default, will update this if needed.
        public int? MailingZip { get; set; }

        public string OtherZip { get; set; }
        public string MailingCountry { get; set; }
        public string OtherCountry { get; set; }
        public string Description { get; set; }
        public string EmailOptOut { get; set; }
        public string SkypeID { get; set; }
        public string CampaignSource { get; set; }
        public string Salutation { get; set; }
        public string SecondaryEmail { get; set; }
        public string Currency { get; set; }
        public double ExchangeRate { get; set; }
        public DateTime LastActivityTime { get; set; }
        public string Twitter { get; set; }
        public string Layout { get; set; }
        public string Territories { get; set; }
        public string PositionClassification { get; set; }
        public string Checkbox1 { get; set; }
        public string LinkedinAccount { get; set; }
        public string SAPUnqualified { get; set; }
        public string SportInterest { get; set; }
        public string DaysVisited { get; set; }
        public double? AverageTimeSpentInMinutes { get; set; }
        public string NumberOfChats { get; set; }
        public DateTime? MostRecentVisit { get; set; }
        public DateTime? FirstVisit { get; set; }
        public string FirstPageVisited { get; set; }
        public string Referrer { get; set; }
        public string VisitorScore { get; set; }
        public string DepartedEmployee { get; set; }
        public int Score { get; set; }
        public int PositiveScore { get; set; }
        public int NegativeScore { get; set; }
        public string PartnerPortalEnabled { get; set; }
        public bool PortalAdmin { get; set; }

        public int PositiveTouchPointScore { get; set; }
        public int TouchPointScore { get; set; }
        public int NegativeTouchPointScore { get; set; }
        public string LeadScore { get; set; }

    }
}

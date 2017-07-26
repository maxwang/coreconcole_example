using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DataImporter.Framework.Models
{
    [Table("zcrm_Products")]
    public class ZohoProduct
    {
        [Key]
        public string ProductID { get; set; }

        //[Column("AccountName")]
        public string ProductOwnerID { get; set; }

        public string ProductName { get; set; }
        public string ProductCode { get; set; }

        public string VendorID { get; set; }
        public string ProductActive { get; set; }
        public string Manufacturer { get; set; }
        public string ProductCategory { get; set; }
        public DateTime? SalesStartDate { get; set; }
        public DateTime?  SalesEndDate { get; set; }
        public DateTime? SupportStartDate { get; set; }
        public DateTime? SupportEndDate { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? ModifiedTime { get; set; }
        public double? UnitPrice { get; set; }
        public double? CommissionRate { get; set; }
        public string Tax { get; set; }
        public string UsageUnit { get; set; }
        public double? QtyOrdered { get; set; }
        public double? QuantityinStock { get; set; }
        public double? ReorderLevel { get; set; }
        public string Handler { get; set; }
        public double? QuantityinDemand { get; set; }
        public string Description { get; set; }
        public string Taxable { get; set; }
        public string Currency { get; set; }
        public double? ExchangeRate { get; set; }
        public string Layout { get; set; }
        public int? APIDays { get; set; }
        public int? APICode { get; set; }
        public int? QtyBandMin { get; set; }
        public int? QtyBandMax { get; set; }
        public string PANOrderType { get; set; }
        public string RenewalSKU { get; set; }

        public string MyobUuid { get; set; }

        public string MyobLinkedGl { get; set; }

        public string MyobNameId { get; set; }
    }
}

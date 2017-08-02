using System;
using System.Collections.Generic;
using System.Text;
using MyobCoreProxy;

namespace DataImporter.Framework.Services
{
    public class ContactCustomerImportOptions
    {
        public SellingDetailsOptions SellingDetailsOptions { get; set; }
    }

    public class SellingDetailsOptions
    {
        public string SaleLayout { get; set; }
        public string PrintedForm { get; set; }
        public string InvoiceDelivery { get; set; }
        public string FreightTaxCode { get; set; }
        public string TaxCode { get; set; }
        public string TermsPaymentIsDue { get; set; }
    }

    public class ProductImport
    {
        public string SellingTaxUid { get; set; }
        public string MyobCompanyFileKey { get; set; }
    }
    public class MyobImportOptions
    {
        public string SalesEmail { get; set; }
        public MyobOptions MyobOptions { get; set; }
        public Dictionary<string, ProductImport> ProductImport { get; set; }
        public Dictionary<string, ContactCustomerImportOptions> ContactCustomerImportOptions { get; set; }
    }
}

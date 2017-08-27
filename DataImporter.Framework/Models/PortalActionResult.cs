using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using MyobProxy.Models;

namespace DataImporter.Framework.Models
{
    
    public class PortalActionResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }

    public class MyobInventoryItemActionResult : PortalActionResult
    {
        public InventoryItem Item { get; set; }
    }

    public class MyobContactCustomerActionResult : PortalActionResult
    {
        public Customer Customer { get; set; }
    }
}

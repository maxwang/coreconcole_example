using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DataImporter.Framework.Models
{
    
    public class PortalActionResult
    {
        public bool IsSuccess { get; set; }
        public string Resutl { get; set; }
    }
}

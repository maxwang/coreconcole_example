using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DataImporter.Framework.Models
{
    public class ApplicationRole : IdentityRole
    {
        public int RoleTypeId { get; set; }

        [ForeignKey("RoleTypeId")]
        public ApplicationRoleType RoleType { get; set; }

        public bool IsInternal { get; set; }
    }
}

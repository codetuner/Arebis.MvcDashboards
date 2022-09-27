using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyMvcApp.Areas.MvcDashboardIdentity.Models.Users
{
    public class IndexModel : BaseIndexModel<IdentityUser>
    {
        public List<SelectListItem> RoleNames { get; internal set; } = null!;
        
        public string? SelectedRoleName { get; set; }
    }
}

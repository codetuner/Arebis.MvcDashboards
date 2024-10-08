﻿using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable enable

namespace MyMvcApp.Areas.MvcDashboardIdentity.Models.Users
{
    public class EditModel : BaseEditModel<IdentityUser>
    {
        public bool IsNew => this.Item.Id == "NEW";

        public bool CanHavePassword { get; set; }

        public bool CanHaveInitialPassword { get; set; }

        public string? InitialPassword { get; set; }

        public bool RemovePasswordOnSave { get; set; }

        public List<IdentityUserClaim<string>> UserClaims { get; set; } = new List<IdentityUserClaim<string>>();

        public IdentityUserClaim<string>? NewClaim { get; set; }

        public List<string> UserRoleNames { get; set; } = new List<string>();

        public bool SupportsUserClaims { get; internal set; }

        public bool SupportsUserRoles { get; internal set; }

        public List<IdentityRole>? Roles { get; internal set; }

        public string[]? ClaimTypes { get; internal set; }
    }
}

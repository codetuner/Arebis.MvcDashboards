﻿using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyMvcApp.Areas.MvcDashboardIdentity.Models.Roles
{
    public class EditModel : BaseEditModel<IdentityRole>
    {
        public bool IsNew => this.Item.Id == "NEW";
    }
}

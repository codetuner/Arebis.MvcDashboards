using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyMvcApp.Areas.MvcDashboardIdentity.Models.Roles
{
    public class DeleteRequestModel
    {
        public DeleteRequestModel()
        {
            this.Item = new IdentityRole();
        }

        public DeleteRequestModel(IdentityRole item)
        {
            Item = item;
        }

        public IdentityRole Item { get; set; }
    }
}

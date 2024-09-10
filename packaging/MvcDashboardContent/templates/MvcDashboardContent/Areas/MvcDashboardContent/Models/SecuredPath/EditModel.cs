using MyMvcApp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyMvcApp.Areas.MvcDashboardContent.Models.SecuredPath
{
    public class EditModel : BaseEditModel<Data.Content.SecuredPath>
    {
        public bool IsNew => this.Item.Id == 0;

        public List<string> PathsList { get; internal set; } = [];
    }
}

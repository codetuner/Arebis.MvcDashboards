using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyMvcApp.Areas.MvcDashboardLocalize.Models.Query
{
    public class EditModel : BaseEditModel<Data.Localize.Query>
    {
        public bool IsNew => this.Item.Id == 0;

        public Data.Localize.Domain[]? Domains { get; internal set; } = null!;

        public string[]? ConnectionNames { get; internal set; } = null!;
    }
}

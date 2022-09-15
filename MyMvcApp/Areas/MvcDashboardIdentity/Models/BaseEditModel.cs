using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyMvcApp.Areas.MvcDashboardIdentity.Models
{
    public class BaseEditModel<TItem>
        where TItem : class
    {
        public TItem Item { get; set; } = null!;

        public bool HasChanges { get; set; }
    }
}

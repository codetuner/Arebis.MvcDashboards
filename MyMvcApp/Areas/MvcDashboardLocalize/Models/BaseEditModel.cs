using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MyMvcApp.Areas.MvcDashboardLocalize.Models
{
    public class BaseEditModel<TItem>
    {
        public TItem Item { get; set; } = default!;

        public bool HasChanges { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyMvcApp.Areas.MvcDashboardIdentity.Models
{
    public class BaseEditModel<TItem>
        where TItem : new()
    {
        public BaseEditModel()
        {
            this.Item = new TItem();
        }

        public BaseEditModel(TItem item)
        {
            Item = item;
        }

        public TItem Item { get; set; }

        public bool HasChanges { get; set; }
    }
}

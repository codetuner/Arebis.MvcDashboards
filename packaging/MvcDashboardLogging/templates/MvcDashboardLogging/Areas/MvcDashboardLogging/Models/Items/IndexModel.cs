using MyMvcApp.Data.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyMvcApp.Areas.MvcDashboardLogging.Models.Items
{
    public class IndexModel : BaseIndexModel<RequestLog>
    { 
        public string? AspectFilter { get; set; }

        public bool Bookmarked { get; set; }
    }
}

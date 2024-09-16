using MyMvcApp.Data.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable enable

namespace MyMvcApp.Areas.MvcDashboardLogging.Models.Items
{
    public class IndexModel : BaseIndexModel<RequestLog>
    {
        public string? ApplicationFilter { get; set; }
        
        public string? AspectFilter { get; set; }

        public bool BookmarkedFilter { get; set; }
    }
}

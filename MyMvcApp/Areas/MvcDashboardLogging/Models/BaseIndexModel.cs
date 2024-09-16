using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable enable

namespace MyMvcApp.Areas.MvcDashboardLogging.Models
{
    public class BaseIndexModel<TItem>
    {
        public TItem[] Items { get; internal set; } = Array.Empty<TItem>();

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int MaxPage { get; set; } = 1;

        public string? Query { get; set; }
        
        public string? Order { get; set; }
    }
}

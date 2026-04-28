using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

#nullable enable

namespace MyMvcApp.Areas.MvcDashboardTasks.Models.TaskDefinition
{
    public class IndexModel : BaseIndexModel<IndexItemModel>
    {
        public string? ProcessRole { get; set; }

        public List<SelectListItem> ProcessRoles { get; internal set; } = null!;
        
        public bool UserIsAdmin { get; internal set; }
    }

    public class IndexItemModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string? ProcessRole { get; set; }

        public bool IsActive { get; set; }

        public DateTime? UtcTimeLastRun { get; set; }

        public bool? IsRunningLastRun { get; set; }

        public TimeSpan? DuractionLastRun { get; set; }

        public bool? SucceededLastRun { get; set; }

        public DateTime? UtcTimeNextRun { get; set; }
    }
}

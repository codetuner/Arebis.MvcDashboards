using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

#nullable enable

namespace MyMvcApp.Areas.MvcDashboardTasks.Models.TaskDefinition
{
    public class IndexModel : BaseIndexModel<Data.Tasks.ScheduledTaskDefinition>
    {
        public string? ProcessRole { get; set; }

        public List<SelectListItem> ProcessRoles { get; internal set; } = null!;
        
        public bool UserIsAdmin { get; internal set; }
    }
}

using Microsoft.AspNetCore.Mvc.Rendering;

namespace MyMvcApp.Areas.MvcDashboardTasks.Models.TaskDefinition
{
    public class IndexModel : BaseIndexModel<Data.Tasks.ScheduledTaskDefinition>
    {
        public string? ProcessRole { get; set; }

        public List<SelectListItem> ProcessRoles { get; internal set; } = null!;
    }
}

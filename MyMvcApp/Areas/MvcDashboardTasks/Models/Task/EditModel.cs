using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyMvcApp.Areas.MvcDashboardTasks.Models.Task
{
    public class EditModel : BaseEditModel<Data.Tasks.Task>
    {
        public Data.Tasks.TaskDefinition[]? Definitions { get; internal set; } = null!;
    }
}

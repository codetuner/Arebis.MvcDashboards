using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable enable

namespace MyMvcApp.Areas.MvcDashboardTasks.Models.Task
{
    public class EditModel : BaseEditModel<Data.Tasks.ScheduledTask>
    {
        public bool IsNew => this.Item.Id == 0;

        public Data.Tasks.ScheduledTaskDefinition[]? Definitions { get; internal set; } = null!;
        
        public bool UserCanWrite { get; internal set; }
    }
}

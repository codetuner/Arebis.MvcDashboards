using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyMvcApp.Areas.MvcDashboardTasks.Models.TaskDefinition
{
    public class EditModel : BaseEditModel<Data.Tasks.TaskDefinition>
    {
        public List<string> ImplementationCandidateNames { get; internal set; } = new();
        
        public List<string> ProcessRoleCandidateNames { get; internal set; } = new();
    }
}

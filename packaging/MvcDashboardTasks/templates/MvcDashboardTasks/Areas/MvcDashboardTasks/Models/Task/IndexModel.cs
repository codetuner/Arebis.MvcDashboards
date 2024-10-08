﻿using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

#nullable enable

namespace MyMvcApp.Areas.MvcDashboardTasks.Models.Task
{
    public class IndexModel : BaseIndexModel<Data.Tasks.ScheduledTask>
    {
        public int? DefinitionId { get; set; }
        
        public string? ProcessRole { get; set; }

        public string? StatusFilter { get; set; }

        public bool SelectionMaster { get; set; }

        public int[]? Selection { get; set; }

        public bool AutoRefresh { get; set; }

        public List<SelectListItem> Definitions { get; internal set; } = null!;

        public List<SelectListItem> ProcessRoles { get; internal set; } = null!;
    }
}

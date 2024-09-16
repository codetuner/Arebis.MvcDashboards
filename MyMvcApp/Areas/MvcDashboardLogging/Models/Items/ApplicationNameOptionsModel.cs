using System.Collections.Generic;

#nullable enable

namespace MyMvcApp.Areas.MvcDashboardLogging.Models.Items
{
    public class ApplicationNameOptionsModel
    {
        public List<string?> Options { get; internal set; } = new();

        public string? SelectedOption { get; internal set; }
    }
}

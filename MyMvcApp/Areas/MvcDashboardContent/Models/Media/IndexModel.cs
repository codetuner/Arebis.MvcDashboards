using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.IO;

namespace MyMvcApp.Areas.MvcDashboardContent.Models.Media
{
    public class IndexModel
    {
        public string? Path { get; set; }

        public string? NewName { get; set; }

        public List<SelectListItem> BreadCrumb { get; internal set; } = [];

        public List<SelectListItem> Directories { get; internal set; } = [];

        public FileInfo[] Files { get; internal set; } = [];
    }
}

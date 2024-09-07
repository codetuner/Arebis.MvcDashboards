using Microsoft.AspNetCore.Mvc.Rendering;
using MyMvcApp.Data.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyMvcApp.Areas.MvcDashboardContent.Models.Document
{
    public class IndexModel : BaseIndexModel<Data.Content.Document>
    {
        public int? DocumentTypeId { get; set; }

        public List<SelectListItem> DocumentTypes { get; internal set; } = [];

        public string State { get; set; } = null!;

        public List<SelectListItem> States { get; internal set; } = [];
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyMvcApp.Areas.MvcDashboardLocalize.Models.Domain
{
    public class EditModel : BaseEditModel<Data.Localize.Domain>
    {
        [Required]
        public string? Cultures { get; set; }
    }
}

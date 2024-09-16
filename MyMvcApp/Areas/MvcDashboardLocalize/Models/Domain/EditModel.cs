using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

#nullable enable

namespace MyMvcApp.Areas.MvcDashboardLocalize.Models.Domain
{
    public class EditModel : BaseEditModel<Data.Localize.Domain>
    {
        public bool IsNew => this.Item.Id == 0;

        [Required]
        public string? Cultures { get; set; }
    }
}

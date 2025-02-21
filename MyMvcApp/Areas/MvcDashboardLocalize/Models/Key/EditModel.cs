using MyMvcApp.Data.Localize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable enable

namespace MyMvcApp.Areas.MvcDashboardLocalize.Models.Key
{
    public class EditModel : BaseEditModel<Data.Localize.Key>
    {
        public bool IsNew => this.Item.Id == 0;

        public string? ArgumentNames { get; set; }

        public List<KeyValue> Values { get; set; } = new();

        public string? SourceCulture { get; set; }

        public bool SaveAsCopy { get; set; }

        public Data.Localize.Domain[] Domains { get; internal set; } = [];

        public bool HasTranslationService { get; internal set; }

        public List<string> WritableCultures { get; internal set; } = new();
    }
}

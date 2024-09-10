using MyMvcApp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMvcApp.Areas.MvcDashboardContent.Models.DocumentType
{
    public class EditModel : BaseEditModel<Data.Content.DocumentType>
    {
        private OwnPropertyTypesSettingsAsStringClass ownPropertyTypesSettingsAsString;

        public EditModel()
        {
            this.ownPropertyTypesSettingsAsString = new OwnPropertyTypesSettingsAsStringClass() { Model = this };
        }

        public bool IsNew => this.Item.Id == 0;

        public Data.Content.DocumentType[] DocumentTypes { get; internal set; } = [];

        public Data.Content.DataType[] DataTypes { get; internal set; } = [];

        public Dictionary<int, Data.Content.DataType> DataTypesDict { get; internal set; } = [];

        public List<int> PropertyTypesToDelete { get; set; } = new();

        public OwnPropertyTypesSettingsAsStringClass OwnPropertyTypesSettingsAsString 
        {
            get
            {
                return this.ownPropertyTypesSettingsAsString;
            }
            //set
            //{
            //    this.ownPropertyTypesSettingsAsString = value;
            //    value.Model = this;
            //}
        }

        public class OwnPropertyTypesSettingsAsStringClass
        {
            public EditModel Model { get; set; } = null!;

            public string this[int index]
            {
                get
                {
                    var sb = new StringBuilder();
                    Model.Item.OwnPropertyTypes ??= new();
                    foreach (var pair in Model.Item.OwnPropertyTypes[index].Settings.OrderBy(p => p.Key))
                    {
                        if (sb.Length > 0) sb.Append(Environment.NewLine);
                        sb.Append(pair.Key);
                        sb.Append('=');
                        sb.Append(pair.Value);
                    }
                    return sb.ToString();
                }
                set
                {
                    Model.Item ??= new Data.Content.DocumentType() { Name = null! };
                    Model.Item.OwnPropertyTypes ??= new();
                    Model.Item.OwnPropertyTypes[index].Settings = new Dictionary<string, string>();
                    if (value != null)
                    {
                        foreach (var pair in value.Split(Environment.NewLine).Where(p => p.Contains('=')))
                        {
                            var parts = pair.Split('=', 2);
                            Model.Item.OwnPropertyTypes[index].Settings[parts[0]] = parts[1];
                        }
                    }
                }
            }
        }
    }
}

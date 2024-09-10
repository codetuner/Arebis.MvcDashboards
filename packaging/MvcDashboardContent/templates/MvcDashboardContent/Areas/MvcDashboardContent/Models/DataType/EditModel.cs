using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace MyMvcApp.Areas.MvcDashboardContent.Models.DataType
{
    public class EditModel : BaseEditModel<Data.Content.DataType>
    {
        public bool IsNew => this.Item.Id == 0;

        public string ItemSettingsAsString
        {
            get
            {
                var sb = new StringBuilder();
                foreach (var pair in this.Item.Settings.OrderBy(p => p.Key))
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
                this.Item ??= new Data.Content.DataType() { Name = null!, Template = null! };
                this.Item.Settings = new Dictionary<string, string>();
                if (value != null)
                {
                    foreach (var pair in value.Split(Environment.NewLine).Where(p => p.Contains('=')))
                    {
                        var parts = pair.Split('=', 2);
                        this.Item.Settings[parts[0]] = parts[1];
                    }

                }
            }
        }
    }
}

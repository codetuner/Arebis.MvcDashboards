namespace MyMvcApp.Areas.MvcDashboardLocalize.Models.Domain
{
    public class PreviewModel : BaseEditModel<Data.Localize.Domain>
    {
        public required string CultureCode { get; set; }

        public bool CodeView { get; set; }
    }
}

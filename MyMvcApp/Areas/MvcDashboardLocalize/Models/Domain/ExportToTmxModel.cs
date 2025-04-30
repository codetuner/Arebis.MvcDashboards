namespace MyMvcApp.Areas.MvcDashboardLocalize.Models.Domain
{
    public class ExportToTmxModel
    {
        public int DomainId { get; set; }

        public bool IncludeNonReviewed { get; set; }
        
        public bool IncludeNotes { get; set; }
    }
}

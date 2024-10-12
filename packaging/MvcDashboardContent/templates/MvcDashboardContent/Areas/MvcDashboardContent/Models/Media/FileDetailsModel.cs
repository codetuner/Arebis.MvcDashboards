using System.IO;

namespace MyMvcApp.Areas.MvcDashboardContent.Models.Media
{
    public class FileDetailsModel
    {
        public string? Path { get; internal set; }

        public string? FileName { get; internal set; }

        public FileInfo? FileInfo { get; internal set; }
    }
}

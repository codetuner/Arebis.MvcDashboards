using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyMvcApp.Areas.MvcDashboardLogging.Models.Home;
using MyMvcApp.Data.Logging;
using MyMvcApp.Logging;

namespace MyMvcApp.Areas.MvcDashboardLogging.Controllers
{
    [Authorize(Roles = "Administrator,LoggingAdministrator")]
    public class HomeController : BaseController
    {
        private static bool migrationsComplete = false;

        #region Construction

        private readonly LoggingDbContext context;

        public HomeController(LoggingDbContext context)
        {
            this.context = context;
        }

        #endregion

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = new IndexModel();

            // Check if there are pending migrations:
            if (!migrationsComplete)
            {
                var migrations = await context.Database.GetPendingMigrationsAsync();
                if (migrations != null && migrations.Any())
                {
                    model.HasPendingMigrations = true;
                }
                else
                {
                    // Skip checking next time:
                    migrationsComplete = true;
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult GetStarted()
        {
            return View();
        }

        [HttpGet]
        public IActionResult RunMigrations()
        {
            context.Database.Migrate();

            return Forward(Url.Action("Index")!);
        }

        public IActionResult Chart(string grain)
        {
            var timeZoneOffset = Int32.Parse(this.Request.Headers["X-Sircl-Timezone-Offset"].FirstOrDefault() ?? "0");

            var now = DateTime.UtcNow;
            var labels = new List<Tuple<int, string>>();
            IQueryable<IGrouping<int, RequestLog>> query;
            if (grain == "hourly")
            {
                var start = now.AddHours(-47);
                for (int i = -47; i <= 0; i++)
                {
                    var then = now.AddHours(i);
                    labels.Add(new Tuple<int, string>(then.Day * 100 + then.Hour, then.AddMinutes(-timeZoneOffset).ToString("HH") + "-" + then.AddMinutes(-timeZoneOffset).AddHours(1).ToString("HH")));
                }
                query = this.context.RequestLogs
                    .Where(l => l.Timestamp >= start)
                    .GroupBy(l => (l.Timestamp.Day * 100 + l.Timestamp.Hour));
            }
            else
            {
                var start = now.AddDays(-29);
                for (int i = -29; i <= 0; i++)
                {
                    var then = now.AddDays(i);
                    labels.Add(new Tuple<int, string>(then.Month * 100 + then.Day, then.AddMinutes(-timeZoneOffset).ToString("MM/dd")));
                }
                query = this.context.RequestLogs
                    .Where(l => l.Timestamp >= start)
                    .GroupBy(l => l.Timestamp.Month * 100 + l.Timestamp.Day);
            }

            var data = query.Select(l => new int[] {
                l.Key,
                l.Count(ll => ll.AspectName == LogAspect.Error.Name),
                l.Count(ll => ll.AspectName == LogAspect.Security.Name),
                l.Count(ll => ll.AspectName == LogAspect.Attention.Name),
                l.Count(ll => ll.AspectName == LogAspect.NotFound.Name),
                l.Count(ll => ll.AspectName == LogAspect.Timing.Name),
                l.Count(ll => ll.AspectName == LogAspect.Information.Name),
            }).ToArray();

            var dataSets = new ChartDataSet[6];
            dataSets[0] = new ChartDataSet(LogAspect.Error, new int[labels.Count]);
            dataSets[1] = new ChartDataSet(LogAspect.Security, new int[labels.Count]);
            dataSets[2] = new ChartDataSet(LogAspect.Attention, new int[labels.Count]);
            dataSets[3] = new ChartDataSet(LogAspect.NotFound, new int[labels.Count]);
            dataSets[4] = new ChartDataSet(LogAspect.Timing, new int[labels.Count]);
            dataSets[5] = new ChartDataSet(LogAspect.Information, new int[labels.Count]);

            for (int i = 0; i < labels.Count; i++)
            {
                dataSets[0].Data[i] = data.SingleOrDefault(d => d[0] == labels[i].Item1)?[1] ?? 0;
                dataSets[1].Data[i] = data.SingleOrDefault(d => d[0] == labels[i].Item1)?[2] ?? 0;
                dataSets[2].Data[i] = data.SingleOrDefault(d => d[0] == labels[i].Item1)?[3] ?? 0;
                dataSets[3].Data[i] = data.SingleOrDefault(d => d[0] == labels[i].Item1)?[4] ?? 0;
                dataSets[4].Data[i] = data.SingleOrDefault(d => d[0] == labels[i].Item1)?[5] ?? 0;
                dataSets[5].Data[i] = data.SingleOrDefault(d => d[0] == labels[i].Item1)?[6] ?? 0;
            }

            return View(new ChartModel(grain, labels.Select(l => l.Item2).ToArray(), dataSets));
        }

        [HttpPost]
        public async Task<IActionResult> Flush(int days)
        {
            // Never delete last 5 days as to avoid deleting recent security traces:
            days = (days < 5) ? 5 : days;

            // Delete:
            int count = 0;
            var connection = context.Database.GetDbConnection();
            connection.Open();
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = $"DELETE FROM [logging].[RequestLog] WHERE [IsBookmarked] = 0 AND [Timestamp] < '{(DateTime.UtcNow.Date.AddDays(-days).ToString("O"))}'";
                count = await cmd.ExecuteNonQueryAsync();
            }
            connection.Close();

            // Set Toaster:
            if (count > 0)
            {
                SetToastrMessage("success", $"Successfully flushed {count:#,##0} request log records.");
            }
            else
            {
                SetToastrMessage("info", $"No request log records flushed.");
            }

            // Return view:
            return Forward(Url.Action("Index")!);
        }
    }
}
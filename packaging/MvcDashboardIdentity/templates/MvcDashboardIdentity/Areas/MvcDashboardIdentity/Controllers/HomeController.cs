using Microsoft.AspNetCore.Mvc;
using MyMvcApp.Areas.MvcDashboardIdentity.Models.Home;
using MyMvcApp.Data;
using System.Linq;

namespace MyMvcApp.Areas.MvcDashboardIdentity.Controllers
{
    public partial class HomeController : BaseController
    {
        #region Construction

        private readonly ApplicationDbContext context;

        public HomeController(ApplicationDbContext context)
        {
            this.context = context;
        }

        #endregion

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult TopMenu()
        {
            var model = new TopMenuModel
            {
                UserCount = context.Users.Count(),
                RoleCount = context.Roles.Count()
            };
            return PartialView(model);
        }

        [HttpGet]
        public IActionResult GetStarted()
        {
            return View();
        }
    }
}

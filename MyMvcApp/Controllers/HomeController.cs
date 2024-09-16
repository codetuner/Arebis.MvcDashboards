using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyMvcApp.Models;
using System;
using System.Diagnostics;
using System.Threading;

namespace MyMvcApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Fail()
        {
            var ex = new ApplicationException("This is a failing call.");
            ex.Data["Source"] = "Fail action of HomeController.";
            ex.Data["TickCount"] = Environment.TickCount;
            throw ex;
        }

        public IActionResult Slow()
        {
            Thread.Sleep(5000);
            return RedirectToAction("Index");
        }
    }
}
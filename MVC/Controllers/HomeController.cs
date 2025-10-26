using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using System.Diagnostics;

namespace MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }


        //private IServices _services;

        //public HomeController(IServices services)
        //{
            //_services = services;
        //}



        public IActionResult Index()
        {
            return View();
        }

        public IActionResult NER()
        {
            return View("~/Views/Home/NER/NER.cshtml");
        }

        public IActionResult PV()
        {
            return View("~/Views/Home/NER/PV/PV.cshtml");
        }

        public IActionResult MiniHydraulique()
        {
            return View("~/Views/Home/NER/Mini-hydraulique/MiniHydraulique.cshtml");
        }

        public IActionResult Eolien()
        {
            return View("~/Views/Home/NER/Eolien/Eolien.cshtml");
        }

        public IActionResult Biogaz()
        {
            return View("~/Views/Home/NER/Biogaz/Biogaz.cshtml");
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
    }
}

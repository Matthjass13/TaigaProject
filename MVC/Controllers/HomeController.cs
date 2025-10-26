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
            var vm = BuildFakeProductionVm();
            return View(vm);
        }

        private ProductionChartVm BuildFakeProductionVm()
        {
            int currentYear = DateTime.Now.Year;
            var years = Enumerable.Range(currentYear - 9, 10).ToList(); // 10 dernières années

            // Données fictives (ex: croissance légère). Tu peux ajuster.
            var rnd = new Random(42);
            var kwh = years
                .Select((y, i) => 100_000 + i * 8_000 + rnd.Next(-3_000, 3_000)) // kWh
                .Select(v => Math.Max(0, v)) // pas de valeurs négatives
                .Select(v => (double)v)
                .ToList();

            return new ProductionChartVm
            {
                Years = years,
                KWh = kwh,
                Title = "Production [kWh]"
            };
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

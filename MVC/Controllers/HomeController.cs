using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using ClassLibrary.DataAccessLayer;
using ClassLibrary.Models;

namespace MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ValaisContext _ctx;

        public HomeController(ILogger<HomeController> logger, ValaisContext ctx)
        {
            _logger = logger;
            _ctx = ctx;
        }



        public IActionResult Index()
        {
            var vm = BuildDbProductionVm();
            return View(vm);
        }

        private ProductionChartVm BuildDbProductionVm()
        {
            const string ENERGY_NAME = "Production cantonale brute";

            // jointure pour récupérer (Année, Valeur) pour le type demandé
            var rows = _ctx.YearlyProduction
                .Include(p => p.Year)
                .Include(p => p.EnergyType)
                .Where(p => p.EnergyType.Name == ENERGY_NAME)
                .OrderByDescending(p => p.Year.Year)   // du plus récent au plus ancien
                .Take(10)                               // 10 dernières années
                .AsNoTracking()
                .ToList();

            // remettre dans l’ordre chronologique pour le graphique
            rows.Reverse();

            var years = rows.Select(r => (int)r.Year.Year).ToList();
            var values = rows.Select(r => (double)r.ValueGWh).ToList();

            return new ProductionChartVm
            {
                Title = "Production [kWh]",
                Years = years,
                KWh = values
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

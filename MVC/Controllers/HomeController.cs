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
            var pie = BuildProductionBrutePie();
            ViewBag.NerLabels = pie.labels;
            ViewBag.NerValues = pie.values;
            ViewBag.NerYear = pie.year;
            return View(vm);
        }

        // GET
        [HttpGet]
        public IActionResult PrivateInstallation(int step = 1)
        {
            if (step < 1) step = 1;
            if (step > 4) step = 4;

            var vm = new PrivateInstallationVm
            {
                Step = step
            };

            return View("~/Views/Home/PrivateInstallation/PrivateInstallation.cshtml", vm);
        }

        // POST (quand on valide une étape)
        [HttpPost]
        public IActionResult PrivateInstallation(PrivateInstallationVm vm)
        {
            // Étape 3 : validation orientation
            if (vm.Step == 3 && !ModelState.IsValid)
            {
                return View("~/Views/Home/PrivateInstallation/PrivateInstallation.cshtml", vm);
            }

            // Étape 4 : clic sur le bouton "Save"
            if (vm.Step == 4)
            {
                // Ici tu pourrais sauvegarder en DB plus tard

                // On simule un numéro d'enregistrement
                var registrationNumber = "9999999";
                TempData["RegistrationNumber"] = registrationNumber;

                return RedirectToAction("PrivateInstallationConfirmation");
            }

            // Étapes 1 -> 2 -> 3 : on avance simplement
            vm.Step = Math.Min(vm.Step + 1, 4);
            ModelState.Clear();

            return View("~/Views/Home/PrivateInstallation/PrivateInstallation.cshtml", vm);
        }

        // Page de confirmation
        public IActionResult PrivateInstallationConfirmation()
        {
            var reg = TempData["RegistrationNumber"]?.ToString() ?? "9999999";
            ViewBag.RegistrationNumber = reg;

            return View("~/Views/Home/PrivateInstallation/PrivateInstallationConfirmation.cshtml");
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

        private (List<string> labels, List<double> values, int year) BuildProductionBrutePie()
        {
            // Dernière année disponible
            var latestYear = _ctx.Yearly
                .OrderByDescending(y => y.Year)
                .Select(y => (int)y.Year)
                .First();

            // Types qui composent le total (adapter les libellés si dans ta DB ils diffèrent)
            var allowed = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "Centrales hydrauliques - Total", "Hydraulique" },
        { "Centrales thermiques - Total",   "Thermiques"  },
        { "Installations biogaz",           "Biogaz"      },
        { "Installations photovoltaïques",  "Photovoltaïque" },
        { "Installations éoliennes",        "Éolien"      },
    };

            // Récupère les lignes composants pour la dernière année
            var rows = _ctx.YearlyProduction
                .Include(p => p.EnergyType)
                .Include(p => p.Year)
                .Where(p => p.Year.Year == latestYear && allowed.Keys.Contains(p.EnergyType.Name))
                .AsNoTracking()
                .ToList();

            // Trie les labels dans l'ordre défini ci-dessus
            var labels = new List<string>();
            var values = new List<double>();
            foreach (var kv in allowed)
            {
                var match = rows.FirstOrDefault(r => r.EnergyType.Name.Equals(kv.Key, StringComparison.OrdinalIgnoreCase));
                if (match != null)
                {
                    labels.Add(kv.Value);
                    values.Add((double)match.ValueGWh);
                }
            }

            return (labels, values, latestYear);
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

using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using ClassLibrary.DataAccessLayer;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
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

        private const string InstallationSessionKey = "PrivateInstallationVm";

        private PrivateInstallationVm LoadInstallationFromSession()
        {
            var json = HttpContext.Session.GetString(InstallationSessionKey);
            if (string.IsNullOrEmpty(json))
                return new PrivateInstallationVm();

            return JsonSerializer.Deserialize<PrivateInstallationVm>(json) ?? new PrivateInstallationVm();
        }

        private void SaveInstallationToSession(PrivateInstallationVm vm)
        {
            var json = JsonSerializer.Serialize(vm);
            HttpContext.Session.SetString(InstallationSessionKey, json);
        }


        // GET
        [HttpGet]
        public IActionResult PrivateInstallation(int step = 1)
        {
            if (step < 1) step = 1;
            if (step > 4) step = 4;

            var vm = LoadInstallationFromSession();
            vm.Step = step;

            return View("~/Views/Home/PrivateInstallation/PrivateInstallation.cshtml", vm);
        }

        [HttpPost]
        public IActionResult PrivateInstallation(PrivateInstallationVm vm, string? action)

        {
            // On récupère ce qu'on avait déjà en session
            var stored = LoadInstallationFromSession() ?? new PrivateInstallationVm();

            switch (vm.Step)
            {
                // ÉTAPE 1 : Localisation
                case 1:
                    stored.Rue = vm.Rue;
                    stored.No = vm.No;
                    stored.NPA = vm.NPA;
                    stored.Localite = vm.Localite;
                    stored.Step = 1;

                    ModelState.Clear();
                    if (string.IsNullOrWhiteSpace(stored.Rue))
                        ModelState.AddModelError(nameof(vm.Rue), "Veuillez entrer la rue.");
                    if (!stored.No.HasValue || stored.No <= 0)
                        ModelState.AddModelError(nameof(vm.No), "Le numéro de rue doit être supérieur à 0.");
                    if (!stored.NPA.HasValue || stored.NPA < 1000 || stored.NPA > 9999)
                        ModelState.AddModelError(nameof(vm.NPA), "Le NPA suisse doit avoir 4 chiffres.");
                    if (string.IsNullOrWhiteSpace(stored.Localite))
                        ModelState.AddModelError(nameof(vm.Localite), "Veuillez entrer la localité.");

                    if (!ModelState.IsValid)
                        return View("~/Views/Home/PrivateInstallation/PrivateInstallation.cshtml", stored);

                    stored.Step = 2; // on passe à l'étape 2
                    break;

                // ÉTAPE 2 : Type d'installation
                case 2:
                    stored.SelectedEnergyType = vm.SelectedEnergyType;
                    stored.SelectedIntegrationType = vm.SelectedIntegrationType;
                    stored.SelectedSolarCellType = vm.SelectedSolarCellType;
                    stored.Step = 3;  // on va à l'étape 3
                    break;

                // ÉTAPE 3 : Orientation
                case 3:
                    // on sauvegarde les valeurs dans l'objet en session
                    stored.OrientationAzimut = vm.OrientationAzimut;
                    stored.ToitureInclinaison = vm.ToitureInclinaison;
                    stored.Step = 3;

                    // validation *uniquement* pour ces deux champs
                    ModelState.Clear();
                    if (!stored.OrientationAzimut.HasValue ||
                        stored.OrientationAzimut < 0 || stored.OrientationAzimut > 360)
                    {
                        ModelState.AddModelError(nameof(vm.OrientationAzimut),
                            "L'orientation doit être entre 0° et 360°.");
                    }

                    if (!stored.ToitureInclinaison.HasValue ||
                        stored.ToitureInclinaison < 0 || stored.ToitureInclinaison > 90)
                    {
                        ModelState.AddModelError(nameof(vm.ToitureInclinaison),
                            "L'inclinaison doit être entre 0° et 90°.");
                    }

                    if (!ModelState.IsValid)
                    {
                        // on reste à l'étape 3 avec les valeurs saisies
                        return View("~/Views/Home/PrivateInstallation/PrivateInstallation.cshtml", stored);
                    }

                    stored.Step = 4; // on passe à l'étape 4
                    break;

                case 4:
                    // on récupère les valeurs saisies
                    stored.Longueur = vm.Longueur;
                    stored.Largeur = vm.Largeur;
                    stored.Step = 4;

                    // validation
                    ModelState.Clear();
                    if (!stored.Longueur.HasValue || stored.Longueur <= 0)
                        ModelState.AddModelError(nameof(vm.Longueur), "La longueur doit être supérieure à 0.");
                    if (!stored.Largeur.HasValue || stored.Largeur <= 0)
                        ModelState.AddModelError(nameof(vm.Largeur), "La largeur doit être supérieure à 0.");

                    if (!ModelState.IsValid)
                    {
                        // on reste à l'étape 4 avec les erreurs et les valeurs
                        return View("~/Views/Home/PrivateInstallation/PrivateInstallation.cshtml", stored);
                    }

                    // si on a cliqué sur "Précédent"
                    if (action == "previous")
                    {
                        stored.Step = 3;           // on revient à l'étape 3
                        break;                     // on sort du switch, puis on affiche la vue avec stored
                    }

                    // sinon, c'est le bouton "save" on enregistre en DB
                    var surface = stored.Longueur.Value * stored.Largeur.Value;

                    var entity = new Installation
                    {
                        Rue = stored.Rue ?? "",
                        No = stored.No ?? 0,
                        Npa = stored.NPA ?? 0,
                        Localite = stored.Localite ?? "",

                        SelectedEnergyType = stored.SelectedEnergyType,
                        SelectedSolarCellType = stored.SelectedSolarCellType,
                        SelectedIntegrationType = stored.SelectedIntegrationType,

                        OrientationAzimut = stored.OrientationAzimut ?? 0,
                        ToitureInclinaison = stored.ToitureInclinaison ?? 0,

                        Longueur = stored.Longueur.Value,
                        Largeur = stored.Largeur.Value,
                        Surface = surface
                    };

                    _ctx.Installations.Add(entity);
                    _ctx.SaveChanges();

                    TempData["RegistrationNumber"] = entity.NoRegistration;
                    HttpContext.Session.Remove(InstallationSessionKey);

                    return RedirectToAction("PrivateInstallationConfirmation");

            }

            // on sauvegarde l'état courant en session et on affiche l'étape suivante
            SaveInstallationToSession(stored);
            ModelState.Clear();
            return View("~/Views/Home/PrivateInstallation/PrivateInstallation.cshtml", stored);
        }



        // Page de confirmation
        public IActionResult PrivateInstallationConfirmation()
        {
            ViewBag.RegistrationNumber = TempData["RegistrationNumber"];
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

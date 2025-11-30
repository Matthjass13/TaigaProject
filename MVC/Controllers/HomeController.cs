using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using MVC.Services;
using System.Text.Json;

namespace MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly IValaisServices _service;
        private const string InstallationSessionKey = "PrivateInstallationVm";

        public HomeController(IValaisServices service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var globalChart = await _service.GetChartAsync();
            var pvChart = await _service.GetPvChartAsync();
            var pie = await _service.GetPieAsync();

            ViewBag.NerLabels = pie.Labels;
            ViewBag.NerValues = pie.Values;
            ViewBag.NerYear = pie.Year;

            var homeVm = new HomeDashboardVm
            {
                GlobalChart = globalChart,
                PvChart = pvChart
            };

            return View(homeVm);
        }

        private void SaveInstallationToSession(PrivateInstallationVm vm)
        {
            var json = JsonSerializer.Serialize(vm);
            HttpContext.Session.SetString(InstallationSessionKey, json);
        }

        private PrivateInstallationVm LoadInstallationFromSession()
        {
            var json = HttpContext.Session.GetString(InstallationSessionKey);
            if (string.IsNullOrEmpty(json))
                return new PrivateInstallationVm();

            return JsonSerializer.Deserialize<PrivateInstallationVm>(json) ?? new PrivateInstallationVm();
        }

        [HttpGet]
        public IActionResult PrivateInstallation(int step = 1)
        {
            step = Math.Clamp(step, 1, 4);

            var vm = LoadInstallationFromSession();
            vm.Step = step;

            return View("~/Views/Home/PrivateInstallation/PrivateInstallation.cshtml", vm);
        }

        [HttpPost]
        public async Task<IActionResult> PrivateInstallation(PrivateInstallationVm vm, string? action)
        {
            var stored = LoadInstallationFromSession();

            switch (vm.Step)
            {
                case 1:
                    stored.Rue = vm.Rue;
                    stored.No = vm.No;
                    stored.NPA = vm.NPA;
                    stored.Localite = vm.Localite;

                    ModelState.Clear();
                    if (string.IsNullOrWhiteSpace(stored.Rue))
                        ModelState.AddModelError(nameof(vm.Rue), "Veuillez entrer la rue.");
                    if (!stored.No.HasValue || stored.No <= 0)
                        ModelState.AddModelError(nameof(vm.No), "Le num�ro de rue doit �tre sup�rieur � 0.");
                    if (!stored.NPA.HasValue || stored.NPA < 1000 || stored.NPA > 9999)
                        ModelState.AddModelError(nameof(vm.NPA), "Le NPA suisse doit avoir 4 chiffres.");
                    if (string.IsNullOrWhiteSpace(stored.Localite))
                        ModelState.AddModelError(nameof(vm.Localite), "Veuillez entrer la localit�.");

                    if (!ModelState.IsValid)
                        return View("~/Views/Home/PrivateInstallation/PrivateInstallation.cshtml", stored);

                    stored.Step = 2;
                    break;

               
                case 2:
                    stored.SelectedEnergyType = vm.SelectedEnergyType;
                    stored.SelectedIntegrationType = vm.SelectedIntegrationType;
                    stored.SelectedSolarCellType = vm.SelectedSolarCellType;

                    stored.Step = 3;
                    break;

                case 3:
                    stored.OrientationAzimut = vm.OrientationAzimut;
                    stored.ToitureInclinaison = vm.ToitureInclinaison;

                    ModelState.Clear();

                    if (!stored.OrientationAzimut.HasValue ||
                        stored.OrientationAzimut < 0 || stored.OrientationAzimut > 360)
                        ModelState.AddModelError(nameof(vm.OrientationAzimut),
                            "L'orientation doit �tre entre 0� et 360�.");

                    if (!stored.ToitureInclinaison.HasValue ||
                        stored.ToitureInclinaison < 0 || stored.ToitureInclinaison > 90)
                        ModelState.AddModelError(nameof(vm.ToitureInclinaison),
                            "L'inclinaison doit �tre entre 0� et 90�.");

                    if (!ModelState.IsValid)
                        return View("~/Views/Home/PrivateInstallation/PrivateInstallation.cshtml", stored);

                    stored.Step = 4;
                    break;

             
                case 4:
                    stored.Longueur = vm.Longueur;
                    stored.Largeur = vm.Largeur;

                    ModelState.Clear();

                    if (!stored.Longueur.HasValue || stored.Longueur <= 0)
                        ModelState.AddModelError(nameof(vm.Longueur), "La longueur doit �tre sup�rieure � 0.");
                    if (!stored.Largeur.HasValue || stored.Largeur <= 0)
                        ModelState.AddModelError(nameof(vm.Largeur), "La largeur doit �tre sup�rieure � 0.");

                    if (!ModelState.IsValid)
                        return View("~/Views/Home/PrivateInstallation/PrivateInstallation.cshtml", stored);

              
                    if (action == "previous")
                    {
                        stored.Step = 3;
                        break;
                    }

                  
                    var registrationNumber = await _service.CreateInstallationAsync(stored);

                    TempData["RegistrationNumber"] = registrationNumber;
                    HttpContext.Session.Remove(InstallationSessionKey);

                    return RedirectToAction("PrivateInstallationConfirmation");
            }

            SaveInstallationToSession(stored);
            ModelState.Clear();

            return View("~/Views/Home/PrivateInstallation/PrivateInstallation.cshtml", stored);
        }


        public IActionResult PrivateInstallationConfirmation()
        {
            ViewBag.RegistrationNumber = TempData["RegistrationNumber"];
            return View("~/Views/Home/PrivateInstallation/PrivateInstallationConfirmation.cshtml");
        }

        public IActionResult NER() => View("~/Views/Home/NER/NER.cshtml");
        
        public async Task<IActionResult> PV()
        {
            var pvChart = await _service.GetPvChartAsync();
            return View("~/Views/Pv/Index.cshtml", pvChart);
        }
        
        public IActionResult MiniHydraulique() => View("~/Views/Home/NER/Mini-hydraulique/MiniHydraulique.cshtml");
        public IActionResult Eolien() => View("~/Views/Home/NER/Eolien/Eolien.cshtml");
        public IActionResult Biogaz() => View("~/Views/Home/NER/Biogaz/Biogaz.cshtml");

        public IActionResult Privacy() => View();
    }
}

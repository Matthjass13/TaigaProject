using Microsoft.AspNetCore.Mvc;
using MVC.Services;

namespace MVC.Controllers
{
    public class PvController : Controller
    {
        private readonly IValaisServices _services;
        public PvController(IValaisServices services)
        {
            _services = services;
        }

        // GET: /Pv
        public async Task<IActionResult> Index()
        {
            var vm = await _services.GetPvChartAsync();
            return View(vm);
        }
    }
}

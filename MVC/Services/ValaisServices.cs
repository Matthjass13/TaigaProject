using MVC.Models;
using System.Net.Http.Json;

namespace MVC.Services
{
    public class ValaisServices : IValaisServices
    {
        private readonly HttpClient _client;

        public ValaisServices(HttpClient client)
        {
            _client = client;
        }

        // ---- CHART ----
        public async Task<ProductionChartVm> GetChartAsync()
        {
            var result = await _client.GetFromJsonAsync<ProductionChartVm>("/api/Production/chart");
            return result ?? new ProductionChartVm();
        }

        // ---- PIE ----
        public async Task<(List<string>, List<double>, int)> GetPieAsync()
        {
            var dto = await _client.GetFromJsonAsync<ProductionPieDto>("/api/Production/pie");

            if (dto == null)
                return (new(), new(), 0);

            return (dto.Labels, dto.Values, dto.Year);
        }

        // ---- INSTALLATION ----
        public async Task<int> CreateInstallationAsync(PrivateInstallationVm vm)
        {
            var payload = new PrivateInstallationDto
            {
                Rue = vm.Rue,
                No = vm.No,
                NPA = vm.NPA,
                Localite = vm.Localite,
                SelectedEnergyType = vm.SelectedEnergyType,
                SelectedSolarCellType = vm.SelectedSolarCellType,
                SelectedIntegrationType = vm.SelectedIntegrationType,
                OrientationAzimut = vm.OrientationAzimut,
                ToitureInclinaison = vm.ToitureInclinaison,
                Longueur = vm.Longueur,
                Largeur = vm.Largeur
            };

            var response = await _client.PostAsJsonAsync("/api/Production/installations", payload);

            return await response.Content.ReadFromJsonAsync<int>();
        }
    }

    // --- DTO locaux MVC (correspondent aux DTO API) ---
    public class ProductionPieDto
    {
        public List<string> Labels { get; set; } = new();
        public List<double> Values { get; set; } = new();
        public int Year { get; set; }
    }

    public class PrivateInstallationDto
    {
        public string? Rue { get; set; }
        public int? No { get; set; }
        public int? NPA { get; set; }
        public string? Localite { get; set; }
        public string? SelectedEnergyType { get; set; }
        public string? SelectedSolarCellType { get; set; }
        public string? SelectedIntegrationType { get; set; }
        public double? OrientationAzimut { get; set; }
        public double? ToitureInclinaison { get; set; }
        public double? Longueur { get; set; }
        public double? Largeur { get; set; }
    }
}



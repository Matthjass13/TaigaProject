using ClassLibrary.DataAccessLayer;
using ClassLibrary.Models;
using Microsoft.EntityFrameworkCore;
using WebAPI.Models;

namespace WebAPI.Business
{
    public class ValaisBusiness : IValaisBusiness
    {
        private readonly ValaisContext _ctx;

        public ValaisBusiness(ValaisContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<ProductionChartDto> GetProductionChartAsync()
        {
            const string ENERGY = "Production cantonale brute";

            var rows = await _ctx.YearlyProduction
                .Include(x => x.Year)
                .Include(x => x.EnergyType)
                .Where(x => x.EnergyType.Name == ENERGY)
                .OrderByDescending(x => x.Year.Year)
                .Take(10)
                .AsNoTracking()
                .ToListAsync();

            rows.Reverse();

            return new ProductionChartDto
            {
                Title = "Production [kWh]",
                Years = rows.Select(r => (int)r.Year.Year).ToList(),
                KWh = rows.Select(r => (double)r.ValueGWh).ToList()
            };
        }

        public async Task<ProductionPieDto> GetProductionPieAsync()
        {
            var latestYear = await _ctx.Yearly
                .OrderByDescending(y => y.Year)
                .Select(y => (int)y.Year)
                .FirstAsync();

            var allowed = new Dictionary<string, string>
            {
                { "Centrales hydrauliques - Total", "Hydraulique" },
                { "Centrales thermiques - Total", "Thermiques" },
                { "Installations biogaz", "Biogaz" },
                { "Installations photovoltaïques", "Photovoltaïque" },
                { "Installations éoliennes", "Éolien" }
            };

            var rows = await _ctx.YearlyProduction
                .Include(p => p.EnergyType)
                .Include(p => p.Year)
                .Where(p => p.Year.Year == latestYear &&
                            allowed.Keys.Contains(p.EnergyType.Name))
                .AsNoTracking()
                .ToListAsync();

            var dto = new ProductionPieDto
            {
                Year = latestYear
            };

            foreach (var kv in allowed)
            {
                var match = rows.FirstOrDefault(r => r.EnergyType.Name.Equals(kv.Key));
                if (match != null)
                {
                    dto.Labels.Add(kv.Value);
                    dto.Values.Add((double)match.ValueGWh);
                }
            }

            return dto;
        }

        public async Task<int> CreateInstallationAsync(PrivateInstallationDto dto)
        {
            var entity = new Installation
            {
                Rue = dto.Rue,
                No = dto.No,
                Npa = dto.Npa,
                Localite = dto.Localite,
                SelectedEnergyType = dto.SelectedEnergyType,
                SelectedSolarCellType = dto.SelectedSolarCellType,
                SelectedIntegrationType = dto.SelectedIntegrationType,
                OrientationAzimut = dto.OrientationAzimut,
                ToitureInclinaison = dto.ToitureInclinaison,
                Longueur = dto.Longueur,
                Largeur = dto.Largeur,
                Surface = dto.Longueur * dto.Largeur
            };

            _ctx.Installations.Add(entity);
            await _ctx.SaveChangesAsync();

            return entity.NoRegistration;
        }
    }
}


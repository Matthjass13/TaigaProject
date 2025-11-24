using ClassLibrary.DataAccessLayer;
using ClassLibrary.Models;
using Microsoft.EntityFrameworkCore;
using WebAPI.Models;

namespace WebAPI.Business
{
    public class ValaisBusiness : IValaisBusiness
    {
        private readonly ValaisContext _ctx;
        private readonly ILogger<ValaisBusiness> _logger;

        public ValaisBusiness(ValaisContext ctx, ILogger<ValaisBusiness> logger)
        {
            _ctx = ctx;
            _logger = logger;
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

            var dto = new ProductionChartDto
            {
                Title = "Production [kWh]",
                Years = rows.Select(r => (int)r.Year.Year).ToList(),
                KWh = rows.Select(r => (double)r.ValueGWh).ToList()
            };

            // Ajouter 2025 avec production PV calculée
            try
            {
                var installs2025 = await _ctx.Installations
                    .Where(i => i.SelectedEnergyType != null && 
                               i.SelectedEnergyType.ToUpper().Contains("PHOTO"))
                    .AsNoTracking()
                    .ToListAsync();

                double prod2025KWh = 0;
                foreach (var inst in installs2025)
                {
                    prod2025KWh += ComputePvInstallationKWh(inst);
                }
                double prod2025GWh = prod2025KWh / 1_000_000.0;

                _logger.LogInformation("GetProductionChartAsync: Ajout 2025 avec {GWh} GWh PV", prod2025GWh);

                dto.Years.Add(2025);
                dto.KWh.Add(prod2025GWh);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur calcul PV 2025 pour chart global");
                // Ajouter 2025 avec 0 si erreur
                dto.Years.Add(2025);
                dto.KWh.Add(0);
            }

            return dto;
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
            try
            {
                _logger.LogInformation("CreateInstallationAsync: Création installation {Type}", dto.SelectedEnergyType);

                // Générer le prochain numéro d'enregistrement
                var maxRegistration = await _ctx.Installations
                    .MaxAsync(i => (int?)i.NoRegistration) ?? 0;
                
                var nextRegistration = maxRegistration + 1;
                
                _logger.LogInformation("CreateInstallationAsync: Numéro généré {No}", nextRegistration);

                var entity = new Installation
                {
                    NoRegistration = nextRegistration,
                    Rue = dto.Rue ?? string.Empty,
                    No = dto.No,
                    Npa = dto.Npa,
                    Localite = dto.Localite ?? string.Empty,
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

                _logger.LogInformation("CreateInstallationAsync: Installation créée avec ID {Id}", entity.NoRegistration);
                return entity.NoRegistration;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur création installation: {Message}", ex.Message);
                if (ex.InnerException != null)
                {
                    _logger.LogError("InnerException: {InnerMessage}", ex.InnerException.Message);
                }
                throw;
            }
        }

        // ---- PV CHART (2010-2018 + 2025 calculé) ----
        public async Task<ProductionChartDto> GetPvChartAsync()
        {
            const string PV_ENERGY_NAME = "Installations photovoltaïques";
            
            var dto = new ProductionChartDto
            {
                Title = "Production PV [GWh]",
                Years = new List<int>(),
                KWh = new List<double>()
            };

            try
            {
                _logger.LogInformation("GetPvChartAsync: START");

                // 1. Charger historique 2010-2018
                try
                {
                    var historical = await _ctx.YearlyProduction
                        .Include(x => x.Year)
                        .Include(x => x.EnergyType)
                        .Where(x => x.EnergyType != null && x.Year != null)
                        .Where(x => x.EnergyType.Name == PV_ENERGY_NAME)
                        .Where(x => x.Year.Year >= 2010 && x.Year.Year <= 2018)
                        .OrderBy(x => x.Year.Year)
                        .AsNoTracking()
                        .ToListAsync();

                    _logger.LogInformation("GetPvChartAsync: {Count} années historiques.", historical.Count);

                    foreach (var h in historical)
                    {
                        if (h.Year != null)
                        {
                            dto.Years.Add((int)h.Year.Year);
                            dto.KWh.Add((double)h.ValueGWh);
                        }
                    }
                }
                catch (Exception exHist)
                {
                    _logger.LogError(exHist, "Erreur chargement historique PV");
                }

                // 2. Calculer production 2025
                try
                {
                    var installs2025 = await _ctx.Installations
                        .Where(i => i.SelectedEnergyType != null && 
                                   i.SelectedEnergyType.ToUpper().Contains("PHOTO"))
                        .AsNoTracking()
                        .ToListAsync();

                    _logger.LogInformation("GetPvChartAsync: {Count} installations 2025.", installs2025.Count);

                    double prod2025KWh = 0;
                    foreach (var inst in installs2025)
                    {
                        try
                        {
                            prod2025KWh += ComputePvInstallationKWh(inst);
                        }
                        catch (Exception exInst)
                        {
                            _logger.LogWarning(exInst, "Erreur calcul installation {Id}", inst.NoRegistration);
                        }
                    }

                    double prod2025GWh = prod2025KWh / 1_000_000.0;
                    
                    _logger.LogInformation("GetPvChartAsync: Prod 2025 = {GWh} GWh", prod2025GWh);

                    dto.Years.Add(2025);
                    dto.KWh.Add(prod2025GWh);
                }
                catch (Exception ex2025)
                {
                    _logger.LogError(ex2025, "Erreur calcul 2025");
                    // Ajouter 2025 avec 0 si erreur
                    dto.Years.Add(2025);
                    dto.KWh.Add(0);
                }

                _logger.LogInformation("GetPvChartAsync: Retour {Years} années", dto.Years.Count);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GLOBALE GetPvChartAsync");
                
                // Retour fallback minimal
                dto.Years = new List<int> { 2025 };
                dto.KWh = new List<double> { 0 };
                return dto;
            }
        }

        private static double ComputePvInstallationKWh(Installation inst)
        {
            // Rendement spécifique (kWh/m2) selon type cellule
            var tech = inst.SelectedSolarCellType?.ToLower() ?? string.Empty;
            double specificYield = tech.Contains("mono") ? 250.0 : 175.0; // défaut poly si inconnu

            // Facteur orientation (simplifié) : 0° = 1.0, sinon +/-90° ~ 0.8
            double orientation = inst.OrientationAzimut ?? 0;
            double orientationFactor = Math.Abs(orientation) < 1 ? 1.0 : (Math.Abs(Math.Abs(orientation) - 90) < 1 ? 0.8 : 0.8); // si autre valeur, applique 0.8

            // Surface déjà calculée dans entité (Longueur*Largeur)
            double surface = inst.Surface ?? 0;
            if (surface <= 0)
            {
                surface = (inst.Longueur ?? 0) * (inst.Largeur ?? 0);
            }
            return surface * specificYield * orientationFactor;
        }
    }
}


using Moq;
using ClassLibrary.DataAccessLayer;
using ClassLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Business;
using WebAPI.Models;
using Xunit;
using Microsoft.Extensions.Logging;
using System.Threading;
using Microsoft.EntityFrameworkCore.Query;
using System.Threading;


namespace WebAPITest
{
    public class ValaisBusinessMoqTests
    {
        private static Mock<DbSet<T>> CreateDbSetMock<T>(IEnumerable<T> elements) where T : class
        {
            var queryable = elements.AsQueryable();
            var dbSetMock = new Mock<DbSet<T>>();

            // Partie async (EF Core)
            dbSetMock.As<IAsyncEnumerable<T>>()
                     .Setup(m => m.GetAsyncEnumerator(default))
                     .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));

            dbSetMock.As<IQueryable<T>>()
                     .Setup(m => m.Provider)
                     .Returns(new TestAsyncQueryProvider<T>(queryable.Provider));

            // Partie synchro
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

            // Ajout simple (si ton code fait ctx.Set<T>().Add(...))
            dbSetMock.Setup(d => d.Add(It.IsAny<T>())).Callback<T>(e => { });

            return dbSetMock;
        }



        [Fact]
        public async Task GetProductionChartAsync_Returns_Last10Years()
        {
            // Arrange
            var energyType = new EnergyType { EnergyTypeID = 1, Name = "Production cantonale brute" };

            // ICI : on crée 10 années, de 2010 à 2019
            var years = Enumerable.Range(2010, 10)
                                  .Select(y => new Yearly { YearID = y, Year = (short)y })
                                  .ToList();

            var productions = years.Select(y => new YearlyProduction
            {
                Year = y,
                EnergyType = energyType,
                ValueGWh = y.Year * 10
            }).ToList();

            var mockSet = CreateDbSetMock(productions);

            var options = new DbContextOptionsBuilder<ValaisContext>().Options;
            var mockCtx = new Mock<ValaisContext>(options);

            mockCtx.Setup(c => c.YearlyProduction).Returns(mockSet.Object);

            var loggerMock = new Mock<ILogger<ValaisBusiness>>();
            var computationMock = new Mock<IComputation>();
            var business = new ValaisBusiness(mockCtx.Object, loggerMock.Object, computationMock.Object);

            // Act
            var result = await business.GetProductionChartAsync();

            // Assert
            Assert.Equal(11, result.Years.Count);
            Assert.Equal(11, result.KWh.Count);

            // Premières années : ton mock
            Assert.Equal(2010, result.Years.First());

            // DERNIÈRE année : 2025 ajoutée par la méthode
            Assert.Equal(2025, result.Years.Last());

        }


        [Fact]
        public async Task GetProductionPieAsync_Returns_AllowedLabels()
        {
            // Arrange
            short latestYear = 2018;
            var allowed = new Dictionary<string, string>
            {
                { "Centrales hydrauliques - Total", "Hydraulique" },
                { "Centrales thermiques - Total", "Thermiques" },
                { "Installations biogaz", "Biogaz" },
                { "Installations photovoltaïques", "Photovoltaïque" },
                { "Installations éoliennes", "Éolien" }
            };

            var energyTypes = allowed.Keys.Select((k, i) => new EnergyType { EnergyTypeID = i + 1, Name = k }).ToList();
            var yearEntity = new Yearly { YearID = 1, Year = latestYear };

            var productions = energyTypes.Select(et => new YearlyProduction
            {
                Year = yearEntity,
                EnergyType = et,
                ValueGWh = 100
            }).ToList();

            var mockSet = CreateDbSetMock(productions);
            var mockYearSet = CreateDbSetMock(new List<Yearly> { yearEntity });

            var options = new DbContextOptionsBuilder<ValaisContext>().Options;
            var mockCtx = new Mock<ValaisContext>(options);
            mockCtx.Setup(c => c.YearlyProduction).Returns(mockSet.Object);
            mockCtx.Setup(c => c.Yearly).Returns(mockYearSet.Object);


            var loggerMock = new Mock<ILogger<ValaisBusiness>>();
            var computationMock = new Mock<IComputation>();
            var business = new ValaisBusiness(mockCtx.Object, loggerMock.Object, computationMock.Object);

            // Act
            var result = await business.GetProductionPieAsync();

            // Assert
            Assert.Equal(latestYear, result.Year);
            Assert.Equal(allowed.Count, result.Labels.Count);
            Assert.All(result.Values, v => Assert.Equal(100, v));
            foreach (var label in result.Labels)
            {
                Assert.Contains(label, allowed.Values);
            }
        }

        [Fact]
        public async Task CreateInstallationAsync_CreatesEntityAndReturnsNoRegistration()
        {
            // Arrange
            var addedInstallations = new List<Installation>();

            // On simule des installations déjà en DB pour tester le Max()
            var existingInstallations = new List<Installation>
            {
                new Installation { NoRegistration = 3 },
                new Installation { NoRegistration = 5 }   // => Max = 5, donc next = 6
            };

            // DbSet mocké qui supporte LINQ (Select, Max, etc.)
            var mockSet = CreateDbSetMock(existingInstallations);

            // On capture ce qui est ajouté pendant CreateInstallationAsync
            mockSet.Setup(d => d.Add(It.IsAny<Installation>()))
                   .Callback<Installation>(i => addedInstallations.Add(i));

            mockSet.Setup(d => d.AddAsync(It.IsAny<Installation>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync((Installation i, CancellationToken _) =>
                   {
                       addedInstallations.Add(i);
                       return null!; // on se fiche de l'EntityEntry en test
                   });

            var options = new DbContextOptionsBuilder<ValaisContext>().Options;
            var mockCtx = new Mock<ValaisContext>(options);
            mockCtx.Setup(c => c.Installations).Returns(mockSet.Object);
            mockCtx.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(1);

            var loggerMock = new Mock<ILogger<ValaisBusiness>>();
            var computationMock = new Mock<IComputation>();
            var business = new ValaisBusiness(mockCtx.Object, loggerMock.Object, computationMock.Object);

            var dto = new PrivateInstallationDto
            {
                Rue = "Rue Test",
                No = 1,
                Npa = 1000,
                Localite = "Localité",
                SelectedEnergyType = "Hydraulique",
                SelectedSolarCellType = "Monochristallin",
                SelectedIntegrationType = "Intégrée",
                OrientationAzimut = 45,
                ToitureInclinaison = 30,
                Longueur = 10,
                Largeur = 5
            };

            // Act
            var noRegistration = await business.CreateInstallationAsync(dto);

            // Assert : comme Max(NoRegistration) = 5, le prochain doit être 6
            Assert.Equal(6, noRegistration);

            var entity = Assert.Single(addedInstallations); // il doit y avoir exactement 1 ajout
            Assert.Equal(dto.Rue, entity.Rue);
            Assert.Equal(dto.Longueur * dto.Largeur, entity.Surface);
            Assert.Equal(dto.No, entity.No);
            Assert.Equal(dto.Npa, entity.Npa);
            Assert.Equal(dto.Localite, entity.Localite);
            Assert.Equal(dto.SelectedEnergyType, entity.SelectedEnergyType);
            Assert.Equal(dto.SelectedSolarCellType, entity.SelectedSolarCellType);
            Assert.Equal(dto.SelectedIntegrationType, entity.SelectedIntegrationType);
            Assert.Equal(dto.OrientationAzimut, entity.OrientationAzimut);
            Assert.Equal(dto.ToitureInclinaison, entity.ToitureInclinaison);

            // SaveChangesAsync doit être appelé une fois
            mockCtx.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetPvChartAsync_Contains2025YearWithValue()
        {
            // Arrange
            const string PV_ENERGY_NAME = "Installations photovoltaïques";

            // Historique PV (on en met, mais on ne va pas tester le nombre exact)
            var pvEnergy = new EnergyType { EnergyTypeID = 1, Name = PV_ENERGY_NAME };

            var years = Enumerable.Range(2010, 9) // 2010..2018
                                  .Select(y => new Yearly { YearID = y, Year = (short)y })
                                  .ToList();

            var historical = years.Select(y => new YearlyProduction
            {
                Year = y,
                EnergyType = pvEnergy,
                ValueGWh = 10
            }).ToList();

            var mockYearlyProdSet = CreateDbSetMock(historical);

            // Installations PV pour 2025 (pour calculer la prod 2025)
            var installs2025 = new List<Installation>
    {
        new Installation
        {
            SelectedEnergyType = PV_ENERGY_NAME,
            SelectedSolarCellType = "Monochristallin",
            Longueur = 10,
            Largeur = 5,
            OrientationAzimut = 0
        }
    };
            var mockInstSet = CreateDbSetMock(installs2025);

            var options = new DbContextOptionsBuilder<ValaisContext>().Options;
            var mockCtx = new Mock<ValaisContext>(options);
            mockCtx.Setup(c => c.YearlyProduction).Returns(mockYearlyProdSet.Object);
            mockCtx.Setup(c => c.Installations).Returns(mockInstSet.Object);

            var loggerMock = new Mock<ILogger<ValaisBusiness>>();
            var computationMock = new Mock<IComputation>();
            var business = new ValaisBusiness(mockCtx.Object, loggerMock.Object, computationMock.Object);

            // Act
            var result = await business.GetPvChartAsync();

            // Assert

            // 1) Titre
            Assert.Equal("Production PV [GWh]", result.Title);

            // 2) Il y a au moins une année
            Assert.NotEmpty(result.Years);
            Assert.Equal(result.Years.Count, result.KWh.Count);

            // 3) L’année 2025 est présente
            Assert.Contains(2025, result.Years);
            var index2025 = result.Years.IndexOf(2025);

            // 4) Sa valeur est >= 0 (on s’assure que le calcul s’est fait sans exception)
            Assert.True(result.KWh[index2025] >= 0);
        }




    }
}


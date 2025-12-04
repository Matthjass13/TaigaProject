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
            var years = Enumerable.Range(2010, 9).Select(y => new Yearly { YearID = y, Year = (short)y }).ToList();
            var productions = years.Select(y => new YearlyProduction
            {
                Year = y,
                EnergyType = energyType,
                ValueGWh = y.Year * 10
            }).ToList();

            var mockSet = CreateDbSetMock(productions);
            var mockCtx = new Mock<ValaisContext>();
            mockCtx.Setup(c => c.YearlyProduction).Returns(mockSet.Object);

            var loggerMock = new Mock<ILogger<ValaisBusiness>>();
            var business = new ValaisBusiness(mockCtx.Object, loggerMock.Object);
            // Act
            var result = await business.GetProductionChartAsync();

            // Assert
            Assert.Equal(10, result.Years.Count);
            Assert.Equal(10, result.KWh.Count);
            Assert.Equal(2010, result.Years.First());
            Assert.Equal(2018, result.Years.Last());
            Assert.Equal("Production [kWh]", result.Title);
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

            var mockCtx = new Mock<ValaisContext>();
            mockCtx.Setup(c => c.YearlyProduction).Returns(mockSet.Object);
            mockCtx.Setup(c => c.Yearly).Returns(mockYearSet.Object);

            var loggerMock = new Mock<ILogger<ValaisBusiness>>();
            var business = new ValaisBusiness(mockCtx.Object, loggerMock.Object);

            // Act
            var result = await business.GetProductionPieAsync();

            // Assert
            Assert.Equal(latestYear, result.Year);
            Assert.Equal(allowed.Count, result.Labels.Count);
            Assert.All(result.Values, v => Assert.Equal(100, v));
        }

        [Fact]
        public async Task CreateInstallationAsync_CreatesEntityAndReturnsNoRegistration()
        {
            // Arrange
            var addedInstallations = new List<Installation>();
            var mockSet = new Mock<DbSet<Installation>>();
            mockSet.Setup(d => d.Add(It.IsAny<Installation>())).Callback<Installation>(i => addedInstallations.Add(i));

            var mockCtx = new Mock<ValaisContext>();
            mockCtx.Setup(c => c.Installations).Returns(mockSet.Object);
            mockCtx.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            var loggerMock = new Mock<ILogger<ValaisBusiness>>();
            var business = new ValaisBusiness(mockCtx.Object, loggerMock.Object);

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

            // Assert
            var entity = addedInstallations.FirstOrDefault();
            Assert.NotNull(entity);
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
        }
    }
}


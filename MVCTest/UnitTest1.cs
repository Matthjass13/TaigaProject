using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using MVC.Controllers;
using MVC.Models;
using MVC.Services;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MVCTest
{
    public class HomeControllerTests
    {
        private static HomeController CreateControllerWithSession(IValaisServices service)
        {
            var controller = new HomeController(service);

            var httpContext = new DefaultHttpContext();
            httpContext.Session = new FakeSession();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var tempDataProvider = new Mock<ITempDataProvider>();
            controller.TempData = new TempDataDictionary(httpContext, tempDataProvider.Object);

            return controller;
        }

        [Fact]
        public async Task Index_ReturnsViewWithHomeDashboardVm_AndPieInViewBag()
        {
            var serviceMock = new Mock<IValaisServices>();

            var fakeGlobalChart = new ProductionChartVm
            {
                Title = "Global",
                Years = new List<int> { 2010, 2011 },
                KWh = new List<double> { 10, 20 }
            };

            var fakePvChart = new ProductionChartVm
            {
                Title = "PV",
                Years = new List<int> { 2020 },
                KWh = new List<double> { 5 }
            };

            var fakePie = (
                Labels: new List<string> { "Hydro", "PV" },
                Values: new List<double> { 80, 20 },
                Year: 2024
            );

            serviceMock.Setup(s => s.GetChartAsync()).ReturnsAsync(fakeGlobalChart);
            serviceMock.Setup(s => s.GetPvChartAsync()).ReturnsAsync(fakePvChart);
            serviceMock.Setup(s => s.GetPieAsync()).ReturnsAsync(fakePie);

            var controller = CreateControllerWithSession(serviceMock.Object);

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<HomeDashboardVm>(viewResult.Model);

            Assert.Same(fakeGlobalChart, model.GlobalChart);
            Assert.Same(fakePvChart, model.PvChart);
            Assert.Equal(fakePie.Labels, controller.ViewBag.NerLabels);
            Assert.Equal(fakePie.Values, controller.ViewBag.NerValues);
            Assert.Equal(fakePie.Year, controller.ViewBag.NerYear);
        }

        [Fact]
        public void PrivateInstallation_Get_DefaultStepIsOne()
        {
            var serviceMock = new Mock<IValaisServices>();
            var controller = CreateControllerWithSession(serviceMock.Object);

            var result = controller.PrivateInstallation();

            var viewResult = Assert.IsType<ViewResult>(result);
            var vm = Assert.IsType<PrivateInstallationVm>(viewResult.Model);

            Assert.Equal(1, vm.Step);
        }

        [Fact]
        public async Task PrivateInstallation_Post_Step1_Invalid_StaysOnStep1()
        {
            var serviceMock = new Mock<IValaisServices>();
            var controller = CreateControllerWithSession(serviceMock.Object);

            var vm = new PrivateInstallationVm
            {
                Step = 1,
                Rue = "",
                No = null,
                NPA = 50,
                Localite = ""
            };

            var result = await controller.PrivateInstallation(vm, null);

            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedVm = Assert.IsType<PrivateInstallationVm>(viewResult.Model);

            Assert.Equal(1, returnedVm.Step);
            Assert.False(controller.ModelState.IsValid);
            serviceMock.Verify(s => s.CreateInstallationAsync(It.IsAny<PrivateInstallationVm>()), Times.Never);
        }

        [Fact]
        public async Task PrivateInstallation_Post_Step1_Valid_GoesToStep2()
        {
            var serviceMock = new Mock<IValaisServices>();
            var controller = CreateControllerWithSession(serviceMock.Object);

            var vm = new PrivateInstallationVm
            {
                Step = 1,
                Rue = "Rue",
                No = 10,
                NPA = 1950,
                Localite = "Sion"
            };

            var result = await controller.PrivateInstallation(vm, null);

            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedVm = Assert.IsType<PrivateInstallationVm>(viewResult.Model);

            Assert.Equal(2, returnedVm.Step);
            serviceMock.Verify(s => s.CreateInstallationAsync(It.IsAny<PrivateInstallationVm>()), Times.Never);
        }

        [Fact]
        public async Task PrivateInstallation_Post_Step2_GoesToStep3()
        {
            var serviceMock = new Mock<IValaisServices>();
            var controller = CreateControllerWithSession(serviceMock.Object);

            var vm = new PrivateInstallationVm
            {
                Step = 2,
                SelectedEnergyType = "PV",
                SelectedIntegrationType = "Toit",
                SelectedSolarCellType = "Mono"
            };

            var result = await controller.PrivateInstallation(vm, null);

            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedVm = Assert.IsType<PrivateInstallationVm>(viewResult.Model);

            Assert.Equal(3, returnedVm.Step);
            serviceMock.Verify(s => s.CreateInstallationAsync(It.IsAny<PrivateInstallationVm>()), Times.Never);
        }

        [Fact]
        public async Task PrivateInstallation_Post_Step3_Invalid_StaysOnStep3()
        {
            var serviceMock = new Mock<IValaisServices>();
            var controller = CreateControllerWithSession(serviceMock.Object);

            var stored = new PrivateInstallationVm
            {
                Step = 3,
                Rue = "Rue",
                No = 10,
                NPA = 1950,
                Localite = "Sion"
            };
            controller.HttpContext.Session.SetString("PrivateInstallationVm", JsonSerializer.Serialize(stored));

            var vm = new PrivateInstallationVm
            {
                Step = 3,
                OrientationAzimut = null,
                ToitureInclinaison = 120
            };

            var result = await controller.PrivateInstallation(vm, null);

            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedVm = Assert.IsType<PrivateInstallationVm>(viewResult.Model);

            Assert.Equal(3, returnedVm.Step);
            Assert.False(controller.ModelState.IsValid);
            serviceMock.Verify(s => s.CreateInstallationAsync(It.IsAny<PrivateInstallationVm>()), Times.Never);
        }

        [Fact]
        public async Task PrivateInstallation_Post_Step3_Valid_GoesToStep4()
        {
            var serviceMock = new Mock<IValaisServices>();
            var controller = CreateControllerWithSession(serviceMock.Object);

            var stored = new PrivateInstallationVm
            {
                Step = 3,
                Rue = "Rue",
                No = 10,
                NPA = 1950,
                Localite = "Sion"
            };
            controller.HttpContext.Session.SetString("PrivateInstallationVm", JsonSerializer.Serialize(stored));

            var vm = new PrivateInstallationVm
            {
                Step = 3,
                OrientationAzimut = 180,
                ToitureInclinaison = 30
            };

            var result = await controller.PrivateInstallation(vm, null);

            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedVm = Assert.IsType<PrivateInstallationVm>(viewResult.Model);

            Assert.Equal(4, returnedVm.Step);
            Assert.True(controller.ModelState.IsValid);
        }

        [Fact]
        public async Task PrivateInstallation_Post_Step4_Invalid_StaysOnStep4()
        {
            var serviceMock = new Mock<IValaisServices>();
            var controller = CreateControllerWithSession(serviceMock.Object);

            var stored = new PrivateInstallationVm
            {
                Step = 4,
                Rue = "Rue",
                No = 10,
                NPA = 1950,
                Localite = "Sion"
            };
            controller.HttpContext.Session.SetString("PrivateInstallationVm", JsonSerializer.Serialize(stored));

            var vm = new PrivateInstallationVm
            {
                Step = 4,
                Longueur = 0,
                Largeur = 5
            };

            var result = await controller.PrivateInstallation(vm, null);

            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedVm = Assert.IsType<PrivateInstallationVm>(viewResult.Model);

            Assert.Equal(4, returnedVm.Step);
            Assert.False(controller.ModelState.IsValid);
            serviceMock.Verify(s => s.CreateInstallationAsync(It.IsAny<PrivateInstallationVm>()), Times.Never);
        }

        [Fact]
        public async Task PrivateInstallation_Post_Step4_Previous_GoesBackToStep3()
        {
            var serviceMock = new Mock<IValaisServices>();
            var controller = CreateControllerWithSession(serviceMock.Object);

            var stored = new PrivateInstallationVm
            {
                Step = 4
            };
            controller.HttpContext.Session.SetString("PrivateInstallationVm", JsonSerializer.Serialize(stored));

            var vm = new PrivateInstallationVm
            {
                Step = 4,
                Longueur = 10,
                Largeur = 5
            };

            var result = await controller.PrivateInstallation(vm, "previous");

            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedVm = Assert.IsType<PrivateInstallationVm>(viewResult.Model);

            Assert.Equal(3, returnedVm.Step);
            serviceMock.Verify(s => s.CreateInstallationAsync(It.IsAny<PrivateInstallationVm>()), Times.Never);
        }

        [Fact]
        public async Task PrivateInstallation_Post_Step4_Valid_CallsServiceAndRedirects()
        {
            var serviceMock = new Mock<IValaisServices>();
            serviceMock.Setup(s => s.CreateInstallationAsync(It.IsAny<PrivateInstallationVm>()))
                       .ReturnsAsync(123);

            var controller = CreateControllerWithSession(serviceMock.Object);

            var vm = new PrivateInstallationVm
            {
                Step = 4,
                Longueur = 10,
                Largeur = 5,
                Rue = "Rue",
                No = 10,
                NPA = 1950,
                Localite = "Sion"
            };

            var result = await controller.PrivateInstallation(vm, null);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("PrivateInstallationConfirmation", redirect.ActionName);
            Assert.Equal(123, controller.TempData["RegistrationNumber"]);
        }

        [Fact]
        public void PrivateInstallationConfirmation_ReturnsViewAndNumber()
        {
            var serviceMock = new Mock<IValaisServices>();
            var controller = CreateControllerWithSession(serviceMock.Object);

            controller.TempData["RegistrationNumber"] = 456;

            var result = controller.PrivateInstallationConfirmation();

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal("~/Views/Home/PrivateInstallation/PrivateInstallationConfirmation.cshtml", view.ViewName);
            Assert.Equal(456, controller.ViewBag.RegistrationNumber);
        }

        [Fact]
        public void NER_ReturnsCorrectView()
        {
            var serviceMock = new Mock<IValaisServices>();
            var controller = CreateControllerWithSession(serviceMock.Object);

            var result = controller.NER();

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal("~/Views/Home/NER/NER.cshtml", view.ViewName);
        }

        [Fact]
        public void MiniHydraulique_ReturnsCorrectView()
        {
            var serviceMock = new Mock<IValaisServices>();
            var controller = CreateControllerWithSession(serviceMock.Object);

            var result = controller.MiniHydraulique();

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal("~/Views/Home/NER/Mini-hydraulique/MiniHydraulique.cshtml", view.ViewName);
        }

        [Fact]
        public void Eolien_ReturnsCorrectView()
        {
            var serviceMock = new Mock<IValaisServices>();
            var controller = CreateControllerWithSession(serviceMock.Object);

            var result = controller.Eolien();

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal("~/Views/Home/NER/Eolien/Eolien.cshtml", view.ViewName);
        }

        [Fact]
        public void Biogaz_ReturnsCorrectView()
        {
            var serviceMock = new Mock<IValaisServices>();
            var controller = CreateControllerWithSession(serviceMock.Object);

            var result = controller.Biogaz();

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal("~/Views/Home/NER/Biogaz/Biogaz.cshtml", view.ViewName);
        }

        [Fact]
        public void Privacy_ReturnsDefaultView()
        {
            var serviceMock = new Mock<IValaisServices>();
            var controller = CreateControllerWithSession(serviceMock.Object);

            var result = controller.Privacy();

            var view = Assert.IsType<ViewResult>(result);
            Assert.Null(view.ViewName);
        }

        [Fact]
        public async Task PV_ReturnsViewWithPvChartVm()
        {
            var serviceMock = new Mock<IValaisServices>();

            var fakePvChart = new ProductionChartVm
            {
                Title = "PV chart",
                Years = new List<int> { 2020, 2021 },
                KWh = new List<double> { 5, 6 }
            };

            serviceMock.Setup(s => s.GetPvChartAsync()).ReturnsAsync(fakePvChart);

            var controller = CreateControllerWithSession(serviceMock.Object);

            var result = await controller.PV();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("~/Views/Pv/Index.cshtml", viewResult.ViewName);

            var model = Assert.IsType<ProductionChartVm>(viewResult.Model);
            Assert.Same(fakePvChart, model);
        }

        [Fact]
        public async Task PvController_Index_ReturnsViewAndModel()
        {
            var serviceMock = new Mock<IValaisServices>();
            var fakePvChart = new ProductionChartVm
            {
                Title = "PV",
                Years = new List<int> { 2020 },
                KWh = new List<double> { 5 }
            };

            serviceMock.Setup(s => s.GetPvChartAsync()).ReturnsAsync(fakePvChart);

            var controller = new PvController(serviceMock.Object);

            var result = await controller.Index();

            var view = Assert.IsType<ViewResult>(result);
            Assert.Null(view.ViewName);

            var model = Assert.IsType<ProductionChartVm>(view.Model);
            Assert.Same(fakePvChart, model);
        }

        [Fact]
        public void PrivateInstallationVm_Surface_ComputesArea()
        {
            var vm = new PrivateInstallationVm
            {
                Longueur = 10,
                Largeur = 5
            };

            Assert.Equal(50, vm.Surface);
        }

        [Fact]
        public async Task ValaisServices_GetPieAsync_ReturnsCorrectTuple()
        {
            var json = "{\"labels\": [\"Hydro\"], \"values\": [100], \"year\": 2024}";

            var handler = new FakeHttpMessageHandler(json);
            var http = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost")
            };

            var svc = new ValaisServices(http);

            var (labels, values, year) = await svc.GetPieAsync();

            Assert.Single(labels);
            Assert.Single(values);
            Assert.Equal("Hydro", labels[0]);
            Assert.Equal(100, values[0]);
            Assert.Equal(2024, year);
        }

        [Fact]
        public async Task ValaisServices_GetChartAsync_MapsDtoToVm()
        {
            var json = "{\"title\":\"Global\",\"years\":[2010,2011],\"kWh\":[10.0,20.0]}";

            var handler = new FakeHttpMessageHandler(json);
            var http = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost")
            };

            var svc = new ValaisServices(http);

            var vm = await svc.GetChartAsync();

            Assert.Equal("Global", vm.Title);
            Assert.Equal(new List<int> { 2010, 2011 }, vm.Years);
            Assert.Equal(new List<double> { 10.0, 20.0 }, vm.KWh);
        }

        [Fact]
        public async Task ValaisServices_GetPvChartAsync_MapsDtoToVm()
        {
            var json = "{\"title\":\"PV\",\"years\":[2025],\"kWh\":[50.0]}";

            var handler = new FakeHttpMessageHandler(json);
            var http = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost")
            };

            var svc = new ValaisServices(http);

            var vm = await svc.GetPvChartAsync();

            Assert.Equal("PV", vm.Title);
            Assert.Single(vm.Years);
            Assert.Equal(2025, vm.Years[0]);
            Assert.Single(vm.KWh);
            Assert.Equal(50.0, vm.KWh[0]);
        }

        [Fact]
        public async Task ValaisServices_CreateInstallationAsync_PostsAndReturnsId()
        {
            var json = "123";

            var handler = new FakeHttpMessageHandler(json);
            var http = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost")
            };

            var svc = new ValaisServices(http);

            var vm = new PrivateInstallationVm
            {
                Rue = "Rue",
                No = 10,
                NPA = 1950,
                Localite = "Sion",
                Longueur = 10,
                Largeur = 5,
                OrientationAzimut = 180,
                ToitureInclinaison = 30,
                SelectedEnergyType = "PV",
                SelectedIntegrationType = "Integrée",
                SelectedSolarCellType = "Monochristallin"
            };

            var id = await svc.CreateInstallationAsync(vm);

            Assert.Equal(123, id);
        }
    }

    internal class FakeSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new();
        public string Id => Guid.NewGuid().ToString();
        public bool IsAvailable => true;
        public IEnumerable<string> Keys => _store.Keys;
        public void Clear() => _store.Clear();
        public void Remove(string key) => _store.Remove(key);
        public void Set(string key, byte[] value) => _store[key] = value;
        public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value);
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    internal class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _json;

        public FakeHttpMessageHandler(string json)
        {
            _json = json;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_json, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }
}





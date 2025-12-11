using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using MVC.Controllers;
using MVC.Models;
using MVC.Services;
using Xunit;

namespace MVCTest
{
    public class HomeControllerTests
    {
        // ---------- Helpers ----------

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

        // ---------- Index() ----------

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
                Labels: new List<string> { "Hydraulique", "PV" },
                Values: new List<double> { 80, 20 },
                Year: 2024
            );

            serviceMock.Setup(s => s.GetChartAsync())
                       .ReturnsAsync(fakeGlobalChart);
            serviceMock.Setup(s => s.GetPvChartAsync())
                       .ReturnsAsync(fakePvChart);
            serviceMock.Setup(s => s.GetPieAsync())
                       .ReturnsAsync(fakePie);

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

        // ---------- GET PrivateInstallation ----------

        [Fact]
        public void PrivateInstallation_Get_DefaultStepIsOne()
        {
            var serviceMock = new Mock<IValaisServices>();
            var controller = CreateControllerWithSession(serviceMock.Object);

            var result = controller.PrivateInstallation();

            var viewResult = Assert.IsType<ViewResult>(result);
            var vm = Assert.IsType<PrivateInstallationVm>(viewResult.Model);

            Assert.Equal(1, vm.Step);
            Assert.Equal("~/Views/Home/PrivateInstallation/PrivateInstallation.cshtml",
                         viewResult.ViewName);
        }

        // ---------- POST PrivateInstallation STEP 1 (invalid) ----------

        [Fact]
        public async Task PrivateInstallation_Post_Step1_Invalid_StaysOnStep1()
        {
            var serviceMock = new Mock<IValaisServices>();
            var controller = CreateControllerWithSession(serviceMock.Object);

            var vm = new PrivateInstallationVm
            {
                Step = 1,
                Rue = "",          // invalide
                No = null,         // invalide
                NPA = 50,          // invalide
                Localite = ""      // invalide
            };

            var result = await controller.PrivateInstallation(vm, action: null);

            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedVm = Assert.IsType<PrivateInstallationVm>(viewResult.Model);

            Assert.Equal(1, returnedVm.Step);
            Assert.False(controller.ModelState.IsValid);

            serviceMock.Verify(s => s.CreateInstallationAsync(It.IsAny<PrivateInstallationVm>()),
                               Times.Never);
        }

        // ---------- POST PrivateInstallation STEP 1 (valid) ----------

        [Fact]
        public async Task PrivateInstallation_Post_Step1_Valid_GoesToStep2()
        {
            var serviceMock = new Mock<IValaisServices>();
            var controller = CreateControllerWithSession(serviceMock.Object);

            var vm = new PrivateInstallationVm
            {
                Step = 1,
                Rue = "Rue du Test",
                No = 10,
                NPA = 1950,
                Localite = "Sion"
            };

            var result = await controller.PrivateInstallation(vm, action: null);

            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedVm = Assert.IsType<PrivateInstallationVm>(viewResult.Model);

            Assert.Equal(2, returnedVm.Step);
            Assert.True(controller.ModelState.IsValid);

            serviceMock.Verify(
                s => s.CreateInstallationAsync(It.IsAny<PrivateInstallationVm>()),
                Times.Never);
        }

        // ---------- POST PrivateInstallation STEP 3 (invalid) ----------

        [Fact]
        public async Task PrivateInstallation_Post_Step3_Invalid_StaysOnStep3()
        {
            var serviceMock = new Mock<IValaisServices>();
            var controller = CreateControllerWithSession(serviceMock.Object);

            // On simule un VM déjà en session à l'étape 3
            var stored = new PrivateInstallationVm
            {
                Step = 3,
                Rue = "Rue du Test",
                No = 10,
                NPA = 1950,
                Localite = "Sion"
            };
            var json = JsonSerializer.Serialize(stored);
            controller.HttpContext.Session.SetString("PrivateInstallationVm", json);

            // VM envoyé depuis le formulaire : step 3 + valeurs invalides
            var vm = new PrivateInstallationVm
            {
                Step = 3,
                OrientationAzimut = null,     // invalide
                ToitureInclinaison = 120      // invalide (> 90)
            };

            var result = await controller.PrivateInstallation(vm, action: null);

            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedVm = Assert.IsType<PrivateInstallationVm>(viewResult.Model);

            // ==> On reste à l'étape 3
            Assert.Equal(3, returnedVm.Step);
            Assert.False(controller.ModelState.IsValid);

            serviceMock.Verify(
                s => s.CreateInstallationAsync(It.IsAny<PrivateInstallationVm>()),
                Times.Never);
        }

        // ---------- POST PrivateInstallation STEP 4 (valid, crée installation) ----------

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
                Rue = "Rue du Test",
                No = 10,
                NPA = 1950,
                Localite = "Sion"
            };

            var result = await controller.PrivateInstallation(vm, action: null);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("PrivateInstallationConfirmation", redirect.ActionName);

            serviceMock.Verify(
                s => s.CreateInstallationAsync(It.IsAny<PrivateInstallationVm>()),
                Times.Once);

            Assert.Equal(123, controller.TempData["RegistrationNumber"]);
        }

        // ---------- PV() ----------

        [Fact]
        public async Task PV_ReturnsViewWithPvChartVm()
        {
            // ARRANGE
            var serviceMock = new Mock<IValaisServices>();

            var fakePvChart = new ProductionChartVm
            {
                Title = "PV chart",
                Years = new List<int> { 2020, 2021 },
                KWh = new List<double> { 5, 6 }
            };

            serviceMock.Setup(s => s.GetPvChartAsync())
                       .ReturnsAsync(fakePvChart);

            var controller = CreateControllerWithSession(serviceMock.Object);

            // ACT
            var result = await controller.PV();

            // ASSERT
            var viewResult = Assert.IsType<ViewResult>(result);

          
            Assert.Equal("~/Views/Pv/Index.cshtml", viewResult.ViewName);

            var model = Assert.IsType<ProductionChartVm>(viewResult.Model);
            Assert.Same(fakePvChart, model);

            serviceMock.Verify(s => s.GetPvChartAsync(), Times.Once);
        }


        // ---------- PvController.Index() ----------

        [Fact]
        public async Task PvController_Index_ReturnsViewAndModel()
        {
            var serviceMock = new Mock<IValaisServices>();
            var fakePvChart = new ProductionChartVm
            {
                Title = "PV chart",
                Years = new List<int> { 2020 },
                KWh = new List<double> { 5 }
            };

            serviceMock.Setup(s => s.GetPvChartAsync())
                       .ReturnsAsync(fakePvChart);

            var controller = new PvController(serviceMock.Object);

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            // Là aussi : pas de View("...") explicite, donc ViewName == null
            Assert.True(string.IsNullOrEmpty(viewResult.ViewName));

            var model = Assert.IsType<ProductionChartVm>(viewResult.Model);
            Assert.Same(fakePvChart, model);

            serviceMock.Verify(s => s.GetPvChartAsync(), Times.Once);
        }

        // ---------- PrivateInstallationVm.Surface ----------

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

        // ---------- ValaisServices.GetPieAsync() ----------

        [Fact]
        public async Task ValaisServices_GetPieAsync_ReturnsCorrectTuple()
        {
            // JSON simulé par un HttpMessageHandler fake
            var json = "{\"labels\": [\"Hydro\"], \"values\": [100], \"year\": 2024}";

            var handler = new FakeHttpMessageHandler(json);
            var http = new HttpClient(handler)
            {
                // BaseAddress obligatoire pour les URLs relatives (GetFromJsonAsync)
                BaseAddress = new Uri("http://localhost")
            };

            var svc = new ValaisServices(http);

            var (labels, values, year) = await svc.GetPieAsync();

            Assert.Single(labels);
            Assert.Equal("Hydro", labels[0]);

            Assert.Single(values);
            Assert.Equal(100, values[0]);

            Assert.Equal(2024, year);
        }
    }

    // ---------- Fake Session ----------

    internal class FakeSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new();

        public string Id { get; } = Guid.NewGuid().ToString();
        public bool IsAvailable => true;
        public IEnumerable<string> Keys => _store.Keys;

        public void Clear() => _store.Clear();

        public Task CommitAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task LoadAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public void Remove(string key) => _store.Remove(key);

        public void Set(string key, byte[] value) => _store[key] = value;

        public bool TryGetValue(string key, out byte[] value)
            => _store.TryGetValue(key, out value);
    }

    // ---------- Fake HttpMessageHandler pour ValaisServices ----------

    internal class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _json;

        public FakeHttpMessageHandler(string json)
        {
            _json = json;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_json, Encoding.UTF8, "application/json")
            };

            return Task.FromResult(response);
        }
    }
}




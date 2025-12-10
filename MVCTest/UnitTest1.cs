using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MVC.Controllers;
using MVC.Models;
using MVC.Services;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Text;
using Xunit;
using System.Threading;
using System.Collections.Generic;

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

        [Fact]
        public void PrivateInstallation_Get_DefaultStepIsOne()
        {
         
            var serviceMock = new Mock<IValaisServices>();
            var controller = CreateControllerWithSession(serviceMock.Object);

          
            var result = controller.PrivateInstallation(); // step = 1 par défaut

          
            var viewResult = Assert.IsType<ViewResult>(result);
            var vm = Assert.IsType<PrivateInstallationVm>(viewResult.Model);

            Assert.Equal(1, vm.Step); 
            Assert.Equal("~/Views/Home/PrivateInstallation/PrivateInstallation.cshtml",
                         viewResult.ViewName);
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

         
            var result = await controller.PrivateInstallation(vm, action: null);

    
            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedVm = Assert.IsType<PrivateInstallationVm>(viewResult.Model);

            Assert.Equal(1, returnedVm.Step);

            Assert.False(controller.ModelState.IsValid);

            serviceMock.Verify(s => s.CreateInstallationAsync(It.IsAny<PrivateInstallationVm>()),
                               Times.Never);
        }

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
                Times.Never
            );
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

            serviceMock.Setup(s => s.GetPvChartAsync())
                       .ReturnsAsync(fakePvChart);

            var controller = CreateControllerWithSession(serviceMock.Object);

            var result = await controller.PV();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("~/Views/Pv/Index.cshtml", viewResult.ViewName);

            var model = Assert.IsType<ProductionChartVm>(viewResult.Model);
            Assert.Same(fakePvChart, model);

            serviceMock.Verify(s => s.GetPvChartAsync(), Times.Once);
        }

    }

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
}



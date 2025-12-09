using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

using MVC.Controllers;
using MVC.Services;
using MVC.Models;

namespace MVCTest
{
    public class HomeControllerTests
    {
        [Fact]
        public async Task Index_ReturnsView_WithHomeDashboardVm_AndViewBagFilled()
        {
            var fakeGlobalChart = new ProductionChartVm();
            var fakePvChart = new ProductionChartVm();

  
            var fakeLabels = new List<string> { "Hydraulique", "PV" };
            var fakeValues = new List<double> { 80, 20 };
            var fakeYear = 2024;

            var serviceMock = new Mock<IValaisServices>();

            serviceMock
                .Setup(s => s.GetChartAsync())
                .ReturnsAsync(fakeGlobalChart);

            serviceMock
                .Setup(s => s.GetPvChartAsync())
                .ReturnsAsync(fakePvChart);

            serviceMock
                .Setup(s => s.GetPieAsync())
                .ReturnsAsync((fakeLabels, fakeValues, fakeYear));

            var controller = new HomeController(serviceMock.Object);

   
            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);

            var model = Assert.IsType<HomeDashboardVm>(viewResult.Model);
            Assert.Same(fakeGlobalChart, model.GlobalChart);
            Assert.Same(fakePvChart, model.PvChart);

            Assert.Equal(fakeLabels, controller.ViewBag.NerLabels);
            Assert.Equal(fakeValues, controller.ViewBag.NerValues);
            Assert.Equal(fakeYear, controller.ViewBag.NerYear);

            serviceMock.Verify(s => s.GetChartAsync(), Times.Once);
            serviceMock.Verify(s => s.GetPvChartAsync(), Times.Once);
            serviceMock.Verify(s => s.GetPieAsync(), Times.Once);
        }

        [Fact]
        public async Task PV_ReturnsPvIndexView_WithPvChartVm()
        {
            var fakePvChart = new ProductionChartVm();

            var serviceMock = new Mock<IValaisServices>();
            serviceMock
                .Setup(s => s.GetPvChartAsync())
                .ReturnsAsync(fakePvChart);

            var controller = new HomeController(serviceMock.Object);

            var result = await controller.PV();

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Equal("~/Views/Pv/Index.cshtml", viewResult.ViewName);

      
            var model = Assert.IsType<ProductionChartVm>(viewResult.Model);
            Assert.Same(fakePvChart, model);

            serviceMock.Verify(s => s.GetPvChartAsync(), Times.Once);
        }

        [Fact]
        public void NER_ReturnsCorrectView()
        {
            var serviceMock = new Mock<IValaisServices>();
            var controller = new HomeController(serviceMock.Object);

            var result = controller.NER();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("~/Views/Home/NER/NER.cshtml", viewResult.ViewName);
        }

        [Fact]
        public void Biogaz_ReturnsCorrectView()
        {
            var serviceMock = new Mock<IValaisServices>();
            var controller = new HomeController(serviceMock.Object);

            var result = controller.Biogaz();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("~/Views/Home/NER/Biogaz/Biogaz.cshtml", viewResult.ViewName);
        }

        [Fact]
        public void Eolien_ReturnsCorrectView()
        {
            var serviceMock = new Mock<IValaisServices>();
            var controller = new HomeController(serviceMock.Object);

            var result = controller.Eolien();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("~/Views/Home/NER/Eolien/Eolien.cshtml", viewResult.ViewName);
        }

        [Fact]
        public void MiniHydraulique_ReturnsCorrectView()
        {
            var serviceMock = new Mock<IValaisServices>();
            var controller = new HomeController(serviceMock.Object);

            var result = controller.MiniHydraulique();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("~/Views/Home/NER/Mini-hydraulique/MiniHydraulique.cshtml", viewResult.ViewName);
        }

        [Fact]
        public void Privacy_ReturnsView()
        {
            var serviceMock = new Mock<IValaisServices>();
            var controller = new HomeController(serviceMock.Object);

            var result = controller.Privacy();

            Assert.IsType<ViewResult>(result);
        }


    }
}


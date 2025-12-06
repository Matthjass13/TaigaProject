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
    public class ComputationTests
    {
        private const double Tolerance = 1e-6;
        private readonly Computation computation = new Computation();
        
        [Theory]
        [InlineData(5, 5, 25)]
        [InlineData(7, 8, 56)]
        public void ComputeSurface_Returns_CorrectSurface(double length, double width, double expectedSurface)
        {
            //Arrange & Act
            double actualSurface = computation.ComputeSurface(length, width);

            //Assert
            Assert.Equal(actualSurface, expectedSurface);
        }


        [Theory]
        [InlineData(1, 0.001)]
        [InlineData(400, 0.4)]
        public void ConvertWattIntoKiloWatt_Returns_CorrectConversion(double watt, double expectedKW)
        {
            //Arrange & Act
            double actualKW = computation.ConvertWattIntoKiloWatt(watt);

            //Assert
            Assert.InRange(actualKW, expectedKW - Tolerance, expectedKW + Tolerance);
        }


        [Theory]
        [InlineData(0, "south")]
        [InlineData(15, "south")]
        [InlineData(120, "west")]
        [InlineData(130, "west")]
        [InlineData(170, "north")]
        [InlineData(-160, "north")]
        [InlineData(-110, "east")]
        [InlineData(-70, "east")]
        public void DetermineDirection_Returns_RightDirection(double azimut, string expectedDirection)
        {
            //Arrange & Act
            string actualDirection = computation.DetermineDirection(azimut);

            //Assert
            Assert.Equal(actualDirection, expectedDirection);
        }

        [Fact]
        public void DetermineOrientationFactor_Returns_100WhenSouth()
        {
            //Arrange
            string direction = "south";

            //Act
            double actualFactor = computation.DetermineOrientationFactor(direction);

            //Assert
            Assert.True(actualFactor == 1);
        }

        [Theory]
        [InlineData("east")]
        [InlineData("west")]
        public void DetermineOrientationFactor_Returns_80WhenEastOrWest(string direction)
        {
            //Arrange & Act
            double actualFactor = computation.DetermineOrientationFactor(direction);

            //Assert
            Assert.True(actualFactor == 0.8);
        }

        [Theory]
        [InlineData("Polychristallin", 175)]
        [InlineData("Monochristallin", 250)]
        public void DetermineSpecificYield_Returns_CorrectYield(string solarCellType, double expectedYield)
        {
            //Arrange & Act
            double actualYield = computation.DetermineSpecificYield(solarCellType);

            //Assert
            Assert.Equal(actualYield, expectedYield);
        }



        [Fact]
        public void ComputeKWh_Returns_CorrectValueWithMocks()
        {
            // Arrange
            var mock = new Mock<Computation>();

            var inst = new Installation
            {
                SelectedSolarCellType = "Mono",
                Longueur = 10,
                Largeur = 5,
                OrientationAzimut = 90
            };

            mock.Setup(s => s.DetermineSpecificYield("Mono")).Returns(1500);
            mock.Setup(s => s.ComputeSurface(10, 5)).Returns(50);
            mock.Setup(s => s.DetermineDirection(90)).Returns("Est");
            mock.Setup(s => s.DetermineOrientationFactor("Est")).Returns(0.85);

            mock.CallBase = true;

            double expected = 1500 * 50 * 0.85;

            // Act
            double result = mock.Object.ComputeKWh(inst);

            // Assert
            Assert.Equal(expected, result);
        }


        [Fact]
        public void ComputeTotalKWh_Returns_ShouldSumAllComputedKWhValues()
        {
            // Arrange
            var mock = new Mock<Computation>();
            mock.CallBase = true;

            var installations = new List<Installation>
            {
                new Installation(),
                new Installation(),
                new Installation()
            };

            mock.SetupSequence(s => s.ComputeKWh(It.IsAny<Installation>()))
                .Returns(100)
                .Returns(200)
                .Returns(300);

            double expected = 100 + 200 + 300;

            // Act
            double result = mock.Object.ComputeTotalKWh(installations);

            // Assert
            Assert.Equal(expected, result);

            mock.Verify(s => s.ComputeKWh(It.IsAny<Installation>()), Times.Exactly(3));
        }


    }
}


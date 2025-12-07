using Moq;
using ClassLibrary.Models;
using WebAPI.Business;

namespace WebAPITest
{
    public class ComputationTests
    {
        private const double TOLERANCE = 1e-6;
        private readonly Computation computation = new Computation();


        // _____________ COMPUTE_SURFACE ______________

        [Theory]
        [InlineData(5, 5, 25)]
        [InlineData(7, 8, 56)]
        public void ComputeSurface_Returns_CorrectSurface(double length, double width, double expectedSurface)
        {
            //Arrange & Act
            double actualSurface = computation.ComputeSurface(length, width);

            //Assert
            Assert.Equal(expectedSurface, actualSurface);
        }

        [Theory]
        [InlineData(-1, 5)]
        [InlineData(5, -2)]
        [InlineData(-3, -3)]
        public void ComputeSurface_ShouldThrow_WhenLengthOrWidthIsNegative(double length, double width)
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => computation.ComputeSurface(length, width));
        }

        // _____________ COMPUTE_KW ______________

        [Theory]
        [InlineData(1, 0.001)]
        [InlineData(400, 0.4)]
        public void ConvertWattIntoKiloWatt_Returns_CorrectConversion(double watt, double expectedKW)
        {
            //Arrange & Act
            double actualKW = computation.ConvertWattIntoKiloWatt(watt);

            //Assert
            Assert.Equal(expectedKW, actualKW, TOLERANCE);
        }

        [Theory]
        [InlineData(-2)]
        [InlineData(-3)]
        public void ConvertWattIntoKiloWatt_ShouldThrow_WhenValueIsNegative(double watt)
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => computation.ConvertWattIntoKiloWatt(watt));
        }

        // _____________ COMPUTE_DIRECTION ______________

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
            Assert.Equal(expectedDirection, actualDirection);
        }

        [Theory]
        [InlineData(-200)]
        [InlineData(300)]
        public void DetermineDirection_ShouldThrow_WhenAbsValueBiggerThan180(double azimut)
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => computation.DetermineDirection(azimut));
        }

        // _____________ COMPUTE_ORIENTATION_FACTOR ______________

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
        [InlineData("EaSt")]
        [InlineData("west")]
        public void DetermineOrientationFactor_Returns_80WhenEastOrWest(string direction)
        {
            //Arrange & Act
            double actualFactor = computation.DetermineOrientationFactor(direction);

            //Assert
            Assert.True(actualFactor == 0.8);
        }

        [Theory]
        [InlineData("toto")]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("northeast")]
        public void DetermineOrientationFactor_ShouldThrow_WhenOrientationIsInvalid(string orientation)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                computation.DetermineOrientationFactor(orientation));
        }

        // _____________ COMPUTE_YIELD ______________

        [Theory]
        [InlineData("Polychristallin", 175)]
        [InlineData("Monochristallin", 250)]
        public void DetermineSpecificYield_Returns_CorrectYield(string solarCellType, double expectedYield)
        {
            //Arrange & Act
            double actualYield = computation.DetermineSpecificYield(solarCellType);

            //Assert
            Assert.Equal(expectedYield, actualYield);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("mono")]
        [InlineData("poly")]
        [InlineData("Monocristallin")]
        [InlineData("Pollychristallin")]
        [InlineData("Unknown")]
        public void DetermineSpecificYield_ShouldThrow_WhenSolarCellTypeIsInvalid(string solarCellType)
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                computation.DetermineSpecificYield(solarCellType));
        }

        // _____________ COMPUTE_KW ______________

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

        // _____________ COMPUTE_TOTAL_KW ______________

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


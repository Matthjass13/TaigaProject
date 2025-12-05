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
        private readonly Computation vb = new Computation();


        /*
        [Theory]
        [InlineData(0, 0)]
        [InlineData(25, 5500)]
        public void CalculateInstalledPowerWatts_ShouldReturnSurfaceTimesWpPerM2(double surface, double expectedWatts)
        {
            double surface = 25.0;
            double expectedWatts = 25.0 * 220; // 25 * 220 = 5500
            double actual = vb.CalculateInstalledPowerWatts(surface);
            Assert.InRange(actual, expectedWatts - Tolerance, expectedWatts + Tolerance);
        }*/





        /*
        [Theory]
        [InlineData(0, 0)]
        [InlineData(25, 5500)]
        public void CalculateInstalledPowerWatts_ShouldReturnSurfaceTimesWpPerM2(double surface, double expectedWatts)
        {
            double surface = 25.0;
            double expectedWatts = 25.0 * 220; // 25 * 220 = 5500
            double actual = vb.CalculateInstalledPowerWatts(surface);
            Assert.InRange(actual, expectedWatts - Tolerance, expectedWatts + Tolerance);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1000, 1)]
        [InlineData(5500, 5.5)]
        [InlineData(12345, 12.345)]
        public void ConvertWattsToKw_ShouldDivideBy1000(double watts, double expectedKw)
        {
            double actual = _calc.ConvertWattsToKw(watts);
            Assert.InRange(actual, expectedKw - Tolerance, expectedKw + Tolerance);
        }

        [Fact]
        public void CalculateSpecificYieldKwhPerKwp_WithReference40m2Mono_ShouldReturnExpected()
        {
            double refProductionKwh = 10000.0; // mono reference for 40m2
            double expectedSpecificYield = 10000.0 / ((ProductionCalculator.ReferenceSurfaceM2 * ProductionCalculator.DefaultWpPerM2) / 1000.0);
            double actual = vb.CalculateSpecificYieldKwhPerKwp(refProductionKwh);
            Assert.InRange(actual, expectedSpecificYield - Tolerance, expectedSpecificYield + Tolerance);
        }

        [Fact]
        public void CalculateProductionBeforeOrientation_ShouldMultiplyKwpBySpecificYield()
        {
            double kWp = 5.5;
            // compute specific yield as above
            double specificYield = vb.CalculateSpecificYieldKwhPerKwp(10000.0);
            double expected = kWp * specificYield; // should be 6250 for our numbers
            double actual = vb.CalculateProductionBeforeOrientation(kWp, specificYield);
            Assert.InRange(actual, expected - Tolerance, expected + Tolerance);
        }

        [Fact]
        public void GetOrientationFactor_45deg_ShouldReturn0Point9(bool, double expectedFactor)
        {
            double angle = 45.0;
            double expected = 0.9;
            double actual = vb.GetOrientationFactor(angle);
            Assert.Equal(actual, expected - Tolerance, expected + Tolerance);
        }

        [Fact]
        public void CalculateAnnualProduction_FullFlowExample_ShouldReturnExpectedAnnualKwh()
        {
            double surface = 25.0;
            bool isMono = true;
            double orientationDeg = 45.0;
            double refProductionFor40m2Mono = 10000.0;

            double expected = 5625.0; // precomputed: 25m2 mono at 45deg -> 5625 kWh (see discussion)
            double actual = vb.CalculateAnnualProduction(surface, isMono, orientationDeg, refProductionFor40m2Mono);

            // Small tolerance
            Assert.InRange(actual, expected - 1e-6, expected + 1e-6);
        }

        */
    }
}


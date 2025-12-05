using ClassLibrary.Models;

namespace WebAPI.Business
{
    public interface IComputation
    {
        public double ComputeSurface(double length, double width);
        public double DetermineSpecificYield(string solarCellType);
        public double ConvertWattIntoKiloWatt(double watt);
        public string DetermineDirection(double azimut);
        public double DetermineOrientationFactor(string orientation);
        public double ComputeKWh(Installation inst);
        public double ComputeTotalKWh(List<Installation> insts);
    
    }
}

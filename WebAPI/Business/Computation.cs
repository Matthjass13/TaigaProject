using ClassLibrary.Models;

namespace WebAPI.Business
{
    public class Computation : IComputation
    {

        public const double ReferenceSurfaceM2 = 40f;

        public virtual double ComputeSurface(double length, double width)
        {
            double resultat = length * width;
            return resultat;
        }

        public virtual double ConvertWattIntoKiloWatt(double watt)
        {
            double resultat = watt / 1000;
            return resultat;
        }

        public virtual string DetermineDirection(double azimut)
        {
            if (azimut > -45 && azimut <= 45)
                return "south";
            if (azimut > 45 && azimut <= 135)
                return "west";
            if (azimut > -135 && azimut <= 45)
                return "east";
            return "north";
        }

        public virtual double DetermineOrientationFactor(string orientation)
        {
            switch(orientation)
            {
                case "south":
                    return 1;
                case "east":
                case "west":
                    return 0.8;
                default:
                    return 0.6;
            }
        }

        public virtual double DetermineSpecificYield(string solarCellType)
        {
            if(solarCellType == "Monochristallin")
                return 10000 / ReferenceSurfaceM2;
            else
                return 7000 / ReferenceSurfaceM2;
        } 

        public virtual double ComputeKWh(Installation inst)
        {
            string solarCellType = inst.SelectedSolarCellType ?? string.Empty;
            double specificYield = DetermineSpecificYield(solarCellType);

            double length = inst.Longueur ?? 0;
            double width = inst.Largeur ?? 0;
            double surface = ComputeSurface(length, width);

            double azimut = inst.OrientationAzimut ?? 0;
            string direction = DetermineDirection(azimut);
            double orientationFactor = DetermineOrientationFactor(direction);

            double result = specificYield * surface * orientationFactor;
            return result;
        }


        public virtual double ComputeTotalKWh(List<Installation> insts)
        {
            double prod2025KWh = 0;
            foreach (var inst in insts)
                prod2025KWh += ComputeKWh(inst);
            return prod2025KWh;
        }

    }
}

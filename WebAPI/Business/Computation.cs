using ClassLibrary.Models;

namespace WebAPI.Business
{
    public class Computation : IComputation
    {

        public const double DefaultWpPerM2 = 220.0;
        public const double ReferenceSurfaceM2 = 40.0;

        public double ComputeKWh(Installation inst)
        {
            throw new NotImplementedException();
        }

        public double ComputeSurface(double length, double width)
        {
            throw new NotImplementedException();
        }

        public double ComputeTotalKWh(List<Installation> insts)
        {
            throw new NotImplementedException();
        }

        public double ConvertWattIntoKiloWatt(double watt)
        {
            throw new NotImplementedException();
        }

        public string DetermineDirection(double azimut)
        {
            throw new NotImplementedException();
        }

        public double DetermineOrientationFactor(string orientation)
        {
            throw new NotImplementedException();
        }

        public double DetermineSpecificYield(string solarCellType)
        {
            throw new NotImplementedException();
        }


        /*

        // 1) puissance installée en W (surface * Wp/m2)
        public double CalculateInstalledPowerWatts(double surfaceM2, double wpPerM2 = DefaultWpPerM2)
        {
            if (surfaceM2 < 0)
                throw new ArgumentException("surface must be non-negative", nameof(surfaceM2));
            return surfaceM2 * wpPerM2;
        }

        public double ConvertWattsToKw(double watts)
        {
            return watts / 1000.0;
        }

        // 3) calcul du rendement spécifique (kWh / kWp / an) à partir d'une référence
        //    refProductionKwh : production observée pour refSurfaceM2
        //    refSurfaceM2 : surface de référence (ex 40)
        //    wpPerM2 : Wp par m2 (ex 220)
        public double CalculateSpecificYieldKwhPerKwp(double refProductionKwh, double refSurfaceM2 = ReferenceSurfaceM2, double wpPerM2 = DefaultWpPerM2)
        {
            if (refSurfaceM2 <= 0) throw new ArgumentException("refSurfaceM2 must be > 0", nameof(refSurfaceM2));
            // puissance crête de la surface de référence en kWp
            double refKwp = (refSurfaceM2 * wpPerM2) / 1000.0;
            if (refKwp <= 0) throw new InvalidOperationException("reference kWp must be > 0");
            return refProductionKwh / refKwp;
        }

        // 4) production annuelle (kWh) avant correction orientation = kWp * specificYield
        public double CalculateProductionBeforeOrientation(double kWp, double specificYieldKwhPerKwp)
        {
            return kWp * specificYieldKwhPerKwp;
        }

        // 5) facteur d'orientation (linéaire entre 0° -> 1.0 et 90° -> 0.8)
        //    orientationDeg: angle en degrés, 0° = sud (meilleure production). On normalise en [-180,180].
        public double GetOrientationFactor(double orientationDeg)
        {
            // normaliser angle vers [-180, +180]
            double a = ((orientationDeg + 180.0) % 360.0 + 360.0) % 360.0 - 180.0;
            double angleAbs = Math.Abs(a);
            // distance minimale à 0° en 0..180 ; on ne prend que 0..90 pour dégradation
            double delta = Math.Min(angleAbs, 90.0);
            // interpolation linéaire : 0° -> 1.0 ; 90° -> 0.8
            return 1.0 - 0.2 * (delta / 90.0);




        }

        // 6) méthode orchestratrice (utilise les étapes précédentes)
        //    refProductionFor40m2 : 10000 for mono, 7000 for poly
        public double CalculateAnnualProduction(
            double surfaceM2,
            bool isMonocrystalline,
            double orientationDeg,
            double refProductionFor40m2 // e.g. 10000 or 7000 depending on panel type
            )
        {
            // 1) installed power in W
            double powerW = CalculateInstalledPowerWatts(surfaceM2, DefaultWpPerM2);

            // 2) to kWp
            double kWp = ConvertWattsToKw(powerW);

            // 3) specific yield based on reference
            double specificYield = CalculateSpecificYieldKwhPerKwp(refProductionFor40m2, ReferenceSurfaceM2, DefaultWpPerM2);

            // 4) production before orientation
            double prodBeforeOrient = CalculateProductionBeforeOrientation(kWp, specificYield);

            // 5) orientation factor
            double orientFactor = GetOrientationFactor(orientationDeg);

            // final
            return prodBeforeOrient * orientFactor;
        }

        */
    }
}

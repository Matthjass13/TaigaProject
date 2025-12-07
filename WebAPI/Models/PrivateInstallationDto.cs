namespace WebAPI.Models
{
    public class PrivateInstallationDto
    {
        public string Rue { get; set; } = "";
        public int No { get; set; }
        public int Npa { get; set; }
        public string Localite { get; set; } = "";

        public string? SelectedEnergyType { get; set; }
        public string? SelectedSolarCellType { get; set; }
        public string? SelectedIntegrationType { get; set; }

        public double OrientationAzimut { get; set; }
        public double ToitureInclinaison { get; set; }

        public double Longueur { get; set; }
        public double Largeur { get; set; }

    }
}

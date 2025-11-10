using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClassLibrary.Models
{
    [Table("Installation")]
    public class Installation
    {
        [Key]
        public int NoRegistration { get; set; }

        public string Rue { get; set; } = null!;
        public int No { get; set; }
        public int Npa { get; set; }
        public string Localite { get; set; } = null!;

        public string? SelectedEnergyType { get; set; }
        public string? SelectedSolarCellType { get; set; }
        public string? SelectedIntegrationType { get; set; }

        public double OrientationAzimut { get; set; }
        public double ToitureInclinaison { get; set; }

        public double Longueur { get; set; }
        public double Largeur { get; set; }
        public double Surface { get; set; }
    }
}

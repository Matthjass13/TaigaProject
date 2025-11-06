using System.ComponentModel.DataAnnotations;

namespace MVC.Models
{
    public class PrivateInstallationVm
    {
        public int Step { get; set; } = 1;
        public string? SelectedEnergyType { get; set; }
        public string? SelectedSolarCellType { get; set; }

        [Required(ErrorMessage = "Veuillez entrer l'orientation du toit.")]
        [Range(0, 360, ErrorMessage = "L'orientation doit être entre 0° et 360°.")]
        public double? OrientationAzimut { get; set; }

        [Required(ErrorMessage = "Veuillez entrer l'inclinaison du toit.")]
        [Range(0, 90, ErrorMessage = "L'inclinaison doit être entre 0° et 90°.")]
        public double? ToitureInclinaison { get; set; }
    }
}


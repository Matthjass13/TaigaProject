using System.ComponentModel.DataAnnotations;

namespace MVC.Models
{
    public class PrivateInstallationVm
    {
        public int Step { get; set; } = 1;
        public string? SelectedEnergyType { get; set; }
        public string? SelectedSolarCellType { get; set; }
        public string? SelectedIntegrationType { get; set; }

        [Required(ErrorMessage = "Veuillez entrer l'orientation du toit.")]
        [Range(0, 360, ErrorMessage = "L'orientation doit être entre 0° et 360°.")]
        public double? OrientationAzimut { get; set; }

        [Required(ErrorMessage = "Veuillez entrer l'inclinaison du toit.")]
        [Range(0, 90, ErrorMessage = "L'inclinaison doit être entre 0° et 90°.")]
        public double? ToitureInclinaison { get; set; }

        [Required(ErrorMessage = "Veuillez entrer la longueur.")]
        [Range(0.1, 1000, ErrorMessage = "La longueur doit être supérieure à 0.")]
        public double? Longueur { get; set; }

        [Required(ErrorMessage = "Veuillez entrer la largeur.")]
        [Range(0.1, 1000, ErrorMessage = "La largeur doit être supérieure à 0.")]
        public double? Largeur { get; set; }

        // Calculé automatiquement
        public double? Surface => Longueur.HasValue && Largeur.HasValue
            ? Longueur.Value * Largeur.Value
            : null;


        [Required(ErrorMessage = "Veuillez entrer la rue.")]
        public string? Rue { get; set; }

        [Required(ErrorMessage = "Veuillez entrer le numéro de rue.")]
        [Range(1, 1000, ErrorMessage = "Le numéro de rue doit être supérieur à 0.")]
        public int? No { get; set; }

        [Required(ErrorMessage = "Veuillez entrer le NPA.")]
        [Range(1000, 9999, ErrorMessage = "Le NPA suisse doit avoir 4 chiffres.")]
        public int? NPA { get; set; }

        [Required(ErrorMessage = "Veuillez entrer la localité.")]
        public string? Localite { get; set; }


        public string? Direction { get; set; }

    }
}


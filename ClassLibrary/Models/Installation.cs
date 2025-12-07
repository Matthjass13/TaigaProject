using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClassLibrary.Models
{
    [Table("Installation")]
    public class Installation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int NoRegistration { get; set; }

        [Required]
        [MaxLength(255)]
        public string Rue { get; set; } = null!;
        
        public int No { get; set; }
        public int Npa { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Localite { get; set; } = null!;

        [MaxLength(50)]
        public string? SelectedEnergyType { get; set; }
        
        [MaxLength(50)]
        public string? SelectedSolarCellType { get; set; }
        
        [MaxLength(50)]
        public string? SelectedIntegrationType { get; set; }

        public double? OrientationAzimut { get; set; }
        public double? ToitureInclinaison { get; set; }

        public double? Longueur { get; set; }
        public double? Largeur { get; set; }
        public double? Surface { get; set; }

    }
}

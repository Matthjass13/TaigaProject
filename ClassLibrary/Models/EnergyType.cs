using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClassLibrary.Models
{
    [Table("EnergyType")]
    public class EnergyType
    {
        [Key]
        [Column("EnergyTypeID")]
        public int EnergyTypeID { get; set; }

        [Required]
        [Column("Name")]
        public string Name { get; set; } = string.Empty;

        [Column("Unit")]
        public string Unit { get; set; } = "GWh";

        public ICollection<YearlyProduction>? YearlyProductions { get; set; }
    }
}


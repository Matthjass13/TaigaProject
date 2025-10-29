using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClassLibrary.Models
{
    [Table("YearlyProduction")]
    public class YearlyProduction
    {
        [Key]
        [Column("ProductionID")]
        public int ProductionID { get; set; }

        [Column("YearID")]
        public int YearID { get; set; }

        [Column("EnergyTypeID")]
        public int EnergyTypeID { get; set; }

        [Column("ValueGWh")]
        public decimal ValueGWh { get; set; }

        public Yearly? Year { get; set; }
        public EnergyType? EnergyType { get; set; }
    }
}


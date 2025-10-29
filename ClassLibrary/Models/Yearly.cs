using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClassLibrary.Models
{
    [Table("Yearly")]
    public class Yearly
    {
        [Key]
        [Column("YearID")]
        public int YearID { get; set; }

        [Column("Year")]
        public short Year { get; set; }

        public ICollection<YearlyProduction>? YearlyProductions { get; set; }
    }
}


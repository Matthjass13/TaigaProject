using Microsoft.EntityFrameworkCore;
using ClassLibrary.Models;

namespace ClassLibrary.DataAccessLayer
{
    public class ValaisContext : DbContext
    {
        public ValaisContext(DbContextOptions<ValaisContext> options) : base(options) { }

        public virtual DbSet<Yearly> Yearly { get; set; }
        public virtual DbSet<EnergyType> EnergyType { get; set; }
        public virtual DbSet<YearlyProduction> YearlyProduction { get; set; }
        public virtual DbSet<Installation> Installations { get; set; }
    }
}



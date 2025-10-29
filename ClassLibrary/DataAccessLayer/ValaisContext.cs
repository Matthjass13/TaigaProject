using Microsoft.EntityFrameworkCore;
using ClassLibrary.Models; // ou ClassLibrary.Models selon l’endroit où sont tes entités

namespace ClassLibrary.DataAccessLayer
{
    public class ValaisContext : DbContext
    {
        public ValaisContext(DbContextOptions<ValaisContext> options) : base(options) { }

        public DbSet<Yearly> Yearly => Set<Yearly>();
        public DbSet<EnergyType> EnergyType => Set<EnergyType>();
        public DbSet<YearlyProduction> YearlyProduction => Set<YearlyProduction>();
    
        
    }
}


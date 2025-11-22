namespace WebAPI.Models
{
    public class ProductionPieDto
    {
        public int Year { get; set; }
        public List<string> Labels { get; set; } = new();
        public List<double> Values { get; set; } = new();
    }
}


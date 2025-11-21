namespace WebAPI.Models
{
    public class ProductionChartDto
    {
        public string Title { get; set; } = "";
        public List<int> Years { get; set; } = new();
        public List<double> KWh { get; set; } = new();
    }
}


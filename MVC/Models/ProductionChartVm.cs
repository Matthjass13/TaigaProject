namespace MVC.Models
{
    public class ProductionChartVm
    {
        public List<int> Years { get; set; } = new();
        public List<double> KWh { get; set; } = new();
        public string Title { get; set; } = "Production [kWh]";
    }
}

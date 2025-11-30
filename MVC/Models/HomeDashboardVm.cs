namespace MVC.Models
{
    public class HomeDashboardVm
    {
        public ProductionChartVm GlobalChart { get; set; } = new();
        public ProductionChartVm PvChart { get; set; } = new();
    }
}
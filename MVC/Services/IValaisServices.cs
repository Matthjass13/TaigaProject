using MVC.Models;

namespace MVC.Services
{
    public interface IValaisServices
    {
        Task<ProductionChartVm> GetChartAsync();
        Task<(List<string> Labels, List<double> Values, int Year)> GetPieAsync();
        Task<int> CreateInstallationAsync(PrivateInstallationVm vm);
    }
}


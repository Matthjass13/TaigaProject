using ClassLibrary.Models;
using WebAPI.Models;

namespace WebAPI.Business
{
    public interface IValaisBusiness
    {
        Task<ProductionChartDto> GetProductionChartAsync();
        Task<ProductionPieDto> GetProductionPieAsync();
        Task<int> CreateInstallationAsync(PrivateInstallationDto dto);
        Task<ProductionChartDto> GetPvChartAsync();
        double CalculateTotalProduction(List<Installation> insts);
    }
}

using WebAPI.Models;

namespace WebAPI.Business
{
    public interface IValaisBusiness
    {
        Task<ProductionChartDto> GetProductionChartAsync();
        Task<ProductionPieDto> GetProductionPieAsync();
        Task<int> CreateInstallationAsync(PrivateInstallationDto dto);
    }
}

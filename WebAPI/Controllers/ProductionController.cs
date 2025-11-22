using Microsoft.AspNetCore.Mvc;
using WebAPI.Business;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductionController : ControllerBase
    {
        private readonly IValaisBusiness _business;

        public ProductionController(IValaisBusiness business)
        {
            _business = business;
        }

        [HttpGet("chart")]
        public async Task<ActionResult<ProductionChartDto>> GetChart()
        {
            return Ok(await _business.GetProductionChartAsync());
        }

        [HttpGet("pie")]
        public async Task<ActionResult<ProductionPieDto>> GetPie()
        {
            return Ok(await _business.GetProductionPieAsync());
        }

        [HttpPost("installations")]
        public async Task<ActionResult<int>> CreateInstallation([FromBody] PrivateInstallationDto dto)
        {
            var registration = await _business.CreateInstallationAsync(dto);
            return Ok(registration);
        }
    }
}


using FitupProject.BLL.DTOs.Payments;
using FitupProject.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitupProject.Controllers
{
    [ApiController]
    [Route("api/conversion-rates")]
    [Authorize]
    public class ConversionRatesController : ControllerBase
    {
        private readonly IConversionRateService _svc;
        public ConversionRatesController(IConversionRateService svc) => _svc = svc;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _svc.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id) => Ok(await _svc.GetByIdAsync(id));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ConversionRateCreateDto dto)
            => Ok(await _svc.CreateAsync(dto));

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] ConversionRateUpdateDto dto)
            => Ok(await _svc.UpdateAsync(id, dto));

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _svc.DeleteAsync(id);
            return NoContent();
        }
    }
}

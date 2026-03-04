using FitupProject.BLL.DTOs.PTs;
using FitupProject.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FitupProject.Controllers
{
    [ApiController]
    [Route("api/pt")]
    public class PublicPTController : ControllerBase
    {
        private readonly IPTService _ptService;

        public PublicPTController(IPTService ptService)
        {
            _ptService = ptService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPTs([FromQuery] PTFilterRequest filter)
        {
            var pts = await _ptService.GetAllPTsAsync(filter);
            return Ok(pts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPTById(string id)
        {
            var pt = await _ptService.GetPTByIdAsync(id);
            if (pt == null) return NotFound(new { message = "Không tìm thấy PT." });
            return Ok(pt);
        }
    }
}
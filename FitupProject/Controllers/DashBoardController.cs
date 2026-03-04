using FitupProject.BLL.DTOs.DashBoard;
using FitupProject.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FitupProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashBoardController : ControllerBase
    {
        private readonly IDashBoardService _dashBoardService;
        public DashBoardController(IDashBoardService dashBoardService)
        {
            _dashBoardService = dashBoardService;
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetSummary([FromQuery] GetSummaryRequest? request)
        {
            var result = await _dashBoardService.GetSummaryAsync(request);
            return Ok(result);
        }
    }
}
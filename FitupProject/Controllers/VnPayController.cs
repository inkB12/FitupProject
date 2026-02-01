using FitupProject.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FitupProject.Controllers
{
    [ApiController]
    [Route("api/vnpay")]
    public class VnPayController : ControllerBase
    {
        private readonly ITopUpService _svc;
        public VnPayController(ITopUpService svc) => _svc = svc;

        // VNPAY redirect user về đây
        [HttpGet("return")]
        public async Task<IActionResult> Return()
        {
            var data = await _svc.HandleReturnAsync(Request.Query);
            return Ok(data);
        }

        // VNPAY server call về đây: phải trả JSON RspCode/Message :contentReference[oaicite:16]{index=16}
        [HttpGet("ipn")]
        public async Task<IActionResult> Ipn()
        {
            var (code, msg) = await _svc.HandleIpnAsync(Request.Query);
            return Ok(new { RspCode = code, Message = msg });
        }
    }
}

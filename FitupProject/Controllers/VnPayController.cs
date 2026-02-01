using FitupProject.BLL.Commons.VNPay;
using FitupProject.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FitupProject.Controllers
{
    [ApiController]
    [Route("api/vnpay")]
    public class VnPayController : ControllerBase
    {
        private readonly ITopUpService _svc;
        private readonly VnPayOptions _opt;
        public VnPayController(ITopUpService svc, IOptions<VnPayOptions> opt)
        {
            _svc = svc;
            _opt = opt.Value;
        }

        // VNPAY redirect user về
        [HttpGet("return")]
        public async Task<IActionResult> Return()
        {
            var okSig = VnPayLibrary.ValidateSignature(Request.Query, _opt.HashSecret);

            if (okSig)
            {
                await _svc.HandleIpnAsync(Request.Query);
            }

            var data = await _svc.HandleReturnAsync(Request.Query);
            return Ok(data);
        }


        // VNPAY server call về trả JSON ResCode/Message 
        [HttpGet("ipn")]
        public async Task<IActionResult> Ipn()
        {
            var (code, msg) = await _svc.HandleIpnAsync(Request.Query);
            return Ok(new { RspCode = code, Message = msg });
        }
    }
}

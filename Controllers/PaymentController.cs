using Microsoft.AspNetCore.Mvc;
using KhanhSkin_BackEnd.Dtos.Payment;
using KhanhSkin_BackEnd.Services.PaymentVNpay;

namespace KhanhSkin_BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IVnPayService _vnPayService;

        public PaymentController(IVnPayService vnPayService)
        {
            _vnPayService = vnPayService;
        }

        [HttpPost("create-payment-url")]
        public IActionResult CreatePaymentUrl([FromBody] PaymentInformationModel paymentModel)
        {
            var paymentUrl = _vnPayService.CreatePaymentUrl(paymentModel, HttpContext);
            return Ok(new { PaymentUrl = paymentUrl });
        }

        [HttpGet("payment-callback")]
        public IActionResult PaymentCallback()
        {
            // Kiểm tra SecureHash
            var isValid = _vnPayService.PaymentExecute(Request.Query).Success;

            if (!isValid)
            {
                return BadRequest(new { message = "Sai mã SecureHash hoặc giao dịch không hợp lệ." });
            }

            // Xử lý phản hồi giao dịch
            var response = _vnPayService.PaymentExecute(Request.Query);
            return Ok(response);
        }
    }
}

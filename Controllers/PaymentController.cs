using AutoWrapper.Wrappers;
using KhanhSkin_BackEnd.Dtos.Payment;
using KhanhSkin_BackEnd.Services.PaymentVNpay;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace KhanhSkin_BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        /// <summary>
        /// Tạo URL thanh toán VNPay
        /// </summary>
        [Authorize]
        [HttpPost("vnpay/create")]
        public async Task<IActionResult> CreateVNPayPayment([FromBody] PaymentWithVNPAY model)
        {
            try
            {
                var paymentUrl = _paymentService.CreatePaymentUrlVNPAY(model, HttpContext);
                return Ok(new { paymentUrl });
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Failed to create VNPay payment: {ex.Message}");
                throw new ApiException(ex.Message, ex.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while creating VNPay payment: {ex.Message}");
                throw new ApiException($"{ex.Message}");
            }
        }

        /// <summary>
        /// Xử lý callback từ VNPay
        /// </summary>
        [HttpGet("vnpay/callback")]
        public IActionResult VNPayCallback([FromQuery] IQueryCollection query)
        {
            try
            {
                var response = _paymentService.PaymentExecute(query);
                return Ok(response);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Failed to process VNPay callback: {ex.Message}");
                throw new ApiException(ex.Message, ex.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while processing VNPay callback: {ex.Message}");
                throw new ApiException($"{ex.Message}");
            }
        }
    }
}

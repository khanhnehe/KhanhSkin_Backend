using System;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using KhanhSkin_BackEnd.Dtos.Payment;
using KhanhSkin_BackEnd.Services.PaymentVNpay;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace KhanhSkin_BackEnd.Services.Orders
{
    public class PaymentService : IPaymentService
    {
        private readonly IOptions<PaymentSetting> _settings;
        private readonly IConfiguration _configuration;

        public PaymentService(IOptions<PaymentSetting> settings, IConfiguration configuration)
        {
            _settings = settings;
            _configuration = configuration;
        }

        // Tạo URL thanh toán cho VNPay
        public string CreatePaymentUrlVNPAY(PaymentWithVNPAY model, HttpContext context)
        {
            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(_configuration["TimeZoneId"]);
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
            var tick = DateTime.Now.Ticks.ToString();
            var pay = new VnPayLibrary();
            var urlCallBack = _configuration["PaymentCallBack:ReturnUrl"];

            // Thêm các tham số vào URL
            pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]);
            pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]);
            pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
            pay.AddRequestData("vnp_Amount", ((int)model.Amount * 100).ToString());
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"]);
            pay.AddRequestData("vnp_IpAddr", GetClientIp(context));
            pay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"]);
            pay.AddRequestData("vnp_OrderInfo", $"{model.OrderId}");
            pay.AddRequestData("vnp_OrderType", model.OrderType ?? "other");
            pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
            pay.AddRequestData("vnp_TxnRef", tick);

            // Tạo URL thanh toán
            var paymentUrl = pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"], _configuration["Vnpay:HashSecret"]);

            return paymentUrl;
        }

        // Thực hiện xử lý phản hồi thanh toán từ VNPay
        public PaymentResponseModel PaymentExecute(IQueryCollection collections)
        {
            var pay = new VnPayLibrary();
            var response = pay.GetFullResponseData(collections, _configuration["Vnpay:HashSecret"]);

            return response;
        }

        // Hàm hỗ trợ tính HMAC SHA256
        private string ComputeHmacSha256(string rawData, string secretKey)
        {
            using (var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                byte[] hash = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        // Lấy địa chỉ IP của client
        private string GetClientIp(HttpContext context)
        {
            var ipAddress = context.Connection.RemoteIpAddress;

            // Nếu là IPv6, chuyển sang IPv4 nếu có
            if (ipAddress != null && ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
            {
                ipAddress = Dns.GetHostEntry(ipAddress).AddressList
                    .FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
            }

            // Trả về địa chỉ IP (IPv4 hoặc IPv6), nếu không có thì trả "127.0.0.1" (localhost)
            return ipAddress?.ToString() ?? "127.0.0.1";
        }
    }
}

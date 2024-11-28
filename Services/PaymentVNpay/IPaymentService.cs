using KhanhSkin_BackEnd.Dtos.Payment;

namespace KhanhSkin_BackEnd.Services.PaymentVNpay
{
    public interface IPaymentService
    {
        /// <summary>
        /// Tạo URL thanh toán VNPay
        /// </summary>
        /// <param name="model">Thông tin thanh toán</param>
        /// <param name="context">HttpContext để lấy địa chỉ IP</param>
        /// <returns>URL thanh toán</returns>
        string CreatePaymentUrlVNPAY(PaymentWithVNPAY model, HttpContext context);

        /// <summary>
        /// Xử lý phản hồi thanh toán từ VNPay
        /// </summary>
        /// <param name="collections">Các tham số phản hồi từ VNPay</param>
        /// <returns>Kết quả thanh toán</returns>
        PaymentResponseModel PaymentExecute(IQueryCollection collections);
    }
}

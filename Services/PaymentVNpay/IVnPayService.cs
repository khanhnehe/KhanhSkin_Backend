using KhanhSkin_BackEnd.Dtos.Payment;

namespace KhanhSkin_BackEnd.Services.PaymentVNpay
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(PaymentInformationModel model, HttpContext context);
        PaymentResponseModel PaymentExecute(IQueryCollection collections);

    }
}

namespace KhanhSkin_BackEnd.Dtos.Payment
{
    public class PaymentWithVNPAY
    {
        public string OrderType { get; set; }
        public double Amount { get; set; }
        public Guid OrderId { get; set; }
        public string Name { get; set; }
    }
}
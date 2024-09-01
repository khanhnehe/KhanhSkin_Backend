namespace KhanhSkin_BackEnd.Dtos.Cart
{
    public class ApplyVoucherDto
    {
        public Guid VoucherId { get; set; }
        public string Action { get; set; } // "apply" hoặc "remove"


    }
}

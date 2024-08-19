using System.ComponentModel.DataAnnotations.Schema;

namespace KhanhSkin_BackEnd.Entities
{
    public class ProductVoucher : BaseEntity
    {
        [ForeignKey("ProductId")]
        public Guid ProductId { get; set; }
        public Product Product { get; set; }

        [ForeignKey("VoucherId")]
        public Guid VoucherId { get; set; }
        public Voucher Voucher { get; set; } // Thêm thuộc tính Voucher để ánh xạ ngược với Voucher
    }
}

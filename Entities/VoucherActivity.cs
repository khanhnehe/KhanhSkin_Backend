using System.ComponentModel.DataAnnotations.Schema;

namespace KhanhSkin_BackEnd.Entities
{
    public class VoucherActivity: BaseEntity
    {
        [ForeignKey("VoucherId")]
        public Guid VoucherId { get; set; }
        public Voucher Voucher { get; set; }

        [ForeignKey("UserId")]
        public Guid UserId { get; set; }
        public User User { get; set; }

        public DateTime UsedAt { get; set; }
        public bool IsSuccess { get; set; }
    }
}

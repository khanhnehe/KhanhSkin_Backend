using KhanhSkin_BackEnd.Share.Dtos;

namespace KhanhSkin_BackEnd.Dtos.Review
{
    public class ReviewGetRequestInputDto : BaseGetRequestInput
    {
        public Guid? ProductId { get; set; }
        public DateTime? StartDate { get; set; } // Ngày bắt đầu để lọc
        public DateTime? EndDate { get; set; }   // Ngày kết thúc để lọc
        public bool? IsApproved { get; set; }    // Nullable để lọc trạng thái phê duyệt nếu cần
    }

}

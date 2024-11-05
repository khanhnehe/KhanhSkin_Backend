using KhanhSkin_BackEnd.Share.Dtos;

namespace KhanhSkin_BackEnd.Dtos.Review
{
    public class CreateReviewDto: BaseDto
    {
        public Guid ProductId { get; set; }
        public Guid? VariantId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime ReviewDate { get; set; }
        public bool IsApproved { get; set; }
        public Guid? OrderId { get; set; }

    }
}

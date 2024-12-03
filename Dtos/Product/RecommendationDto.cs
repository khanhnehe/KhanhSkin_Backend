

using KhanhSkin_BackEnd.Share.Dtos;
using System.Text.Json.Serialization;


namespace KhanhSkin_BackEnd.Dtos.Product
{
    public class RecommendationDto : BaseDto
    {
        public string ProductName { get; set; }
        public Guid Id { get; set; }
        public int Purchases { get; set; }
    }


}




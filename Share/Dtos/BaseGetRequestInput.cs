namespace KhanhSkin_BackEnd.Share.Dtos
{
    public class BaseGetRequestInput
    {
        public string? FreeTextSearch { get; set; }
        public int? PageIndex { get; set; }
        public int? PageSize { get; set; }
        public string? Sort { get; set; }
    }
}

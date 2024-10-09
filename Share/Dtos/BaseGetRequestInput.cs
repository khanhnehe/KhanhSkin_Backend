namespace KhanhSkin_BackEnd.Share.Dtos
{
    public class BaseGetRequestInput
    {
        /// <summary>
        /// Tìm kiếm văn bản tự do, ví dụ: tên sản phẩm, tên danh mục
        /// </summary>
        public string? FreeTextSearch { get; set; }

        /// <summary>
        /// Chỉ số của trang hiện tại. Trang đầu tiên thường là 1.
        /// </summary>
        public int PageIndex { get; set; } = 1; // Giá trị mặc định là 1

        /// <summary>
        /// Kích thước trang, số lượng bản ghi trên mỗi trang
        /// </summary>
        public int PageSize { get; set; } = 10; // Giá trị mặc định là 10

        /// <summary>
        /// Tên trường dữ liệu để sắp xếp, ví dụ: "Price", "CreatedDate", "Purchases"
        /// </summary>
        public string? SortBy { get; set; }

        /// <summary>
        /// Hướng sắp xếp: true = Ascending (tăng dần), false = Descending (giảm dần)
        /// </summary>
        public bool IsAscending { get; set; } = true; // Mặc định là sắp xếp tăng dần

        /// <summary>
        /// Hàm xác định điều kiện hợp lệ cho một yêu cầu (có thể override ở các lớp con)
        /// </summary>
        public virtual bool IsValid()
        {
            return PageIndex > 0 && PageSize > 0; // Kiểm tra điều kiện hợp lệ
        }
    }
}

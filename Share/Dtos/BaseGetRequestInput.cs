namespace KhanhSkin_BackEnd.Share.Dtos
{
    // Định nghĩa một class có tên là BaseGetRequestInput
    public class BaseGetRequestInput
    {
        // Thuộc tính FreeTextSearch có kiểu dữ liệu là string và có thể chứa giá trị null.
        // Dùng để chứa văn bản tìm kiếm tự do, ví dụ như tìm kiếm sản phẩm theo tên.
        public string? FreeTextSearch { get; set; }

        // Thuộc tính PageIndex có kiểu dữ liệu là int và có thể chứa giá trị null.
        // Dùng để xác định chỉ số của trang hiện tại mà người dùng muốn lấy dữ liệu.
        // Thông thường, trang đầu tiên có chỉ số là 0 hoặc 1 tùy vào cách bạn định nghĩa.
        public int? PageIndex { get; set; }

        // Thuộc tính PageSize có kiểu dữ liệu là int và có thể chứa giá trị null.
        // Dùng để xác định số lượng bản ghi trên mỗi trang mà người dùng muốn lấy.
        public int? PageSize { get; set; }

        // Thuộc tính Sort có kiểu dữ liệu là string và có thể chứa giá trị null.
        // Dùng để xác định cách thức sắp xếp dữ liệu, ví dụ như "name_asc" hoặc "price_desc".
        public string? Sort { get; set; }
    }
}

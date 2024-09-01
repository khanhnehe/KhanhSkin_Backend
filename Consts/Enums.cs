namespace KhanhSkin_BackEnd.Consts
{
    public class Enums
    {
        public enum Role
        {
            User = 1,
            Admin = 2
        }

        public enum DiscountType
        {
            AmountMoney = 1, // Giảm theo số tiền
            Percentage // Giảm %
        }

        public enum VoucherType
        {
            Global = 1, // toàn sàn
            Specific //một số sp nhất định
        }

        public enum VoucherStatus
        {
            NotApplied = 1,
            Applied  // Voucher đang được áp dụng
             // Voucher chưa được áp dụng
        }

        public enum OrderStatus
        {
            Pending = 1, // Đang chờ xử lý
            Shipping, // Đang giao hàng
            Completed, // Đã hoàn thành
            Canceled // Đã hủy
        }
    }
}

using System.ComponentModel;

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
            [Description("Đang chờ xử lý")]
            Pending = 1,

            [Description("Đang giao hàng")]
            Shipping,

            [Description("Đã hoàn thành")]
            Completed,

            [Description("Đã hủy")]
            Canceled
        }

        public enum ShippingMethod
        {
            [Description("Giao hàng nhanh")]
            FasfDelivery = 1, 

            [Description(" Giao hàng tiết kiệm")]
            EconomyDelivery 
        }


        public enum PaymentMethod
        {
            [Description("Thanh toán khi nhận hàng")]
            Receive = 1,

            [Description("Thanh toán VNpay")]
            Vnpay
        }

        public enum ActionType
        {
            [Description("Nhập")]
            Import = 1,

            [Description("Xuất")]
            Export
        }
    }
}

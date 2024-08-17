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
    }
}

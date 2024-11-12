namespace KhanhSkin_BackEnd.Entities
{
    public class Supplier : BaseEntity
    {
        public string SupplierName { get; set; }                 // Tên nhà cung cấp
        public string? ContactDetails { get; set; }       // Thông tin liên hệ
        public string AddresSuppliers { get; set; }              // Địa chỉ nhà cung cấp
        public string EmailSupplier { get; set; }                // Email nhà cung cấp
        public string PhoneSupplier { get; set; }                // Số điện thoại của nhà cung cấp

    }
}

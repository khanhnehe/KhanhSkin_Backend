using KhanhSkin_BackEnd.Share.Dtos;

namespace KhanhSkin_BackEnd.Dtos.Supplier
{
    public class SupplierDto: BaseDto
    {
        public string SupplierName { get; set; }               
        public string? ContactDetails { get; set; }     
        public string AddresSuppliers { get; set; }             
        public string EmailSupplier { get; set; }               
        public string PhoneSupplier { get; set; }
    }
}

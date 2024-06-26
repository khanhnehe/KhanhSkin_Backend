namespace KhanhSkin_BackEnd.Entities
{
    public class BaseEntity : BaseEntity<Guid>
    {
       
    }

    public class BaseEntity<TPrimaryKey> where TPrimaryKey : struct
    {
        public TPrimaryKey Id { get; set; }
        //public bool IsDeleted { get; set; }
    }
}

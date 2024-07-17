using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using KhanhSkin_BackEnd.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace KhanhSkin_BackEnd.Repositories
{
    public interface IRepository<TEntity, TPrimaryKey> where TEntity : class
    {
        AppDbContext Context { get; }
        // Trả về đối tượng DbContext hiện tại, cho phép truy cập trực tiếp đến các cơ chế của Entity Framework Core.

        DbSet<TEntity> Table { get; }
        // Trả về một DbSet cho loại TEntity, đại diện cho bảng hoặc tập hợp trong cơ sở dữ liệu.

        IQueryable<TEntity> AsQueryable();
        // Trả về một IQueryable cho TEntity, cho phép xây dựng các truy vấn LINQ trên DbSet.

        Task<TEntity> CreateAsync(TEntity entity);
        // Thêm một thực thể vào cơ sở dữ liệu và lưu thay đổi một cách bất đồng bộ.

        Task<bool> CreateListAsync(List<TEntity> entities);
        // Thêm một danh sách các thực thể vào cơ sở dữ liệu và lưu thay đổi một cách bất đồng bộ, trả về kết quả thành công hoặc thất bại.

        Task<TEntity> DeleteAsync(TPrimaryKey id);


        Task<TEntity> DeleteByEntityAsync(TEntity entity);


        Task<TEntity> FirstOrDefault(Expression<Func<TEntity, bool>> predicate);
        // Tìm thực thể đầu tiên phù hợp với điều kiện chỉ định hoặc trả về null nếu không tìm thấy.

        Task<List<TEntity>> GetAllListAsync();
        // Lấy tất cả các thực thể trong bảng dưới dạng danh sách.

        Task<TEntity> GetAsync(TPrimaryKey id);
        // Lấy một thực thể dựa trên khóa chính.

       
        Task<TEntity> UpdateAsync(TEntity entity);
        // Cập nhật một thực thể và lưu thay đổi.

        Task<bool> UpdateListAsync(List<TEntity> entities);
        // Cập nhật một danh sách các thực thể và lưu thay đổi một cách bất đồng bộ.

        Task SaveChangesAsync();
        // Lưu tất cả các thay đổi hiện có trong context đến cơ sở dữ liệu một cách bất đồng bộ.


        // Thêm phương thức BeginTransactionAsync vào interface
        Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel);
        IQueryable<TEntity> AsQueryableIncluding(params Expression<Func<TEntity, object>>[] propertySelectors);
        // Tạo một IQueryable cho TEntity và bao gồm các thuộc tính được chỉ định để tải sẵn (eager loading).
    }

    // Interface cụ thể cho các repository với TEntity và khóa chính là Guid
    public interface IRepository<TEntity> : IRepository<TEntity, Guid> where TEntity : class
    {
    }
}

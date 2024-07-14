using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using KhanhSkin_BackEnd.Entities;

namespace KhanhSkin_BackEnd.Repositories
{
    // Lớp Repository cho phép thao tác với các thực thể trong cơ sở dữ liệu
    public class Repository<TEntity, TPrimaryKey> : IRepository<TEntity, TPrimaryKey>
        where TEntity : BaseEntity<TPrimaryKey> // TEntity phải kế thừa từ BaseEntity có khóa chính TPrimaryKey
        where TPrimaryKey : struct // TPrimaryKey phải là kiểu dữ liệu cấu trúc (ví dụ như int, Guid)
    {
        private readonly AppDbContext _context; // Biến thể hiện cho DbContext, sử dụng để tương tác với cơ sở dữ liệu

        // Constructor nhận vào một thể hiện của AppDbContext
        public Repository(AppDbContext context)
        {
            _context = context;
        }

        public AppDbContext Context => _context;  // Cung cấp quyền truy cập vào DbContext

        public DbSet<TEntity> Table => _context.Set<TEntity>(); // Truy cập đến bảng tương ứng với TEntity trong cơ sở dữ liệu

        // Trả về một IQueryable cho TEntity, cho phép xây dựng các truy vấn LINQ trên DbSet
        public IQueryable<TEntity> AsQueryable() => Table.AsQueryable();

        // Thêm một thực thể vào cơ sở dữ liệu và lưu thay đổi một cách bất đồng bộ
        public async Task<TEntity> CreateAsync(TEntity entity)
        {
            await Table.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        // Thêm một danh sách các thực thể vào cơ sở dữ liệu và lưu thay đổi một cách bất đồng bộ
        public async Task<bool> CreateListAsync(List<TEntity> entities)
        {
            await Table.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
            return true;
        }

        // Xóa một thực thể dựa trên khóa chính và lưu thay đổi
        public async Task<TEntity> DeleteAsync(TPrimaryKey id)
        {
            var entity = await Table.FindAsync(id);
            if (entity == null)
            {
                return null;
            }
            Table.Remove(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        // Xóa một thực thể cụ thể và lưu thay đổi
        public async Task<TEntity> DeleteByEntityAsync(TEntity entity)
        {
            Table.Remove(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        // Lấy tất cả các thực thể trong bảng dưới dạng danh sách
        public async Task<List<TEntity>> GetAllListAsync()
        {
            return await Table.ToListAsync();
        }

        // Lấy một thực thể dựa trên khóa chính
        public async Task<TEntity> GetAsync(TPrimaryKey id)
        {
            return await Table.FindAsync(id);
        }

        // Cập nhật một thực thể và lưu thay đổi
        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            Table.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        // Cập nhật một danh sách các thực thể và lưu thay đổi một cách bất đồng bộ
        public async Task<bool> UpdateListAsync(List<TEntity> entities)
        {
            Table.UpdateRange(entities);
            await _context.SaveChangesAsync();
            return true;
        }

        // Lưu tất cả các thay đổi hiện có trong context đến cơ sở dữ liệu một cách bất đồng bộ
        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }

        // Tạo một IQueryable cho TEntity và bao gồm các thuộc tính được chỉ định để tải sẵn (eager loading)
        public IQueryable<TEntity> AsQueryableIncluding(params Expression<Func<TEntity, object>>[] propertySelectors)
        {
            IQueryable<TEntity> query = Table;
            foreach (var propertySelector in propertySelectors)
            {
                query = query.Include(propertySelector);
            }
            return query;
        }

        // Tìm thực thể đầu tiên phù hợp với điều kiện chỉ định hoặc trả về null nếu không tìm thấy
        public async Task<TEntity?> FirstOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            return await Table.FirstOrDefaultAsync(predicate);
        }
    }

    // Đặc biệt hóa của lớp Repository cho các thực thể có khóa chính là Guid
    public class Repository<TEntity> : Repository<TEntity, Guid>, IRepository<TEntity> where TEntity : BaseEntity<Guid>
    {
        public Repository(AppDbContext context) : base(context)
        {
        }
    }


}

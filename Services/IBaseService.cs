using KhanhSkin_BackEnd.Entities;
using KhanhSkin_BackEnd.Share.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KhanhSkin_BackEnd.Services
{
    //interface chung cho các dịch vụ cơ bản
    public interface IBaseService<TEntity, TDto, TCreateDto, TGetInput>
        where TEntity : BaseEntity // phải là một lớp
        where TDto : BaseDto
        where TCreateDto : BaseDto
        where TGetInput : BaseGetRequestInput
    {
        // tạo một thực thể mới từ DTO tạo mới
        Task<TEntity> Create(TCreateDto input);

        // xóa một thực thể dựa trên ID
        Task<TEntity> Delete(Guid id);

        // lấy danh sách thực thể dưới dạng phân trang dựa trên input
        //Task<PagedViewModel<TDto>> GetListPaged(TGetInput input);

        //cập nhật một thực thể từ DTO cập nhật
        Task<TEntity> Update(TCreateDto input);

        //lưu một thực thể, có thể dùng cho cả tạo mới và cập nhật
        Task<Guid> Save(TCreateDto input);

        //lấy một thực thể dựa trên ID
        Task<TDto> Get(Guid id);

        //lấy tất cả các thực thể dưới dạng danh sách
        Task<List<TDto>> GetAll();

        Task<PagedViewModel<TDto>> GetListPaged(TGetInput input);

    }
}

using AutoMapper;
using KhanhSkin_BackEnd.Entities;
using KhanhSkin_BackEnd.Repositories;
using KhanhSkin_BackEnd.Services.CurrentUser;
using KhanhSkin_BackEnd.Share.Dtos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KhanhSkin_BackEnd.Services
{
    public abstract class BaseService<TEntity, TDto, TCreateDto, TGetInput> : IBaseService<TEntity, TDto, TCreateDto, TGetInput>
        where TEntity : BaseEntity
        where TDto : BaseDto
        where TCreateDto : BaseDto
        where TGetInput : BaseGetRequestInput
    {
        protected readonly IMapper _mapper;
        protected readonly IRepository<TEntity> _repository;
        protected readonly ILogger<BaseService<TEntity, TDto, TCreateDto, TGetInput>> _logger;
        protected readonly ICurrentUser _currentUser;

        public BaseService(IMapper mapper,
            IRepository<TEntity> repository,
            ILogger<BaseService<TEntity, TDto, TCreateDto, TGetInput>> logger,
            ICurrentUser currentUser)
        {
            _mapper = mapper;
            _repository = repository;
            _logger = logger;
            _currentUser = currentUser;
        }

        public virtual async Task<TEntity> Create(TCreateDto input)
        {
            try
            {
                var entity = _mapper.Map<TEntity>(input);
                await _repository.CreateAsync(entity);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating entity: {EntityName}", typeof(TEntity).Name);
                throw;
            }
        }

        public virtual async Task<Guid> Save(TCreateDto input)
        {
            var entity = _mapper.Map<TEntity>(input);
            if (input.Id.HasValue)
            {
                // Update
                var existingEntity = await _repository.GetAsync(input.Id.Value);
                if (existingEntity != null)
                {
                    _mapper.Map(input, existingEntity);
                    await _repository.UpdateAsync(existingEntity);
                }
            }
            else
            {
                // Create
                await _repository.CreateAsync(entity);
            }
            return entity.Id;
        }

        public virtual async Task<TEntity> Update(TCreateDto input)
        {
            try
            {
                var entity = await _repository.GetAsync(input.Id.Value);
                if (entity != null)
                {
                    _mapper.Map(input, entity);
                    await _repository.UpdateAsync(entity);
                    return entity;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating entity: {EntityName}", typeof(TEntity).Name);
                throw;
            }
        }

        public virtual async Task<TEntity> Delete(Guid id)
        {
            try
            {
                var entity = await _repository.DeleteAsync(id);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting entity: {EntityName}", typeof(TEntity).Name);
                throw;
            }
        }

        public virtual async Task<TDto> Get(Guid id)
        {
            try
            {
                var entity = await _repository.GetAsync(id);
                return _mapper.Map<TDto>(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving entity: {EntityName}", typeof(TEntity).Name);
                throw;
            }
        }

        public virtual async Task<List<TDto>> GetAll()
        {
            try
            {
                var entities = await _repository.GetAllListAsync();
                return entities.Select(e => _mapper.Map<TDto>(e)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all entities of type: {EntityType}", typeof(TEntity).Name);
                throw;
            }
        }
    }
}
//hêm từ khóa virtual vào các phương thức của lớp cơ sở, bạn có thể ghi đè (override)
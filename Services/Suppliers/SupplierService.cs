using AutoMapper;
using AutoWrapper.Wrappers;
using KhanhSkin_BackEnd.Dtos.Product;
using KhanhSkin_BackEnd.Dtos.ProductVariant;
using KhanhSkin_BackEnd.Dtos.Review;
using KhanhSkin_BackEnd.Dtos.Supplier;
using KhanhSkin_BackEnd.Dtos.User;
using KhanhSkin_BackEnd.Entities;
using KhanhSkin_BackEnd.Repositories;
using KhanhSkin_BackEnd.Services.CurrentUser;
using KhanhSkin_BackEnd.Share.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KhanhSkin_BackEnd.Services.Suppliers
{
    public class SupplierService : BaseService<Supplier, SupplierDto, SupplierDto, SupplierGetRequestInputDto>
    {
        private readonly IConfiguration _config;
        private readonly IRepository<Supplier> _supplierRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<SupplierService> _logger;
        private readonly ICurrentUser _currentUser;

        public SupplierService(
           IConfiguration config,
           IRepository<Supplier> supplierRepository,
           IMapper mapper,
           ILogger<SupplierService> logger,
           ICurrentUser currentUser)
           : base(mapper, supplierRepository, logger, currentUser)
        {
            _config = config;
            _supplierRepository = supplierRepository;
            _mapper = mapper;
            _logger = logger;
            _currentUser = currentUser;
        }

        public override async Task<Supplier> Create(SupplierDto input)
        {
            var supplier = _mapper.Map<Supplier>(input);
            await _supplierRepository.CreateAsync(supplier);
            await _supplierRepository.SaveChangesAsync();
            return supplier;
        }

        public override async Task<Supplier> Update(Guid id, SupplierDto input)
        {
            var existingSupplier = await _supplierRepository.GetAsync(id);
            if (existingSupplier == null)
            {
                throw new ApiException($"Supplier with ID {id} not found.");
            }

            _mapper.Map(input, existingSupplier);
            await _supplierRepository.UpdateAsync(existingSupplier);
            await _supplierRepository.SaveChangesAsync();

            return existingSupplier;
        }

        public override async Task<Supplier> Delete(Guid id)
        {
            var supplier = await _supplierRepository.DeleteAsync(id);
            if (supplier == null)
            {
                throw new ApiException($"Supplier with ID {id} not found.");
            }

            await _supplierRepository.SaveChangesAsync();
            return supplier;
        }

        public override async Task<SupplierDto> Get(Guid id)
        {
            var supplier = await _supplierRepository.GetAsync(id);
            if (supplier == null)
            {
                throw new ApiException($"Supplier with ID {id} not found.");
            }

            return _mapper.Map<SupplierDto>(supplier);
        }

        public async Task<PagedViewModel<SupplierDto>> GetSupplierPage(SupplierGetRequestInputDto input)
        {
            var query = _supplierRepository.AsQueryable();

            if (!string.IsNullOrEmpty(input.FreeTextSearch))
            {
                var freeTextSearch = input.FreeTextSearch.ToLower();
                query = query.Where(p => p.SupplierName.ToLower().Contains(freeTextSearch));
            }

        

            var totalCount = await query.CountAsync();

            query = query.Skip((input.PageIndex - 1) * input.PageSize)
                         .Take(input.PageSize);

            var supplierList = await query.ToListAsync();
            var supplierDtoList = _mapper.Map<List<SupplierDto>>(supplierList);

            return new PagedViewModel<SupplierDto>
            {
                Items = supplierDtoList,
                TotalRecord = totalCount
            };
        }




    }
}

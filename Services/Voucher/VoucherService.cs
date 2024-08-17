﻿using AutoMapper;
using AutoWrapper.Wrappers;
using KhanhSkin_BackEnd.Dtos.Voucher;
using KhanhSkin_BackEnd.Entities;
using KhanhSkin_BackEnd.Repositories;
using KhanhSkin_BackEnd.Services.CurrentUser;
using KhanhSkin_BackEnd.Services;
using static KhanhSkin_BackEnd.Consts.Enums;
using Microsoft.EntityFrameworkCore;

public class VoucherService : BaseService<KhanhSkin_BackEnd.Entities.Voucher, CreateUpdateVoucherDto, CreateUpdateVoucherDto, VoucherGetRequestInputDto>
{
    private readonly IConfiguration _config;
    private readonly IRepository<KhanhSkin_BackEnd.Entities.Voucher> _voucherRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<VoucherService> _logger;

    public VoucherService(
        IConfiguration config,
        IRepository<KhanhSkin_BackEnd.Entities.Voucher> repository,
        IRepository<Product> productRepository,
        IMapper mapper,
        ILogger<VoucherService> logger,
        ICurrentUser currentUser)
        : base(mapper, repository, logger, currentUser)
    {
        _config = config;
        _voucherRepository = repository;
        _productRepository = productRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<bool> CheckCodeExist(string code)
    {
        return await _voucherRepository.AsQueryable().AnyAsync(u => u.Code == code);
    }

    public override async Task<KhanhSkin_BackEnd.Entities.Voucher> Create(CreateUpdateVoucherDto input)
    {
        if (await CheckCodeExist(input.Code))
        {
            throw new ApiException("Voucher code already exists.");
        }

        var voucher = _mapper.Map<KhanhSkin_BackEnd.Entities.Voucher>(input);

        if (input.VoucherType == VoucherType.Specific && input.ApplicableProductIds != null && input.ApplicableProductIds.Any())
        {
            voucher.ApplicableProducts = new List<Product>();

            foreach (var productId in input.ApplicableProductIds)
            {
                var product = await _productRepository.GetAsync(productId);
                if (product == null)
                {
                    throw new ApiException($"Product with ID {productId} not found.");
                }
                voucher.ApplicableProducts.Add(product);
            }
        }

        await _voucherRepository.CreateAsync(voucher);
        await _voucherRepository.SaveChangesAsync();

        return voucher;
    }

    public override async Task<KhanhSkin_BackEnd.Entities.Voucher> Update(Guid id, CreateUpdateVoucherDto input)
    {
        var voucherExist = await _voucherRepository.GetAsync(id);
        if (voucherExist == null)
        {
            throw new ApiException($"Voucher not found.");
        }

        _mapper.Map(input, voucherExist);

        voucherExist.ApplicableProducts.Clear();

        if (input.VoucherType == VoucherType.Specific && input.ApplicableProductIds != null && input.ApplicableProductIds.Any())
        {
            voucherExist.ApplicableProducts = new List<Product>();

            foreach (var productId in input.ApplicableProductIds)
            {
                var product = await _productRepository.GetAsync(productId);
                if (product == null)
                {
                    throw new ApiException($"Product with ID {productId} not found.");
                }
                voucherExist.ApplicableProducts.Add(product);
            }
        }

        await _voucherRepository.UpdateAsync(voucherExist);

        return voucherExist;
    }



    public override async Task<List<CreateUpdateVoucherDto>> GetAll()
    {
        try
        {
            var vouchers = await _voucherRepository.GetAllListAsync();
            return vouchers.Select(v => _mapper.Map<CreateUpdateVoucherDto>(v)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving all vouchers.");
            throw new ApiException("Có lỗi xảy ra khi lấy danh sách tất cả voucher.");
        }
    }

    public override async Task<KhanhSkin_BackEnd.Entities.Voucher> Delete(Guid id)
    {
        var voucher = await _voucherRepository.AsQueryable().AnyAsync(v => v.Id == id);
        if (!voucher)
        {
            throw new ApiException($"Voucher with ID {id} not found.");
        }
        return await _voucherRepository.DeleteAsync(id);
    }

    public override async Task<CreateUpdateVoucherDto> Get(Guid id)
    {
        var voucher = await _voucherRepository.GetAsync(id);
        if (voucher == null)
        {
            throw new ApiException($"Voucher with ID {id} not found.");
        }
        return _mapper.Map<CreateUpdateVoucherDto>(voucher);
    }
}
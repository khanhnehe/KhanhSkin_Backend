﻿using AutoMapper;
using KhanhSkin_BackEnd.Dtos.Cart;
using KhanhSkin_BackEnd.Dtos.Voucher;
using KhanhSkin_BackEnd.Entities;
using KhanhSkin_BackEnd.Repositories;
using KhanhSkin_BackEnd.Services.CurrentUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using static KhanhSkin_BackEnd.Consts.Enums;

namespace KhanhSkin_BackEnd.Services.Carts
{
    public class CartService : BaseService<KhanhSkin_BackEnd.Entities.Cart, CartDto, AddProductToCartDto, CartGetRequestInputDto>
    {
        private readonly IConfiguration _config;
        private readonly IRepository<KhanhSkin_BackEnd.Entities.Cart> _cartRepository;
        private readonly IRepository<CartItem> _cartItemRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductVariant> _productVariantRepository;
        private readonly IRepository<KhanhSkin_BackEnd.Entities.Voucher> _voucherRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CartService> _logger;
        private readonly ICurrentUser _currentUser;

        public CartService(
            IConfiguration config,
            IRepository<KhanhSkin_BackEnd.Entities.Cart> cartRepository,
            IRepository<CartItem> cartItemRepository,
            IRepository<ProductVariant> productVariantRepository,
            IRepository<Product> productRepository,
            IRepository<KhanhSkin_BackEnd.Entities.Voucher> voucherRepository,
            IMapper mapper,
            ILogger<CartService> logger,
            ICurrentUser currentUser)
            : base(mapper, cartRepository, logger, currentUser)
        {
            _config = config;
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _productRepository = productRepository;
            _productVariantRepository = productVariantRepository;
            _voucherRepository = voucherRepository;
            _mapper = mapper;
            _logger = logger;
            _currentUser = currentUser;
        }

        public async Task<CartDto> AddProductToCart(AddProductToCartDto input)
        {
            try
            {
                // Lấy ID người dùng hiện tại
                var userId = _currentUser.Id;
                if (userId == null)
                {
                    throw new Exception("User not authenticated"); // Ném ngoại lệ nếu người dùng chưa đăng nhập
                }

                // Kiểm tra số lượng sản phẩm thêm vào giỏ hàng
                if (input.AmountAdd < -1)
                {
                    throw new Exception("Số lượng sản phẩm được thêm phải lớn hơn hoặc bằng 1"); // Ném ngoại lệ nếu số lượng sản phẩm không hợp lệ
                }

                // Tìm giỏ hàng của người dùng hiện tại
                var cart = await _cartRepository
                    .AsQueryable()
                    .Include(c => c.Voucher)
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                // Nếu giỏ hàng chưa tồn tại, tạo mới giỏ hàng
                if (cart == null)
                {
                    cart = new Cart
                    {
                        UserId = userId.Value,
                        TotalPrice = 0,
                        CartItems = new List<CartItem>()
                    };
                    await _cartRepository.CreateAsync(cart);
                }

                // Tìm sản phẩm theo ID và bao gồm các biến thể
                var product = await _productRepository
                    .AsQueryable()
                    .Include(p => p.Variants)
                    .FirstOrDefaultAsync(p => p.Id == input.ProductId);

                if (product == null)
                {
                    throw new Exception("Product not found");
                }

                // Kiểm tra và xử lý nếu sản phẩm có biến thể
                if (product.Variants != null && product.Variants.Any())
                {
                    if (!input.VariantId.HasValue)
                    {
                        throw new Exception("Vui lòng chọn biến thể của sản phẩm");
                    }

                    var variant = product.Variants.FirstOrDefault(v => v.Id == input.VariantId.Value);
                    if (variant == null)
                    {
                        throw new Exception("Variant not found for the specified product.");
                    }

                    if (input.AmountAdd > variant.QuantityVariant)
                    {
                        throw new Exception("Số sản phẩm được thêm vượt quá giới hạn tồn kho");
                    }

                    await AddOrUpdateCartItem(cart, product, variant, input.AmountAdd);
                }
                else
                {
                    if (input.AmountAdd > product.Quantity)
                    {
                        throw new Exception("Số sản phẩm được thêm vượt quá giới hạn tồn kho");
                    }

                    await AddOrUpdateCartItem(cart, product, null, input.AmountAdd);
                }

                // Cập nhật tổng giá trị của giỏ hàng
                cart.TotalPrice = cart.CartItems.Sum(cartitem => cartitem.ItemsPrice);

                // Áp dụng voucher nếu có
                if (cart.VoucherId.HasValue)
                {
                    var applyVoucherDto = new ApplyVoucherDto { VoucherId = cart.VoucherId.Value };
                    await ApplyVoucherToCart(applyVoucherDto);
                }
                else
                {
                    cart.FinalPrice = cart.TotalPrice;
                }

                await _cartRepository.UpdateAsync(cart);

                var cartDto = _mapper.Map<CartDto>(cart);
                return cartDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred");
                throw new Exception($"{ex.Message}");
            }
        }

        public async Task ApplyVoucherToCart(ApplyVoucherDto input)
        {
            try
            {
                var userId = _currentUser.Id;
                if (userId == null)
                {
                    throw new Exception("User not authenticated");
                }

                var cart = await _cartRepository
                    .AsQueryable()
                    .Include(c => c.Voucher)
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId.Value);

                if (cart == null)
                {
                    throw new Exception("Cart not found");
                }

                var voucher = await _voucherRepository
                    .AsQueryable()
                    .Include(v => v.ProductVouchers)
                    .FirstOrDefaultAsync(v => v.Id == input.VoucherId);

                if (voucher == null)
                {
                    throw new Exception("Voucher không tồn tại.");
                }

                // Xử lý hành động "remove" để gỡ voucher khỏi giỏ hàng
                if (input.Action == "remove")
                {
                    cart.VoucherId = null;
                    cart.DiscountValue = 0;
                    cart.FinalPrice = cart.TotalPrice;
                    await _cartRepository.UpdateAsync(cart);
                    return;
                }

                // Kiểm tra trạng thái hoạt động và hạn sử dụng của voucher
                if (!voucher.IsActive || voucher.EndTime < DateTime.Now)
                {
                    cart.VoucherId = null;
                    cart.DiscountValue = 0;
                    cart.FinalPrice = cart.TotalPrice;
                    await _cartRepository.UpdateAsync(cart);
                    throw new Exception("Voucher không hợp lệ hoặc đã hết hạn.");
                }

                // Kiểm tra số lượt sử dụng còn lại của voucher
                if (voucher.TotalUses == 0)
                {
                    cart.VoucherId = null;
                    cart.DiscountValue = 0;
                    cart.FinalPrice = cart.TotalPrice;
                    await _cartRepository.UpdateAsync(cart);
                    throw new Exception("Voucher đã hết lượt sử dụng.");
                }

                // Kiểm tra giá trị đơn hàng đạt yêu cầu tối thiểu
                if (cart.TotalPrice < voucher.MinimumOrderValue)
                {
                    cart.VoucherId = null;
                    cart.DiscountValue = 0;
                    cart.FinalPrice = cart.TotalPrice;
                    await _cartRepository.UpdateAsync(cart);
                    throw new Exception("Giá trị đơn hàng không đủ để áp dụng voucher.");
                }

                decimal discountValue = 0;

                // Kiểm tra loại voucher là Specific
                if (voucher.VoucherType == VoucherType.Specific)
                {
                    var productIdsInCart = cart.CartItems.Select(ci => ci.ProductId).ToList();
                    var productIdsInVoucher = voucher.ProductVouchers.Select(pv => pv.ProductId).ToList();

                    var eligibleCartItems = cart.CartItems
                        .Where(ci => productIdsInVoucher.Contains(ci.ProductId))
                        .ToList();

                    if (!eligibleCartItems.Any())
                    {
                        cart.VoucherId = null;
                        cart.DiscountValue = 0;
                        cart.FinalPrice = cart.TotalPrice;
                        await _cartRepository.UpdateAsync(cart);
                        throw new Exception("Voucher chỉ áp dụng cho một số sản phẩm nhất định.");
                    }

                    // Tính toán giảm giá dựa trên từng sản phẩm đủ điều kiện
                    if (voucher.DiscountType == DiscountType.Percentage)
                    {
                        foreach (var cartItem in eligibleCartItems)
                        {
                            discountValue += cartItem.ItemsPrice * voucher.DiscountValue / 100;
                        }
                    }
                    else if (voucher.DiscountType == DiscountType.AmountMoney)
                    {
                        // Áp dụng giảm giá cố định cho từng sản phẩm đủ điều kiện, chia đều nếu có nhiều sản phẩm
                        var discountPerItem = voucher.DiscountValue / eligibleCartItems.Count;
                        foreach (var cartItem in eligibleCartItems)
                        {
                            discountValue += discountPerItem;
                        }
                    }
                }
                else
                {
                    // Đối với các loại voucher không phải Specific, áp dụng giảm giá toàn bộ giỏ hàng
                    if (voucher.DiscountType == DiscountType.Percentage)
                    {
                        discountValue = cart.TotalPrice * voucher.DiscountValue / 100;
                    }
                    else if (voucher.DiscountType == DiscountType.AmountMoney)
                    {
                        discountValue = voucher.DiscountValue;
                    }
                }

                // Cập nhật giá trị giảm và giá cuối cùng
                cart.DiscountValue = discountValue;
                cart.FinalPrice = cart.TotalPrice - discountValue;
                cart.VoucherId = voucher.Id;

                await _cartRepository.UpdateAsync(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while applying voucher to cart.");
                throw new Exception($"{ex.Message}");
            }
        }

        public async Task AddOrUpdateCartItem(Cart cart, Product product, ProductVariant variant, int amountAdd)
            {
                CartItem existCartItem = null;

                if (variant != null)
                {
                    // Tìm sản phẩm trong giỏ hàng theo biến thể
                    existCartItem = cart.CartItems.FirstOrDefault(cartitem => cartitem.ProductId == product.Id && cartitem.VariantId == variant.Id);

                    if (existCartItem != null)
                    {
                        // Nếu sản phẩm đã tồn tại trong giỏ hàng, cập nhật số lượng và giá
                        if (existCartItem.Amount + amountAdd > variant.QuantityVariant)
                        {
                            throw new Exception("Bạn đã có sản phẩm trong giỏ hàng. Số sản phẩm được thêm vượt quá giới hạn tồn kho");
                        }

                        existCartItem.Amount += amountAdd;
                        existCartItem.ItemsPrice = existCartItem.Amount * (variant.SalePriceVariant ?? variant.PriceVariant);
                        await _cartItemRepository.UpdateAsync(existCartItem);
                    }
                    else
                    {
                        // Nếu sản phẩm chưa tồn tại trong giỏ hàng, tạo mới sản phẩm
                        existCartItem = new CartItem
                        {
                            ProductId = product.Id,
                            VariantId = variant.Id,
                            ProductName = product.ProductName,
                            NameVariant = variant.NameVariant ?? "Default Variant",
                            ProductPrice = variant.PriceVariant,
                            ProductSalePrice = variant.SalePriceVariant ?? variant.PriceVariant,
                            Images = product.Images,
                            Amount = amountAdd,
                            ItemsPrice = (variant.SalePriceVariant ?? variant.PriceVariant) * amountAdd,
                            CartId = cart.Id
                        };
                        cart.CartItems.Add(existCartItem);
                        await _cartItemRepository.CreateAsync(existCartItem);
                    }
                }
                else
                {
                    // Tìm sản phẩm trong giỏ hàng không có biến thể
                    existCartItem = cart.CartItems.FirstOrDefault(cartitem => cartitem.ProductId == product.Id && cartitem.VariantId == null);

                    if (existCartItem != null)
                    {
                        // Nếu sản phẩm đã tồn tại trong giỏ hàng, cập nhật số lượng và giá
                        if (existCartItem.Amount + amountAdd > product.Quantity)
                        {
                            throw new Exception("Bạn đã có sản phẩm trong giỏ hàng. Số sản phẩm được thêm vượt quá giới hạn tồn kho");
                        }

                        existCartItem.Amount += amountAdd;
                        existCartItem.ItemsPrice = existCartItem.Amount * (product.SalePrice ?? product.Price);
                        await _cartItemRepository.UpdateAsync(existCartItem);
                    }
                    else
                    {
                        // Nếu sản phẩm chưa tồn tại trong giỏ hàng, tạo mới sản phẩm
                        existCartItem = new CartItem
                        {
                            ProductId = product.Id,
                            ProductName = product.ProductName,
                            NameVariant = null,
                            ProductPrice = product.Price,
                            ProductSalePrice = product.SalePrice ?? product.Price,
                            Images = product.Images,
                            Amount = amountAdd,
                            ItemsPrice = (product.SalePrice ?? product.Price) * amountAdd,
                            CartId = cart.Id
                        };
                        cart.CartItems.Add(existCartItem);
                        await _cartItemRepository.CreateAsync(existCartItem);
                    }
                }

                // Cập nhật tổng giá trị của giỏ hàng
                cart.TotalPrice = cart.CartItems.Sum(cartitem => cartitem.ItemsPrice);

            // Áp dụng lại voucher nếu có
            // Áp dụng voucher nếu có
            if (cart.VoucherId.HasValue)
            {
                var applyVoucherDto = new ApplyVoucherDto { VoucherId = cart.VoucherId.Value };
                await ApplyVoucherToCart(applyVoucherDto);
            }
            else
                {
                    cart.FinalPrice = cart.TotalPrice;
                }

                await _cartRepository.UpdateAsync(cart);
            }


        public async Task<CartDto> RemoveCartItem(Guid cartItemId)
        {
            try
            {
                // Lấy ID người dùng hiện tại
                var userId = _currentUser.Id;
                if (userId == null)
                {
                    throw new Exception("User not authenticated");
                }

                // Tìm giỏ hàng của người dùng hiện tại
                var cart = await _cartRepository
                    .AsQueryable()
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId.Value);

                if (cart == null)
                {
                    throw new Exception("Cart not found for the specified user.");
                }

                // Tìm CartItem cần xóa
                var cartItem = cart.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);
                if (cartItem == null)
                {
                    throw new Exception("Cart item not found.");
                }

                // Xóa CartItem
                cart.CartItems.Remove(cartItem);
                await _cartItemRepository.DeleteAsync(cartItem.Id);

                // Cập nhật tổng giá trị của giỏ hàng
                cart.TotalPrice = cart.CartItems.Sum(ci => ci.ItemsPrice);

                // Áp dụng voucher nếu có
                if (cart.VoucherId.HasValue)
                {
                    var applyVoucherDto = new ApplyVoucherDto { VoucherId = cart.VoucherId.Value };
                    await ApplyVoucherToCart(applyVoucherDto);
                }
                else
                {
                    cart.FinalPrice = cart.TotalPrice;
                }

                await _cartRepository.UpdateAsync(cart);

                var cartDto = _mapper.Map<CartDto>(cart);
                return cartDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while removing item from cart");
                throw new Exception($"{ex.Message}");
            }
        }

        public async Task<CartDto> GetCartByUserId()
        {
            try
            {
                // Retrieve the current user's ID
                var userId = _currentUser.Id;
                if (userId == null)
                {
                    throw new Exception("User not authenticated");
                }

                // Find the cart for the current user
                var cart = await _cartRepository
                    .AsQueryable()
                    .Include(c => c.Voucher)
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId.Value);

                if (cart == null)
                {
                    throw new Exception("Cart not found for the specified user.");
                }

                var cartDto = _mapper.Map<CartDto>(cart);
                return cartDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the cart for the current user.");
                throw new Exception($"An error occurred: {ex.Message}");
            }
        }




    }
}

using AutoMapper;
using KhanhSkin_BackEnd.Consts;
using KhanhSkin_BackEnd.Dtos.Address;
using KhanhSkin_BackEnd.Dtos.Cart;
using KhanhSkin_BackEnd.Dtos.Order;
using KhanhSkin_BackEnd.Entities;
using KhanhSkin_BackEnd.Repositories;
using KhanhSkin_BackEnd.Services.CurrentUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static KhanhSkin_BackEnd.Consts.Enums;

namespace KhanhSkin_BackEnd.Services.Orders
{
    public class OrderService : BaseService<Order, OrderDto, OrderItemDto, OrderGetRequestInputDto>
    {
        private readonly IConfiguration _config;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Cart> _cartRepository;
        private readonly IRepository<ProductVariant> _productVariantRepository;
        private readonly IRepository<KhanhSkin_BackEnd.Entities.Voucher> _voucherRepository;
        private readonly IRepository<UserVoucher> _userVoucherRepository;
        private readonly IRepository<KhanhSkin_BackEnd.Entities.Address> _addressRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderService> _logger;
        private readonly ICurrentUser _currentUser;

        public OrderService(
           IConfiguration config,
           IRepository<Order> orderRepository,
           IRepository<OrderItem> orderItemRepository,
           IRepository<ProductVariant> productVariantRepository,
           IRepository<Product> productRepository,
           IRepository<Cart> cartRepository,
           IRepository<KhanhSkin_BackEnd.Entities.Voucher> voucherRepository,
           IRepository<KhanhSkin_BackEnd.Entities.Address> addressRepository,
           IRepository<UserVoucher> userVoucherRepository,
           IMapper mapper,
           ILogger<OrderService> logger,
           ICurrentUser currentUser)
           : base(mapper, orderRepository, logger, currentUser)
        {
            _config = config;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _productRepository = productRepository;
            _cartRepository = cartRepository;
            _productVariantRepository = productVariantRepository;
            _voucherRepository = voucherRepository;
            _addressRepository = addressRepository;
            _userVoucherRepository = userVoucherRepository;
            _mapper = mapper;
            _logger = logger;
            _currentUser = currentUser;
        }

        public async Task<OrderDto> CheckOutOrder(CheckoutOrderDto input)
        {
            // Lấy ID người dùng hiện tại
            var userId = _currentUser.Id;
            if (userId == null)
            {
                throw new Exception("User not authenticated");
            }

            // Tìm giỏ hàng của người dùng hiện tại
            var cart = await _cartRepository.AsQueryable()
                .Include(c => c.CartItems)
                .Include(c => c.Voucher)
                .FirstOrDefaultAsync(c => c.Id == input.CartId && c.UserId == userId);

            if (cart == null)
            {
                throw new Exception("Cart not found or does not belong to the current user");
            }

            // Tìm địa chỉ giao hàng
            var address = await _addressRepository.GetAsync(input.AddressId);
            if (address == null || address.UserId != userId)
            {
                throw new Exception("Address not found or does not belong to the current user");
            }

            // Tạo đơn hàng mới
            var order = new Order
            {
                UserId = userId.Value,
                CartId = cart.Id,
                AddressId = input.AddressId,
                DiscountValue = cart.DiscountValue,
                ShippingMethod = input.ShippingMethod,
                PaymentMethod = input.PaymentMethod,
                OrderDate = DateTime.Now,
                ShippingAddressSnapshot = $"{address.AddressDetail}, {address.Province}, {address.District}, {address.Ward}, {address.PhoneNumber}",
                OrderItems = _mapper.Map<ICollection<OrderItem>>(cart.CartItems),
                ShippingPrice = (input.ShippingMethod == Enums.ShippingMethod.FasfDelivery) ? 35000 : 25000 // Tính phí vận chuyển
            };

            // Áp dụng voucher nếu có
            if (cart.VoucherId.HasValue)
            {
                var voucher = await _voucherRepository
                    .AsQueryable()
                    .Include(v => v.ProductVouchers)  
                    .FirstOrDefaultAsync(v => v.Id == cart.VoucherId.Value);

                if (voucher == null || voucher.EndTime < DateTime.Now)
                {
                    throw new Exception("Voucher không hợp lệ hoặc đã hết hạn");
                }

                // Kiểm tra giá trị đơn hàng có đạt yêu cầu tối thiểu để áp dụng voucher hay không
                if (cart.TotalPrice < voucher.MinimumOrderValue)
                {
                    throw new Exception("Giá trị đơn hàng không đủ để áp dụng voucher.");
                }

                if (voucher.VoucherType == VoucherType.Specific)
                {
                    var productIdsInCart = cart.CartItems.Select(ci => ci.ProductId).ToList();
                    var productIdsInVoucher = voucher.ProductVouchers.Select(pv => pv.ProductId).ToList();

                    var validProducts = productIdsInCart.Intersect(productIdsInVoucher).ToList();


                    if (!validProducts.Any())
                    {
                        throw new Exception("Voucher chỉ áp dụng cho một số sản phẩm nhất định.");
                    }
                }

                // Giảm TotalUses sau khi thành công
                if (voucher.TotalUses > 0)
                {
                    voucher.TotalUses--;
                    await _voucherRepository.UpdateAsync(voucher);
                }
                else
                {
                    throw new Exception("Voucher đã hết lượt sử dụng.");
                }
            }

            // Tính toán tổng giá trị đơn hàng
            order.FinalPrice = cart.FinalPrice + order.ShippingPrice;

            // Tạo mã theo dõi đơn hàng (Tracking Code)
            order.TrackingCode = GenerateTrackingCode();

            // Thêm đơn hàng vào cơ sở dữ liệu
            await _orderRepository.CreateAsync(order);

            // Xóa giỏ hàng sau khi tạo đơn hàng (nếu cần)
            await _cartRepository.DeleteAsync(cart.Id);

            // Trả về OrderDto
            var orderDto = _mapper.Map<OrderDto>(order);
            orderDto.Address = _mapper.Map<AddressDto>(address); // Ánh xạ Address sang AddressDto

            return orderDto;
        }



        public async Task ApplyVouchertoOrder(ApplyVoucherOrderDto input)
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
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId.Value);

                if (cart == null)
                {
                    throw new Exception("Cart not found");
                }

                var voucher = await _voucherRepository
                    .AsQueryable()
                    .FirstOrDefaultAsync(v => v.Id == input.VoucherId);

                if (voucher == null)
                {
                    throw new Exception("Voucher không tồn tại.");
                }

                // Nếu hành động là "remove", xóa voucher khỏi giỏ hàng
                if (input.Action == "remove")
                {
                    cart.VoucherId = null;
                    cart.DiscountValue = 0;
                    cart.FinalPrice = cart.TotalPrice;
                    await _cartRepository.UpdateAsync(cart);
                    return;
                }

                // Kiểm tra nếu voucher không còn hoạt động hoặc hết hạn
                if (!voucher.IsActive || voucher.EndTime < DateTime.Now)
                {
                    cart.VoucherId = null;
                    cart.DiscountValue = 0;
                    cart.FinalPrice = cart.TotalPrice;
                    await _cartRepository.UpdateAsync(cart);
                    throw new Exception("Voucher không hợp lệ hoặc đã hết hạn.");
                }

                // Kiểm tra giá trị đơn hàng có đạt yêu cầu tối thiểu để áp dụng voucher hay không
                if (cart.TotalPrice < voucher.MinimumOrderValue)
                {
                    cart.VoucherId = null;
                    cart.DiscountValue = 0;
                    cart.FinalPrice = cart.TotalPrice;
                    await _cartRepository.UpdateAsync(cart);
                    throw new Exception("Giá trị đơn hàng không đủ để áp dụng voucher.");
                }

                // Kiểm tra nếu voucher là Specific, thì chỉ áp dụng cho một số sản phẩm nhất định
                if (voucher.VoucherType == VoucherType.Specific)
                {
                    var productIdsInCart = cart.CartItems.Select(ci => ci.ProductId).ToList();
                    var productIdsInVoucher = voucher.ProductVouchers.Select(pv => pv.ProductId).ToList();

                    var validProducts = await _productRepository
                        .AsQueryable()
                        .Where(p => productIdsInVoucher.Contains(p.Id) && productIdsInCart.Contains(p.Id))
                        .ToListAsync();

                    if (!validProducts.Any())
                    {
                        cart.VoucherId = null;
                        cart.DiscountValue = 0;
                        cart.FinalPrice = cart.TotalPrice;
                        await _cartRepository.UpdateAsync(cart);
                        throw new Exception("Voucher chỉ áp dụng cho một số sản phẩm nhất định.");
                    }
                }

                // Tính toán giá trị giảm giá và áp dụng voucher
                decimal discountValue = 0;
                if (voucher.DiscountType == DiscountType.AmountMoney)
                {
                    discountValue = voucher.DiscountValue;
                }
                else if (voucher.DiscountType == DiscountType.Percentage)
                {
                    discountValue = cart.TotalPrice * voucher.DiscountValue / 100;
                }

                cart.DiscountValue = discountValue;
                cart.FinalPrice = cart.TotalPrice - discountValue;
                cart.VoucherId = voucher.Id;

                await _cartRepository.UpdateAsync(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while applying voucher to cart.");
                throw new Exception($"An error occurred while applying voucher to cart: {ex.Message}");
            }
        }

        private string GenerateTrackingCode()
        {
            // Tạo mã theo dõi đơn hàng duy nhất
            return $"TRK-{Guid.NewGuid().ToString().ToUpper().Substring(0, 8)}";
        }



    }
}

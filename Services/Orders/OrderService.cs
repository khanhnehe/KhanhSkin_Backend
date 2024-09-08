using AutoMapper;
using KhanhSkin_BackEnd.Consts;
using KhanhSkin_BackEnd.Dtos.Address;
using KhanhSkin_BackEnd.Dtos.Cart;
using KhanhSkin_BackEnd.Dtos.Order;
using KhanhSkin_BackEnd.Entities;
using KhanhSkin_BackEnd.Helper;
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
            var userId = _currentUser.Id;
            if (userId == null)
            {
                throw new Exception("User not authenticated");
            }

            var cart = await _cartRepository.AsQueryable()
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Variant)
                .Include(c => c.Voucher)
                .FirstOrDefaultAsync(c => c.Id == input.CartId && c.UserId == userId);

            if (cart == null)
            {
                throw new Exception("Cart not found or does not belong to the current user");
            }

            var address = await _addressRepository.GetAsync(input.AddressId);
            if (address == null || address.UserId != userId)
            {
                throw new Exception("Address not found or does not belong to the current user");
            }

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
                ShippingPrice = (input.ShippingMethod == ShippingMethod.FasfDelivery) ? 35000 : 25000
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

            // Giảm số lượng sản phẩm và variant khi đơn hàng được đặt thành công
            foreach (var cartItem in cart.CartItems)
            {
                var product = await _productRepository.GetAsync(cartItem.ProductId);

                if (cartItem.VariantId.HasValue)
                {
                    // Nếu sản phẩm có variant, giảm số lượng của variant và product
                    var variant = await _productVariantRepository.GetAsync(cartItem.VariantId.Value);
                    if (variant == null)
                    {
                        throw new Exception($"Variant không tồn tại cho sản phẩm {product.ProductName}");
                    }

                    if (variant.QuantityVariant < cartItem.Amount)
                    {
                        throw new Exception($"Số lượng variant {variant.NameVariant} không đủ để thực hiện đơn hàng.");
                    }

                    variant.QuantityVariant -= cartItem.Amount; // Trừ số lượng variant
                    product.Quantity -= cartItem.Amount; // Trừ số lượng tổng của product
                    await _productVariantRepository.UpdateAsync(variant);
                }
                else
                {
                    // Nếu không có variant, chỉ giảm số lượng của product
                    if (product.Quantity < cartItem.Amount)
                    {
                        throw new Exception($"Số lượng sản phẩm {product.ProductName} không đủ để thực hiện đơn hàng.");
                    }

                    product.Quantity -= cartItem.Amount;
                }

                await _productRepository.UpdateAsync(product);
            }

            order.TrackingCode = CreateTrackingCode();

            await _orderRepository.CreateAsync(order);

            // Xóa giỏ hàng sau khi tạo đơn 
            await _cartRepository.DeleteAsync(cart.Id);

            // Trả về OrderDto
            var orderDto = _mapper.Map<OrderDto>(order);
            orderDto.Address = _mapper.Map<AddressDto>(address);

            //lấy mô tả cho enum
            orderDto.ShippingMethodDes = order.ShippingMethod.GetDescription();
            orderDto.PaymentMethodDes = order.PaymentMethod.GetDescription();
            orderDto.OrderStatusDes = order.OrderStatus.GetDescription();

            return orderDto;
        }


        private string CreateTrackingCode()
        {
            return $"ORD-{Guid.NewGuid().ToString().ToUpper().Substring(0, 8)}";
        }



        public async Task<List<OrderDto>> GetOrderByUserId()
        {
            try
            {
                // Retrieve the current user's ID
                var userId = _currentUser.Id;
                if (userId == null)
                {
                    throw new Exception("User not authenticated");
                }

                // Find all orders for the current user
                var orders = await _orderRepository
                    .AsQueryable()
                    .Include(c => c.OrderItems)
                    .Include(c => c.Address)
                    .Include(o => o.User)
                    .Where(c => c.UserId == userId.Value) // Find all orders for the user
                    .ToListAsync();

                // Nếu không có đơn hàng nào, trả về danh sách rỗng
                if (orders == null || !orders.Any())
                {
                    return new List<OrderDto>();
                }

                // Map the list of orders to a list of OrderDto
                var orderDtos = _mapper.Map<List<OrderDto>>(orders);
                return orderDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the orders for the current user.");
                throw new Exception($"{ex.Message}");
            }
        }




        // Phương thức thay đổi trạng thái đơn hàng
        public async Task<OrderDto> ChangeStatusOrder(ChangeStatus input)
        {
            // Kiểm tra tính hợp lệ của Order ID
            if (input.OrderId == Guid.Empty)
            {
                throw new Exception("Invalid Order ID.");
            }

            // Lấy thông tin đơn hàng từ cơ sở dữ liệu
            var order = await _orderRepository.AsQueryable()
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product) // Bao gồm thông tin sản phẩm của từng mục đơn hàng
                .FirstOrDefaultAsync(o => o.Id == input.OrderId); 

            if (order == null)
            {
                throw new Exception("Order not found.");
            }

            // Xử lý hành động của admin dựa trên trạng thái
            var status = input.Status.ToLower();

            if (status == "cancel")
            {
                await CancelOrder(order); // Hủy
            }
            else if (status == "confirm")
            {
                await ConfirmOrder(order); // Xác nhận 
            }
            else if (status == "receive")
            {
                await ReceiveOrder(order); // đã nhận
            }
            else
            {
                throw new Exception("Invalid status action.");
            }

            await _orderRepository.UpdateAsync(order);

            var orderDto = _mapper.Map<OrderDto>(order);
            orderDto.Address = _mapper.Map<AddressDto>(order.Address);

            orderDto.OrderStatusDes = order.OrderStatus.GetDescription();


            return orderDto;
        }

        // hủy đơn hàng
        private async Task CancelOrder(Order order)
        {
            order.OrderStatus = Enums.OrderStatus.Canceled;
            order.DeliveryDate = null;

            // Trả lại số lượng sản phẩm và variant (nếu có)
            foreach (var orderItem in order.OrderItems)
            {
                // Lấy sản phẩm và bao gồm cả danh sách các biến thể
                var product = await _productRepository.AsQueryable()
                    .Include(p => p.Variants) // Bao gồm biến thể của sản phẩm
                    .FirstOrDefaultAsync(p => p.Id == orderItem.ProductId);

                if (product == null)
                {
                    throw new Exception($"Không tìm thấy sản phẩm với productId {orderItem.ProductId}.");
                }

                // Nếu sản phẩm có Variant
                if (orderItem.VariantId.HasValue)
                {
                    var variant = product.Variants.FirstOrDefault(v => v.Id == orderItem.VariantId.Value);
                    if (variant == null)
                    {
                        throw new Exception($"Không tìm thấy biến thể cho sản phẩm {product.ProductName} với variantId {orderItem.VariantId.Value}.");
                    }

                    // Cập nhật lại số lượng cho variant
                    variant.QuantityVariant += orderItem.Amount;
                    _logger.LogInformation("Cập nhật lại số lượng biến thể cho variantId: {VariantId}", orderItem.VariantId.Value);

                    // Cập nhật lại số lượng của sản phẩm bằng với số lượng thay đổi của biến thể
                    product.Quantity += orderItem.Amount;
                    _logger.LogInformation("Cập nhật lại số lượng sản phẩm cho productId: {ProductId} theo biến thể", product.Id);
                }
                else
                {
                    // Nếu sản phẩm không có Variant, chỉ cộng lại số lượng sản phẩm
                    product.Quantity += orderItem.Amount;
                    _logger.LogInformation("Cập nhật lại số lượng sản phẩm cho productId: {ProductId}", orderItem.ProductId);
                }

                // Cập nhật lại sản phẩm sau khi điều chỉnh
                await _productRepository.UpdateAsync(product);
            }

            var orderDto = _mapper.Map<OrderDto>(order);
            orderDto.OrderStatusDes = order.OrderStatus.GetDescription();
        }


        // xác nhận đơn hàng
        private async Task ConfirmOrder(Order order)
        {
            order.OrderStatus = Enums.OrderStatus.Shipping; 
            order.DeliveryDate = DateTime.Now;

            var orderDto = _mapper.Map<OrderDto>(order);
            orderDto.OrderStatusDes = order.OrderStatus.GetDescription();
        }

        // đã nhận
        private async Task ReceiveOrder(Order order)
        {
            order.OrderStatus = Enums.OrderStatus.Completed; 

            // Cập nhật số lượng mua của sản phẩm
            foreach (var orderItem in order.OrderItems)
            {
                var product = await _productRepository.GetAsync(orderItem.ProductId);
                if (product == null)
                {
                    throw new Exception($"Product not found.");
                }

                product.Purchases += orderItem.Amount; // Tăng số lượng mua của sản phẩm

                await _productRepository.UpdateAsync(product); 
            }

            var orderDto = _mapper.Map<OrderDto>(order);
            orderDto.OrderStatusDes = order.OrderStatus.GetDescription();
        }

        public async Task<List<OrderDto>> GetAllOrders()
        {
            var allOrders = await _orderRepository.AsQueryable()
                .Include(o => o.OrderItems)
                .Include(o => o.Address)
                .Include(o => o.User)
                .ToListAsync();

            if (allOrders == null || !allOrders.Any())
            {
                throw new Exception("No orders found.");
            }

            var orderDtos = _mapper.Map<List<OrderDto>>(allOrders);
            foreach (var orderDto in orderDtos)
            {
                var order = allOrders.First(o => o.Id == orderDto.Id);
                orderDto.Address = _mapper.Map<AddressDto>(order.Address);
            }

            return orderDtos;
        }


        public async Task<List<OrderDto>> GetOrderByStatus(OrderGetRequestInputDto input)
        {
            // Bắt đầu truy vấn với tất cả đơn hàng
            var query = _orderRepository.AsQueryable()
                .Include(o => o.OrderItems)
                .Include(o => o.User)
                .Include(o => o.Address)
                .AsNoTracking();

            // Kiểm tra nếu người dùng muốn lọc theo trạng thái đơn hàng
            if (input.OrderStatus.HasValue)
            {
                query = query.Where(o => o.OrderStatus == input.OrderStatus.Value);
            }

            // Các logic lọc khác nếu có
            // ví dụ: lọc theo userId hoặc ngày tạo đơn hàng

            // Lấy danh sách đơn hàng sau khi lọc
            var orders = await query.ToListAsync();

            // Ánh xạ kết quả sang OrderDto và trả về
            var orderDtos = _mapper.Map<List<OrderDto>>(orders);
            return orderDtos;
        }

        public async Task<List<OrderDto>> GetOrderByUserIdAndStatus(OrderGetRequestInputDto input)
        {
            try
            {
                // Lấy ID của người dùng hiện tại
                var userId = _currentUser.Id;
                if (userId == null)
                {
                    throw new Exception("User not authenticated");
                }

                // Bắt đầu truy vấn với tất cả các đơn hàng của người dùng hiện tại
                var query = _orderRepository.AsQueryable()
                    .Include(o => o.OrderItems)
                    .Include(o => o.Address)
                    .Include(o => o.User)
                    .Where(o => o.UserId == userId.Value) // Lọc theo UserId của người dùng hiện tại
                    .AsNoTracking();

                // Lọc theo trạng thái đơn hàng nếu có
                if (input.OrderStatus.HasValue)
                {
                    query = query.Where(o => o.OrderStatus == input.OrderStatus.Value);
                }

                // Lấy danh sách đơn hàng sau khi áp dụng các điều kiện lọc
                var orders = await query.ToListAsync();

                // Nếu không tìm thấy đơn hàng, trả về danh sách rỗng
                if (orders == null || !orders.Any())
                {
                    return new List<OrderDto>();
                }

                // Ánh xạ danh sách đơn hàng sang OrderDto và trả về
                var orderDtos = _mapper.Map<List<OrderDto>>(orders);
                return orderDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving orders for the current user and status.");
                throw new Exception($"{ex.Message}");
            }
        }

    }
}

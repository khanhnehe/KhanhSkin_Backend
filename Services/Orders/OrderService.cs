using AutoMapper;
using KhanhSkin_BackEnd.Consts;
using KhanhSkin_BackEnd.Dtos.Address;
using KhanhSkin_BackEnd.Dtos.Cart;
using KhanhSkin_BackEnd.Dtos.Order;
using KhanhSkin_BackEnd.Dtos.Product;
using KhanhSkin_BackEnd.Entities;
using KhanhSkin_BackEnd.Helper;
using KhanhSkin_BackEnd.Repositories;
using KhanhSkin_BackEnd.Services.CurrentUser;
using KhanhSkin_BackEnd.Share.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static KhanhSkin_BackEnd.Consts.Enums;
using System.Text.RegularExpressions;

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
        private readonly IRepository<KhanhSkin_BackEnd.Entities.InventoryLog> _inventoryLogRepository;
        private readonly IRepository<KhanhSkin_BackEnd.Entities.Address> _addressRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderService> _logger;
        private readonly ICurrentUser _currentUser;

        public OrderService(
           IConfiguration config,
           IRepository<Order> orderRepository,
           IRepository<OrderItem> orderItemRepository,
           IRepository<KhanhSkin_BackEnd.Entities.InventoryLog> inventoryLogRepository,
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
            _inventoryLogRepository = inventoryLogRepository;
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
                    .ThenInclude(ci => ci.Product)
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
                DiscountValue = cart.DiscountValue,
                ShippingMethod = input.ShippingMethod,
                PaymentMethod = input.PaymentMethod,
                OrderDate = DateTime.Now,
                PhoneNumber = address.PhoneNumber,
                Province = address.Province,
                District = address.District,
                Ward = address.Ward,
                AddressDetail = address.AddressDetail,
                ShippingPrice = (input.ShippingMethod == ShippingMethod.FasfDelivery) ? 35000 : 25000,
            };

            order.FinalPrice = cart.FinalPrice + order.ShippingPrice;

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

                order.VoucherId = voucher.Id;
            }

            var orderItems = new List<OrderItem>();

            foreach (var cartItem in cart.CartItems)
            {
                var orderItem = new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    ProductName = cartItem.Product.ProductName,
                    VariantId = cartItem.VariantId,
                    NameVariant = cartItem.Variant?.NameVariant,
                    ProductPrice = cartItem.ProductPrice,
                    ProductSalePrice = cartItem.ProductSalePrice,
                    Amount = cartItem.Amount,
                    ItemsPrice = cartItem.ItemsPrice,
                    Images = cartItem.Product.Images.ToList()
                };

                orderItems.Add(orderItem);
            }

            order.OrderItems = orderItems;

            foreach (var cartItem in cart.CartItems)
            {
                var product = await _productRepository.GetAsync(cartItem.ProductId);

                if (cartItem.VariantId.HasValue)
                {
                    var variant = await _productVariantRepository.GetAsync(cartItem.VariantId.Value);
                    if (variant == null)
                    {
                        throw new Exception($"Variant không tồn tại cho sản phẩm {product.ProductName}");
                    }

                    if (variant.QuantityVariant < cartItem.Amount)
                    {
                        throw new Exception($"Số lượng variant {variant.NameVariant} không đủ để thực hiện đơn hàng.");
                    }

                    variant.QuantityVariant -= cartItem.Amount;
                    product.Quantity -= cartItem.Amount;
                    await _productVariantRepository.UpdateAsync(variant);
                }
                else
                {
                    if (product.Quantity < cartItem.Amount)
                    {
                        throw new Exception($"Số lượng sản phẩm {product.ProductName} không đủ để thực hiện đơn hàng.");
                    }

                    product.Quantity -= cartItem.Amount;
                }

                await _productRepository.UpdateAsync(product);

                // Tạo InventoryLog từ cartItem thay vì orderItem
                await LogInventoryFromCartItem(cartItem, Enums.ActionType.Export, "Đặt hàng");

            }

            order.TrackingCode = CreateTrackingCode();

            await _orderRepository.CreateAsync(order);

            await _cartRepository.DeleteAsync(cart.Id);

            var orderDto = _mapper.Map<OrderDto>(order);
            orderDto.Address = _mapper.Map<AddressDto>(address);

            orderDto.ShippingMethodDes = order.ShippingMethod.GetDescription();
            orderDto.PaymentMethodDes = order.PaymentMethod.GetDescription();
            orderDto.OrderStatusDes = order.OrderStatus.GetDescription();

            return orderDto;
        }

        private string CreateTrackingCode()
        {
            return $"ORD-{Guid.NewGuid().ToString().ToUpper().Substring(0, 8)}";
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
                .FirstOrDefaultAsync(o => o.Id == input.OrderId);

            if (order == null)
            {
                throw new Exception("Order not found.");
            }

            // Xử lý hành động của admin dựa trên trạng thái
            var status = input.Status.ToLower();

            if (status == "cancel")
            {
                await CancelOrder(order); // Hủy đơn hàng
            }
            else if (status == "confirm")
            {
                await ConfirmOrder(order); // Xác nhận đơn hàng
            }
            else if (status == "receive")
            {
                await ReceiveOrder(order); // Đã nhận hàng
            }
            else
            {
                throw new Exception("Invalid status action.");
            }

            await _orderRepository.UpdateAsync(order);

            var orderDto = _mapper.Map<OrderDto>(order);
            orderDto.OrderStatusDes = order.OrderStatus.GetDescription();

            return orderDto;
        }

        // Hủy đơn hàng và cập nhật lại số lượng sản phẩm/biến thể
        private async Task CancelOrder(Order order)
        {
            order.OrderStatus = Enums.OrderStatus.Canceled;
            order.DeliveryDate = null;

            // Trả lại số lượng sản phẩm và variant (nếu có)
            foreach (var orderItem in order.OrderItems)
            {
                // Lấy sản phẩm từ cơ sở dữ liệu dựa vào ProductId
                var product = await _productRepository.AsQueryable()
                    .Include(p => p.Variants) // Bao gồm biến thể của sản phẩm
                    .FirstOrDefaultAsync(p => p.Id == orderItem.ProductId);

                if (product == null)
                {
                    _logger.LogWarning($"Không tìm thấy sản phẩm với productId {orderItem.ProductId}. Không thể cập nhật lại số lượng.");
                    continue;
                }

                // Nếu sản phẩm có Variant
                if (orderItem.VariantId.HasValue)
                {
                    var variant = product.Variants.FirstOrDefault(v => v.Id == orderItem.VariantId.Value);

                    if (variant == null)
                    {
                        _logger.LogWarning($"Không tìm thấy biến thể cho sản phẩm {product.ProductName} với variantId {orderItem.VariantId.Value}. Không thể cập nhật lại số lượng biến thể.");
                        continue;
                    }

                    variant.QuantityVariant += orderItem.Amount;
                    _logger.LogInformation("Cập nhật lại số lượng biến thể cho variantId: {VariantId}", orderItem.VariantId.Value);
                }

                product.Quantity += orderItem.Amount;
                _logger.LogInformation("Cập nhật lại số lượng sản phẩm cho productId: {ProductId}", product.Id);

                await _productRepository.UpdateAsync(product);
                // Tạo InventoryLog từ orderItem sau khi hoàn tác số lượng
                await LogInventoryOrderItem(orderItem, Enums.ActionType.Import, "Hủy đơn");
            }
        }

        // Xác nhận đơn hàng
        private async Task ConfirmOrder(Order order)
        {
            order.OrderStatus = Enums.OrderStatus.Shipping;
            order.DeliveryDate = DateTime.Now;
        }

        // Đã nhận hàng
        private async Task ReceiveOrder(Order order)
        {
            order.OrderStatus = Enums.OrderStatus.Completed;

            // Cập nhật số lượng mua của sản phẩm
            foreach (var orderItem in order.OrderItems)
            {
                var product = await _productRepository.GetAsync((Guid)orderItem.ProductId);
                if (product == null)
                {
                    throw new Exception($"Product not found.");
                }

                product.Purchases += orderItem.Amount; // Tăng số lượng mua của sản phẩm
                await _productRepository.UpdateAsync(product);
            }
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
                    //.Include(c => c.Address)
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

     
        public async Task<List<OrderDto>> GetAllOrders()
        {
            var allOrders = await _orderRepository.AsQueryable()
                .Include(o => o.OrderItems)
                //.Include(o => o.Address)
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
                //orderDto.Address = _mapper.Map<AddressDto>(order.Address);
            }

            return orderDtos;
        }


        public IQueryable<Order> GetOrderByStatus(OrderGetRequestInputDto input)
        {
            // Bắt đầu truy vấn với tất cả đơn hàng
            var query = _orderRepository.AsQueryable()
                .Include(o => o.OrderItems)
                .Include(o => o.User)
                //.Include(o => o.Address)
                .AsNoTracking();

            query = query.OrderByDescending(o => o.OrderDate);

            // Kiểm tra nếu người dùng muốn lọc theo trạng thái đơn hàng
            if (input.OrderStatus.HasValue)
            {
                query = query.Where(o => o.OrderStatus == input.OrderStatus.Value);
            }

            // Kiểm tra nếu người dùng muốn lọc theo StartDate
            if (input.StartDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= input.StartDate.Value);
            }

            // Kiểm tra nếu người dùng muốn lọc theo EndDate
            if (input.EndDate.HasValue)
            {
                query = query.Where(o => o.OrderDate <= input.EndDate.Value);
            }

            // Kiểm tra nếu có giá trị FreeTextSearch, tìm theo ProductName và FullName
            if (!string.IsNullOrEmpty(input.FreeTextSearch))
            {
                query = query.Where(o => o.OrderItems.Any(oi => oi.ProductName.Contains(input.FreeTextSearch))
                                      || o.User.FullName.Contains(input.FreeTextSearch)
                                      || o.TrackingCode.Contains(input.FreeTextSearch));
            }

            return query; // Trả về IQueryable<Order> thay vì Task<List<Order>>
        }


        public virtual async Task<PagedViewModel<Order>> GetPagedOrders(OrderGetRequestInputDto input)
        {
            // Bắt đầu từ truy vấn cơ bản, đảm bảo GetOrderByStatus trả về IQueryable<Order>
            var query = GetOrderByStatus(input);

            // Đếm tổng số bản ghi thỏa mãn điều kiện
            var totalCount = await query.CountAsync();  // Sử dụng CountAsync với IQueryable<Order>

            // Áp dụng phân trang
            query = query.Skip((input.PageIndex - 1) * input.PageSize)  // Skip hoạt động với IQueryable<Order>
                         .Take(input.PageSize);

            // Lấy dữ liệu sau khi phân trang
            var data = await query.ToListAsync();  // ToListAsync hoạt động với IQueryable<Order>

            // Trả về kết quả dưới dạng `PagedViewModel`
            return new PagedViewModel<Order>
            {
                Items = data,
                TotalRecord = totalCount
            };
        }


        public async Task<List<Order>> GetOrderByUserIdAndStatus(OrderGetRequestInputDto input)
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
                    //.Include(o => o.Address)
                    .Include(o => o.User)
                    .Where(o => o.UserId == userId.Value) // Lọc theo UserId của người dùng hiện tại
                    .AsNoTracking();

                query = query.OrderByDescending(o => o.OrderDate);

                // Lọc theo trạng thái đơn hàng nếu có
                if (input.OrderStatus.HasValue)
                {
                    query = query.Where(o => o.OrderStatus == input.OrderStatus.Value);
                }

                // Lọc theo StartDate và EndDate nếu có
                if (input.StartDate.HasValue)
                {
                    query = query.Where(o => o.OrderDate >= input.StartDate.Value);
                }
                if (input.EndDate.HasValue)
                {
                    query = query.Where(o => o.OrderDate <= input.EndDate.Value);
                }

                // Kiểm tra nếu có giá trị FreeTextSearch, tìm theo ProductName và FullName
                if (!string.IsNullOrEmpty(input.FreeTextSearch))
                {
                    query = query.Where(o => o.OrderItems.Any(oi => oi.ProductName.Contains(input.FreeTextSearch))
                                          || o.User.FullName.Contains(input.FreeTextSearch)
                                          || o.TrackingCode.Contains(input.FreeTextSearch));
                }

                // Lấy danh sách đơn hàng sau khi áp dụng các điều kiện lọc
                var orders = await query.ToListAsync();

                // Nếu không tìm thấy đơn hàng, trả về danh sách rỗng
                if (orders == null || !orders.Any())
                {
                    return new List<Order>();
                }

                // Ánh xạ danh sách đơn hàng sang OrderDto và trả về
                return  orders;
                ;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving orders for the current user and status.");
                throw new Exception($"{ex.Message}");
            }
        }

        //log ra lịch sử xuất kho với cartItem

        private string GenerateCodeInventory()
        {
            // Tạo chuỗi với tiền tố "ORD-", sau đó lấy 4 ký tự ngẫu nhiên từ Guid và ghép vào phần thời gian ngắn gọn
            string datePart = DateTime.UtcNow.ToString("MMdd");
            string randomPart = Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper();

            // Kết hợp các phần lại thành mã ngắn gọn
            return $"NH-{datePart}{randomPart}";
        }

        private async Task LogInventoryFromCartItem(CartItem cartItem, Enums.ActionType actionType, string note = "")
        {

             var inventoryLog = new KhanhSkin_BackEnd.Entities.InventoryLog
            {
                ProductId = cartItem.ProductId,
                ProductName = cartItem.Product.ProductName,
                ProductSKU = cartItem.Product.SKU,
                ProductVariantId = cartItem.VariantId,
                VariantName = cartItem.Variant?.NameVariant,
                QuantityChange = actionType == Enums.ActionType.Export ? -cartItem.Amount : cartItem.Amount,
                TransactionDate = DateTime.Now,
                ActionType = actionType,
                 CodeInventory = GenerateCodeInventory(),
                 Note = note
            };

            await _inventoryLogRepository.CreateAsync(inventoryLog);
        }

        private async Task LogInventoryOrderItem(OrderItem orderItem, Enums.ActionType actionType, string note = "")
        {
            var inventoryLog = new KhanhSkin_BackEnd.Entities.InventoryLog
            {
                ProductId = orderItem.ProductId,
                ProductName = orderItem.ProductName,
                ProductVariantId = orderItem.VariantId,
                VariantName = orderItem.NameVariant,
                QuantityChange = actionType == Enums.ActionType.Import ? orderItem.Amount : -orderItem.Amount,
                TransactionDate = DateTime.Now,
                ActionType = actionType,
                CodeInventory = GenerateCodeInventory(),
                Note = note
            };

            await _inventoryLogRepository.CreateAsync(inventoryLog);
        }

    }
}

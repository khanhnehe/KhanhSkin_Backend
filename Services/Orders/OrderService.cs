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
using KhanhSkin_BackEnd.Dtos.Report;
using System.Globalization;
using KhanhSkin_BackEnd.Dtos.Payment;
using KhanhSkin_BackEnd.Services.PaymentVNpay;
using Microsoft.AspNetCore.Http;

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
        private readonly IRepository<KhanhSkin_BackEnd.Entities.InventoryLog> _inventoryLogRepository;
        private readonly IRepository<KhanhSkin_BackEnd.Entities.Address> _addressRepository;
        private readonly IVnPayService _vnPayService;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderService> _logger;
        private readonly ICurrentUser _currentUser;
        private readonly IHttpContextAccessor _httpContextAccessor;

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
           IMapper mapper,
           IVnPayService vnPayService,
           ILogger<OrderService> logger,
           IHttpContextAccessor httpContextAccessor,
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
            _mapper = mapper;
            _vnPayService = vnPayService;
            _logger = logger;
            _currentUser = currentUser;
            _httpContextAccessor = httpContextAccessor;

        }



        public async Task<OrderDto> CheckOutOrder(CheckoutOrderDto input)
        {
            var userId = _currentUser.Id;
            if (userId == null)
            {
                throw new Exception("User not authenticated");
            }

            // Lấy thông tin giỏ hàng
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

            // Lấy thông tin địa chỉ giao hàng
            var address = await _addressRepository.GetAsync(input.AddressId);
            if (address == null || address.UserId != userId)
            {
                throw new Exception("Address not found or does not belong to the current user");
            }

            // Tạo đối tượng Order
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

            // Nếu phương thức thanh toán là VNPay, tạo URL thanh toán nhưng vẫn tạo Order
            if (input.PaymentMethod == PaymentMethod.Vnpay)
            {
                var paymentModel = new PaymentInformationModel
                {
                    OrderType = "other",
                    Amount = (double)order.FinalPrice,
                    OrderDescription = "Thanh toán đơn hàng",
                    Name = _currentUser.FullName ?? "Khách hàng"
                };

                var paymentUrl = _vnPayService.CreatePaymentUrl(paymentModel, _httpContextAccessor.HttpContext);

                // Log URL thanh toán
                _logger.LogInformation("Generated Payment URL: {PaymentUrl}", paymentUrl);

                // Lưu URL thanh toán vào OrderDto để trả về sau
                order.OrderStatus = Enums.OrderStatus.Pending; // Đơn hàng đang chờ thanh toán
            }

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

                if (voucher.TotalUses <= 0)
                {
                    throw new Exception("Voucher này đã được sử dụng hết.");
                }

                voucher.TotalUses--;
                await _voucherRepository.UpdateAsync(voucher);

                order.VoucherId = voucher.Id;
            }

            // Tạo danh sách sản phẩm trong đơn hàng
            var orderItems = cart.CartItems.Select(cartItem => new OrderItem
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
            }).ToList();

            order.OrderItems = orderItems;

            // Cập nhật số lượng sản phẩm và log lịch sử
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
                await LogInventoryFromCartItem(cartItem, Enums.ActionType.Export, "Đặt hàng");
            }

            order.TrackingCode = CreateTrackingCode();

            // Lưu Order vào database
            await _orderRepository.CreateAsync(order);

            // Xóa giỏ hàng sau khi tạo Order
            await _cartRepository.DeleteAsync(cart.Id);

            // Trả về OrderDto
            var orderDto = _mapper.Map<OrderDto>(order);
            orderDto.Address = _mapper.Map<AddressDto>(address);

            orderDto.ShippingMethodDes = order.ShippingMethod.GetDescription();
            orderDto.PaymentMethodDes = order.PaymentMethod.GetDescription();
            orderDto.OrderStatusDes = order.OrderStatus.GetDescription();

            if (input.PaymentMethod == PaymentMethod.Vnpay)
            {
                orderDto.PaymentUrl = _vnPayService.CreatePaymentUrl(new PaymentInformationModel
                {
                    OrderType = "other",
                    Amount = (double)order.FinalPrice,
                    OrderDescription = "Thanh toán đơn hàng",
                    Name = _currentUser.FullName ?? "Khách hàng"
                }, _httpContextAccessor.HttpContext);
            }

            return orderDto;
        }



        private string CreateTrackingCode()
        {
            return $"ORD-{Guid.NewGuid().ToString().ToUpper().Substring(0, 8)}";
        }


        // Thống kê doanh thu và lợi nhuận
        public async Task<List<ReportResponseDto>> GetRevenueAndProfitReports(ReportRequestDto request)
        {
            var (startDate, endDate) = CalculateDateRange(request);

            // Lấy danh sách các đơn hàng đã hoàn thành trong khoảng thời gian đã chọn
            var orders = await _orderRepository.AsQueryable()
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate && o.OrderStatus == OrderStatus.Completed)
                .Include(o => o.OrderItems) 
                .ToListAsync();

            // Lấy danh sách các ProductId và VariantId từ các đơn hàng
            var productIds = orders.SelectMany(o => o.OrderItems.Select(i => i.ProductId)).Distinct(); 
            var variantIds = orders.SelectMany(o => o.OrderItems.Select(i => i.VariantId)).Distinct(); 

            // Lấy danh sách InventoryLog liên quan đến các sản phẩm và biến thể từ các đơn hàng (chỉ bao gồm hành động nhập kho - FIFO)
            var inventoryLogs = await _inventoryLogRepository.AsQueryable()
                .Where(log => productIds.Contains(log.ProductId) &&
                              variantIds.Contains(log.ProductVariantId) &&
                              log.ActionType == ActionType.Import && // Chỉ tính log nhập kho
                              log.QuantityChange > 0) // Chỉ quan tâm đến số lượng tăng thêm
                .OrderBy(log => log.TransactionDate) // Sắp xếp theo ngày nhập (FIFO)
                .ToListAsync();

            // Nhóm dữ liệu đơn hàng theo PeriodType (ngày, tháng, năm)
            var groupedData = orders
                .GroupBy(o =>
                {
                    return request.PeriodType switch
                    {
                        // Nhóm theo ngày
                        PeriodType.Day => o.OrderDate.Date.ToString("yyyy-MM-dd"),

                        // Nhóm theo tháng (mặc định nhóm theo từng ngày trong tháng)
                        PeriodType.Month => o.OrderDate.Date.ToString("yyyy-MM-dd"),

                        // Nhóm theo năm (dựa trên từng tháng trong năm)
                        PeriodType.Year => $"{o.OrderDate.Month}/{o.OrderDate.Year}",

                        // Xử lý lỗi nếu PeriodType không hợp lệ
                        _ => throw new Exception("PeriodType không hợp lệ.")
                    };
                })
                .Select(group => new ReportResponseDto
                {
                    // Dữ liệu cho từng nhóm
                    TimePeriod = group.Key, // Khoảng thời gian (ngày, tháng, năm)
                    Revenue = group.Sum(o => o.FinalPrice - o.ShippingPrice), // Doanh thu (giá cuối trừ phí vận chuyển)
                    GrossProfit = group.Sum(o => CalculateGrossProfit(o, inventoryLogs)) // Lợi nhuận gộp (dựa trên FIFO)
                })
                .ToList();

            // Bổ sung dữ liệu cho đủ các ngày trong tháng nếu PeriodType là Month
            if (request.PeriodType == PeriodType.Month)
            {
                var currentYear = DateTime.UtcNow.Year;
                var selectedMonth = request.SelectedMonth ?? DateTime.UtcNow.Month;
                var daysInMonth = DateTime.DaysInMonth(currentYear, selectedMonth); // Số ngày trong tháng được chọn

                // Tạo dữ liệu cho tất cả các ngày trong tháng
                var fullMonthData = Enumerable.Range(1, daysInMonth)
                    .Select(day => new ReportResponseDto
                    {
                        TimePeriod = new DateTime(currentYear, selectedMonth, day).ToString("yyyy-MM-dd"),
                        Revenue = groupedData.FirstOrDefault(g => g.TimePeriod == new DateTime(currentYear, selectedMonth, day).ToString("yyyy-MM-dd"))?.Revenue ?? 0,
                        GrossProfit = groupedData.FirstOrDefault(g => g.TimePeriod == new DateTime(currentYear, selectedMonth, day).ToString("yyyy-MM-dd"))?.GrossProfit ?? 0,
                        GrowthRate = null
                    })
                    .ToList();

                groupedData = fullMonthData;
            }
            // Bổ sung dữ liệu cho đủ 12 tháng nếu PeriodType là Year
            else if (request.PeriodType == PeriodType.Year)
            {
                var currentYear = DateTime.UtcNow.Year;

                // Tạo dữ liệu cho tất cả 12 tháng trong năm
                var fullYearData = Enumerable.Range(1, 12)
                    .Select(month => new ReportResponseDto
                    {
                        TimePeriod = $"{month:D2}/{currentYear}", // Tháng/Năm
                        Revenue = groupedData.FirstOrDefault(g => g.TimePeriod == $"{month}/{currentYear}")?.Revenue ?? 0,
                        GrossProfit = groupedData.FirstOrDefault(g => g.TimePeriod == $"{month}/{currentYear}")?.GrossProfit ?? 0,
                        GrowthRate = null
                    })
                    .ToList();

                groupedData = fullYearData;
            }

            // Tính tỷ lệ tăng trưởng theo thời gian
            for (int i = 1; i < groupedData.Count; i++)
            {
                var current = groupedData[i];
                var previous = groupedData[i - 1];

                // Tỷ lệ tăng trưởng (%): ((Doanh thu hiện tại - Doanh thu kỳ trước) / Doanh thu kỳ trước) * 100
                current.GrowthRate = previous.Revenue > 0
                    ? ((current.Revenue - previous.Revenue) / previous.Revenue) * 100
                    : null;
            }

            // Đảm bảo kỳ đầu tiên không có tỷ lệ tăng trưởng
            if (groupedData.Count > 0)
            {
                groupedData[0].GrowthRate = null;
            }

            // Trả về dữ liệu thống kê
            return groupedData;
        }


        private (DateTime StartDate, DateTime EndDate) CalculateDateRange(ReportRequestDto request)
        {
            var currentYear = DateTime.UtcNow.Year;

            switch (request.PeriodType)
            {
                case Enums.PeriodType.Day:
                    // Thống kê ngày hiện tại
                    var today = DateTime.Now.Date;
                    var timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                    var localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone).Date;
                    return (localTime, localTime.AddDays(1).AddTicks(-1));

                case Enums.PeriodType.Month:
                    // Kiểm tra SelectedMonth
                    if (!request.SelectedMonth.HasValue || request.SelectedMonth < 1 || request.SelectedMonth > 12)
                    {
                        throw new Exception("SelectedMonth phải nằm trong khoảng từ 1 đến 12.");
                    }

                    // Thống kê tháng được chọn
                    var startOfMonth = new DateTime(currentYear, request.SelectedMonth.Value, 1);
                    var endOfMonth = startOfMonth.AddMonths(1).AddTicks(-1);
                    return (startOfMonth, endOfMonth);

                case Enums.PeriodType.Year:
                    // Thống kê toàn bộ năm hiện tại
                    var startOfYear = new DateTime(currentYear, 1, 1);
                    var endOfYear = new DateTime(currentYear, 12, 31, 23, 59, 59);
                    return (startOfYear, endOfYear);

                default:
                    throw new Exception("PeriodType không hợp lệ.");
            }
        }

        // Phương thức để tính lợi nhuận gộp cho một đơn hàng
        private decimal CalculateGrossProfit(Order order, List<KhanhSkin_BackEnd.Entities.InventoryLog> inventoryLogs)
        {
            decimal totalProfit = 0; // Biến tổng lợi nhuận gộp cho toàn bộ đơn hàng

            foreach (var item in order.OrderItems) // Duyệt qua từng sản phẩm trong đơn hàng
            {
                int remainingQuantity = item.Amount; // Số lượng cần bán của sản phẩm hiện tại

                // Lọc các log nhập kho liên quan đến sản phẩm/biến thể hiện tại
                var logsForItem = inventoryLogs
                    .Where(log => log.ProductId == item.ProductId &&
                                  log.ProductVariantId == item.VariantId)
                    .ToList();

                // Nếu không tìm thấy log nhập kho nào liên quan
                if (!logsForItem.Any())
                {
                    // Hiển thị thông báo lỗi và bỏ qua sản phẩm này
                    Console.WriteLine($"Không tìm thấy dữ liệu nhập kho cho sản phẩm {item.ProductName}.");
                    continue; // Chuyển sang sản phẩm tiếp theo
                }

                decimal totalCost = 0; // Tổng giá vốn (cost price) của sản phẩm trong đơn hàng

                foreach (var log in logsForItem) // Duyệt qua từng log nhập kho liên quan đến sản phẩm
                {
                    if (remainingQuantity > log.QuantityChange) // Nếu số lượng cần bán lớn hơn số lượng trong log hiện tại
                    {
                        // Tính giá vốn dựa trên toàn bộ số lượng trong log hiện tại
                        totalCost += log.CostPrice.GetValueOrDefault() * log.QuantityChange;

                        // Giảm số lượng cần bán
                        remainingQuantity -= log.QuantityChange;
                    }
                    else // Nếu số lượng trong log đủ để đáp ứng số lượng cần bán
                    {
                        // Tính giá vốn chỉ dựa trên số lượng còn lại cần bán
                        totalCost += log.CostPrice.GetValueOrDefault() * remainingQuantity;

                        // Đặt số lượng còn lại cần bán về 0 (đã đủ số lượng)
                        remainingQuantity = 0;
                        break; // Thoát khỏi vòng lặp
                    }
                }

                // Nếu vẫn còn số lượng cần bán mà không đủ tồn kho để đáp ứng
                if (remainingQuantity > 0)
                {
                    // Hiển thị thông báo lỗi và bỏ qua sản phẩm này
                    Console.WriteLine($"Tồn kho không đủ cho sản phẩm {item.ProductName}, thiếu {remainingQuantity}.");
                    continue; // Chuyển sang sản phẩm tiếp theo
                }

                // Tính lợi nhuận của sản phẩm: Giá bán - Tổng giá vốn
                var itemProfit = item.ItemsPrice - totalCost;

                // Cộng lợi nhuận của sản phẩm vào tổng lợi nhuận
                totalProfit += itemProfit;
            }

            return totalProfit; // Trả về tổng lợi nhuận gộp của đơn hàng
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
                await LogInventoryOrderItem(orderItem, Enums.ActionType.CancelProduct, "Hủy đơn");
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



        /// <summary>
        /// ////////
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// 



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


        // cho admin
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

        // cho từng người dùng ---------------------------
        public IQueryable<Order> GetOrderByStatusUser(OrderGetRequestInputDto input)

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


        public virtual async Task<PagedViewModel<Order>> GetPagedOrderUser(OrderGetRequestInputDto input)
        {

            // Bắt đầu từ truy vấn cơ bản, thêm điều kiện lọc theo userId
            var query = GetOrderByUserIdAndStatus(input);

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



        public IQueryable<Order> GetOrderByUserIdAndStatus(OrderGetRequestInputDto input)
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

                return query;

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
                QuantityChange = actionType == Enums.ActionType.CancelProduct ? orderItem.Amount : -orderItem.Amount,
                TransactionDate = DateTime.Now,
                ActionType = actionType,
                CodeInventory = GenerateCodeInventory(),
                Note = note
            };

            await _inventoryLogRepository.CreateAsync(inventoryLog);
        }


        //////
        ///
        // Thống kê doanh thu và lợi nhuận


        public async Task<List<RecommendationDto>> GetTopSellingProductsAsync()
        {
            // Lấy danh sách top 10 sản phẩm bán chạy nhất
            var topSellingProducts = await _productRepository.AsQueryable()
                .OrderByDescending(p => p.Purchases) // Sắp xếp giảm dần theo số lượng bán
                .Take(10) // Lấy top 10 sản phẩm
                .ToListAsync();

            // Ánh xạ danh sách Product sang ProductDto
            var result = _mapper.Map<List<RecommendationDto>>(topSellingProducts);

            return result;
        }

        // thống kê đơn hàng trong ngày
        public async Task<int> CountOrderToday()
        {
            // Lấy ngày hiện tại
            var today = DateTime.Today;

            // Đếm số đơn hàng có OrderDate trong ngày hiện tại
            var count = await _orderRepository.AsQueryable()
                .Where(o => o.OrderDate.Date == today)
                .CountAsync();

            return count;
        }


    }
}

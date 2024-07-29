using AutoMapper;
using KhanhSkin_BackEnd.Dtos.Cart;
using KhanhSkin_BackEnd.Entities;
using KhanhSkin_BackEnd.Repositories;
using KhanhSkin_BackEnd.Services.CurrentUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KhanhSkin_BackEnd.Services.Carts
{
    public class CartService : BaseService<KhanhSkin_BackEnd.Entities.Cart, CartDto, AddProductToCartDto, CartGetRequestInputDto>
    {
        private readonly IConfiguration _config;
        private readonly IRepository<KhanhSkin_BackEnd.Entities.Cart> _cartRepository;
        private readonly IRepository<CartItem> _cartItemRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductVariant> _productVariantRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CartService> _logger;
        private readonly ICurrentUser _currentUser;

        public CartService(
            IConfiguration config,
            IRepository<KhanhSkin_BackEnd.Entities.Cart> cartRepository,
            IRepository<CartItem> cartItemRepository,
            IRepository<ProductVariant> productVariantRepository,
            IRepository<Product> productRepository,
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
            _mapper = mapper;
            _logger = logger;
            _currentUser = currentUser;
        }

        public async Task<CartDto> AddProductToCart(AddProductToCartDto input)
        {
            try
            {
                var userId = _currentUser.Id;
                if (userId == null)
                {
                    throw new Exception("User not authenticated");
                }

                if (input.AmountAdd < 1)
                {
                    throw new Exception("Số lượng sản phẩm được thêm phải lớn hơn hoặc bằng 1");
                }

                // Tìm giỏ hàng của người dùng hiện tại
                var cart = await _cartRepository.FirstOrDefault(c => c.UserId == userId);

                if (cart == null)
                {
                    cart = new Cart
                    {
                        UserId = userId.Value,
                        TotalPrice = 0
                    };
                    await _cartRepository.CreateAsync(cart);
                }

                var product = await _productRepository.GetAsync(input.ProductId);
                if (product == null)
                {
                    throw new Exception("Product not found");
                }

                if (input.VariantId.HasValue)
                {
                    var variant = await _productVariantRepository.GetAsync(input.VariantId.Value);
                    if (variant == null)
                    {
                        throw new Exception("Variant not found");
                    }

                    if (input.AmountAdd > variant.QuantityVariant)
                    {
                        throw new Exception("Số sản phẩm được thêm vượt quá giới hạn tồn kho");
                    }

                    await AddOrUpdateCartItem(cart, product, variant, input.AmountAdd);
                }
                else
                {
                    if (product.Variants != null && product.Variants.Any())
                    {
                        throw new Exception("Product has variants. Please select a variant.");
                    }

                    if (input.AmountAdd > product.Quantity)
                    {
                        throw new Exception("Số sản phẩm được thêm vượt quá giới hạn tồn kho");
                    }

                    await AddOrUpdateCartItem(cart, product, null, input.AmountAdd);
                }

                // Cập nhật tổng giá trị của giỏ hàng
                cart.TotalPrice = cart.CartItems.Sum(ci => ci.ItemsPrice);
                await _cartRepository.UpdateAsync(cart);

                var cartDto = _mapper.Map<CartDto>(cart);
                return cartDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding product to cart");
                throw new Exception($"Có lỗi xảy ra: {ex.Message}");
            }
        }

        private async Task AddOrUpdateCartItem(Cart cart, Product product, ProductVariant variant, int amountAdd)
        {
            CartItem existingCartItem = null;

            if (variant != null)
            {
                existingCartItem = await _cartItemRepository.FirstOrDefault(ci => ci.CartId == cart.Id && ci.ProductId == product.Id && ci.VariantId == variant.Id);

                if (existingCartItem != null)
                {
                    if (existingCartItem.Amount + amountAdd > variant.QuantityVariant)
                    {
                        throw new Exception("Bạn đã có sản phẩm trong giỏ hàng. Số sản phẩm được thêm vượt quá giới hạn tồn kho");
                    }

                    existingCartItem.Amount += amountAdd;
                    existingCartItem.ItemsPrice = existingCartItem.Amount * (variant.SalePriceVariant ?? variant.PriceVariant);
                    await _cartItemRepository.UpdateAsync(existingCartItem);
                }
                else
                {
                    existingCartItem = new CartItem
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
                    await _cartItemRepository.CreateAsync(existingCartItem);
                }
            }
            else
            {
                existingCartItem = await _cartItemRepository.FirstOrDefault(ci => ci.CartId == cart.Id && ci.ProductId == product.Id && ci.VariantId == null);

                if (existingCartItem != null)
                {
                    if (existingCartItem.Amount + amountAdd > product.Quantity)
                    {
                        throw new Exception("Bạn đã có sản phẩm trong giỏ hàng. Số sản phẩm được thêm vượt quá giới hạn tồn kho");
                    }

                    existingCartItem.Amount += amountAdd;
                    existingCartItem.ItemsPrice = existingCartItem.Amount * (product.SalePrice ?? product.Price);
                    await _cartItemRepository.UpdateAsync(existingCartItem);
                }
                else
                {
                    existingCartItem = new CartItem
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
                    await _cartItemRepository.CreateAsync(existingCartItem);
                }
            }
        }

        public async Task<CartDto> GetCartByUserId(Guid userId)
        {
            try
            {
                var cart = await _cartRepository
                    .AsQueryable()
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    throw new Exception("Cart not found for the specified user.");
                }

                var cartDto = _mapper.Map<CartDto>(cart);
                return cartDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the cart for user ID: {UserId}", userId);
                throw new Exception($"An error occurred: {ex.Message}");
            }
        }
    }
}

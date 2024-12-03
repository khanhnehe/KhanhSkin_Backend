using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace KhanhSkin_BackEnd.Entities
{
    public class AppDbContext : DbContext
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<ProductType> ProductTypes { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<ProductVoucher> ProductVouchers { get; set; } // Thêm DbSet cho ProductVoucher
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<InventoryLog> InventoryLogs { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    "Data Source=LAPTOP-316GAGPG\\SQLEXPRESS; Initial Catalog=KhanhSkin; User id=khanhhe; Password=123456;MultipleActiveResultSets=True;Connection Timeout=120;TrustServerCertificate=True",
                    sqlOptions => sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                );
            }
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Product>()
                .Property(p => p.SalePrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Product>()
                .Property(p => p.AverageRating)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ProductVariant>()
                .Property(pv => pv.PriceVariant)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ProductVariant>()
                .Property(pv => pv.SalePriceVariant)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Cart>()
                .Property(c => c.TotalPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.ProductPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.ProductSalePrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.ItemsPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Voucher>()
                .Property(v => v.MinimumOrderValue)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Voucher>()
                .Property(v => v.DiscountValue)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Cart>()
           .Property(c => c.DiscountValue)
           .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Cart>()
           .Property(c => c.FinalPrice)
            .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
           .Property(o => o.DiscountValue)
           .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.FinalPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.ShippingPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.ItemsPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.ProductPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.ProductSalePrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<InventoryLog>()
                   .Property(il => il.CostPrice)
                   .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<InventoryLog>()
                .Property(il => il.ItemPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<InventoryLog>()
                .Property(il => il.TotalPrice)
                .HasColumnType("decimal(18,2)");

            // TL FK và quan hệ 1-n giữa Product và ProductVariant
            modelBuilder.Entity<Product>()
                .HasMany(p => p.Variants)
                .WithOne(v => v.Product)
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // TL FK và quan hệ 1-n giữa Product và Review
            modelBuilder.Entity<Product>()
                .HasMany(p => p.Reviews)
                .WithOne(r => r.Product)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Reviews)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductVariant>()
             .HasMany(v => v.Reviews)
             .WithOne(r => r.Variant)
             .HasForeignKey(r => r.VariantId)
             .OnDelete(DeleteBehavior.NoAction);


            // Thiết lập quan hệ 1-n giữa Order và Review
            modelBuilder.Entity<Review>()
               .HasOne(r => r.Order) // Thiết lập Order là khóa ngoại trong Review
               .WithMany() // Không cần thêm Reviews vào Order
               .HasForeignKey(r => r.OrderId)
               .OnDelete(DeleteBehavior.SetNull);

            // TL FK và quan hệ 1-n giữa User và Favorite
            modelBuilder.Entity<User>()
                .HasMany(u => u.Favorites)
                .WithOne(f => f.User)
                .HasForeignKey(f => f.UserId);

            // TL FK và quan hệ 1-n giữa Product và Favorite
            modelBuilder.Entity<Product>()
                .HasMany(p => p.Favorites)
                .WithOne(f => f.Product)
                .HasForeignKey(f => f.ProductId);

            // TL bảng nối nhiều-nhiều giữa ProductType và Category
            modelBuilder.Entity<ProductType>()
                .HasMany(pt => pt.Categories)
                .WithMany(c => c.ProductTypes)
                .UsingEntity<Dictionary<string, object>>(
                    "ProductTypeCategory",
                    j => j
                        .HasOne<Category>()
                        .WithMany()
                        .HasForeignKey("CategoryId")
                        .HasConstraintName("FK_ProductTypeCategory_Categories_CategoryId")
                        .OnDelete(DeleteBehavior.Cascade),
                    j => j
                        .HasOne<ProductType>()
                        .WithMany()
                        .HasForeignKey("ProductTypeId")
                        .HasConstraintName("FK_ProductTypeCategory_ProductTypes_ProductTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                );

            // TL bảng nối nhiều-nhiều giữa Product và Category
            modelBuilder.Entity<Product>()
                .HasMany(p => p.Categories)
                .WithMany(c => c.Products)
                .UsingEntity<Dictionary<string, object>>(
                    "ProductCategory",
                    j => j
                        .HasOne<Category>()
                        .WithMany()
                        .HasForeignKey("CategoryId")
                        .HasConstraintName("FK_ProductCategory_Categories_CategoryId")
                        .OnDelete(DeleteBehavior.Cascade),
                    j => j
                        .HasOne<Product>()
                        .WithMany()
                        .HasForeignKey("ProductId")
                        .HasConstraintName("FK_ProductCategory_Products_ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                );

            // TL bảng nối nhiều-nhiều giữa Product và ProductType
            modelBuilder.Entity<Product>()
                .HasMany(p => p.ProductTypes)
                .WithMany(pt => pt.Products)
                .UsingEntity<Dictionary<string, object>>(
                    "ProductProductType",
                    j => j
                        .HasOne<ProductType>()
                        .WithMany()
                        .HasForeignKey("ProductTypeId") // Sử dụng ProductTypeId thay vì ProductTypesId
                        .HasConstraintName("FK_ProductProductType_ProductTypes_ProductTypeId")
                        .OnDelete(DeleteBehavior.Cascade),
                    j => j
                        .HasOne<Product>()
                        .WithMany()
                        .HasForeignKey("ProductId") // Sử dụng ProductId thay vì ProductsId
                        .HasConstraintName("FK_ProductProductType_Products_ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                );

            // TL bảng nối nhiều-nhiều giữa Product và Voucher
            modelBuilder.Entity<ProductVoucher>()
                .HasKey(pv => new { pv.ProductId, pv.VoucherId });

            modelBuilder.Entity<ProductVoucher>()
                .HasOne(pv => pv.Product)
                .WithMany(p => p.ProductVouchers)
                .HasForeignKey(pv => pv.ProductId);

            modelBuilder.Entity<ProductVoucher>()
                .HasOne(pv => pv.Voucher)
                .WithMany(v => v.ProductVouchers)
                .HasForeignKey(pv => pv.VoucherId);

            // quan hệ 1-n giữa Brand và Product
            modelBuilder.Entity<Brand>()
             .HasMany(b => b.Products)
             .WithOne(p => p.Brand)
             .HasForeignKey(p => p.BrandId)
             .OnDelete(DeleteBehavior.NoAction);


            // quan hệ 1-1 giữa User và Cart
            modelBuilder.Entity<User>()
                .HasOne(u => u.Cart)
                .WithOne(c => c.User)
                .HasForeignKey<Cart>(c => c.UserId);

            // quan hệ 1-n giữa Cart và CartItem
            modelBuilder.Entity<Cart>()
                .HasMany(c => c.CartItems)
                .WithOne(ci => ci.Cart)
                .HasForeignKey(ci => ci.CartId);

            // quan hệ 1-n giữa Product và CartItem
            modelBuilder.Entity<Product>()
                .HasMany(p => p.CartItems)
                .WithOne(ci => ci.Product)
                .HasForeignKey(ci => ci.ProductId);

            // quan hệ 1-n giữa ProductVariant và CartItem
            modelBuilder.Entity<ProductVariant>()
                .HasMany(v => v.CartItems)
                .WithOne(ci => ci.Variant)
                .HasForeignKey(ci => ci.VariantId);

            // TL chỉ số unique cho CartItem
            modelBuilder.Entity<CartItem>()
                .HasIndex(ci => new { ci.CartId, ci.ProductId, ci.VariantId })
                .IsUnique();



            // Thiết lập quan hệ 1-n giữa Voucher và Cart
            modelBuilder.Entity<Voucher>()
                .HasMany(v => v.Carts)
                .WithOne(c => c.Voucher)
                .HasForeignKey(c => c.VoucherId)
                .OnDelete(DeleteBehavior.SetNull); // Hoặc DeleteBehavior.NoAction tùy thuộc vào yêu cầu

            modelBuilder.Entity<Address>()
             .HasOne(a => a.User)
             .WithMany(u => u.Addresses) // Một người dùng có thể có nhiều địa chỉ
             .HasForeignKey(a => a.UserId)
             .OnDelete(DeleteBehavior.Cascade); // Xóa địa chỉ nếu người dùng bị xóa

          

            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany() // Điều này ngăn chặn multiple cascade paths
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.NoAction); // Hoặc bạn có thể dùng DeleteBehavior.SetNull

            modelBuilder.Entity<OrderItem>()
                 .HasOne(oi => oi.Order)
                 .WithMany(o => o.OrderItems)
                 .HasForeignKey(oi => oi.OrderId)
                 .OnDelete(DeleteBehavior.Cascade); // Hoặc SetNull tùy thuộc vào logic của bạn

       //     modelBuilder.Entity<OrderItem>()
       //.HasOne(oi => oi.Variant)
       //.WithMany(v => v.OrderItems)
       //.HasForeignKey(oi => oi.VariantId)
       //.OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Order>()
                 .HasOne(o => o.Voucher)
                 .WithMany()
                 .HasForeignKey(o => o.VoucherId)
                 .OnDelete(DeleteBehavior.NoAction);

            //modelBuilder.Entity<OrderItem>()
            //    .HasOne(oi => oi.CartItem)
            //    .WithMany()
            //    .HasForeignKey(oi => oi.CartItemId)
            //    .OnDelete(DeleteBehavior.NoAction);


            modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Product)
            .WithMany(p => p.OrderItems)
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.SetNull);



            //modelBuilder.Entity<Order>()
            //    .HasOne(o => o.Cart)
            //    .WithMany()
            //    .HasForeignKey(o => o.CartId)
            //    .OnDelete(DeleteBehavior.NoAction);


        }
    }
}

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
        public DbSet<VoucherActivity> VoucherActivity { get; set; }
        public DbSet<ProductVoucher> ProductVouchers { get; set; } // Thêm DbSet cho ProductVoucher

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // TL kiểu dữ liệu cho các thuộc tính decimal trong Product
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Product>()
                .Property(p => p.SalePrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Product>()
                .Property(p => p.AverageRating)
                .HasColumnType("decimal(18,2)");

            // TL kiểu dữ liệu cho các thuộc tính decimal trong ProductVariant
            modelBuilder.Entity<ProductVariant>()
                .Property(pv => pv.PriceVariant)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ProductVariant>()
                .Property(pv => pv.SalePriceVariant)
                .HasColumnType("decimal(18,2)");

            // TL kiểu dữ liệu cho các thuộc tính decimal trong Cart
            modelBuilder.Entity<Cart>()
                .Property(c => c.TotalPrice)
                .HasColumnType("decimal(18,2)");

            // TL kiểu dữ liệu cho các thuộc tính decimal trong CartItem
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

            // Cấu hình cho thuộc tính DiscountValue
            modelBuilder.Entity<Voucher>()
                .Property(v => v.DiscountValue)
                .HasColumnType("decimal(18,2)"); // Chỉ định 

            // TL FK và quan hệ 1-n giữa Product và ProductVariant
            modelBuilder.Entity<Product>()
                .HasMany(p => p.Variants)
                .WithOne(v => v.Product)
                .HasForeignKey(v => v.ProductId);

            // TL FK và quan hệ 1-n giữa Product và Review
            modelBuilder.Entity<Product>()
                .HasMany(p => p.Reviews)
                .WithOne(r => r.Product)
                .HasForeignKey(r => r.ProductId);

            // TL FK và quan hệ 1-n giữa User và Review
            modelBuilder.Entity<User>()
                .HasMany(u => u.Reviews)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId);

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
                .HasForeignKey(p => p.BrandId);

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

            // Thiết lập quan hệ 1 - n giữa user và userVoucher
            modelBuilder.Entity<UserVoucher>()
                .HasOne(uv => uv.User)
                .WithMany(u => u.UserVouchers)
                .HasForeignKey(uv => uv.UserId);

            // TL quan hệ 1 - n giữa Voucher và UserVoucher
            modelBuilder.Entity<UserVoucher>()
                .HasOne(uv => uv.Voucher)
                .WithMany(v => v.UserVouchers)
                .HasForeignKey(uv => uv.VoucherId);

            // Thiết lập quan hệ giữa User, Voucher và VoucherActivity
            //modelBuilder.Entity<VoucherActivity>()
            //    .HasOne(va => va.User)
            //    .WithMany(u => u.VoucherActivities)
            //    .HasForeignKey(va => va.UserId);

            //modelBuilder.Entity<VoucherActivity>()
            //    .HasOne(va => va.Voucher)
            //    .WithMany(v => v.VoucherActivities)
            //    .HasForeignKey(va => va.VoucherId);
        }
    }
}

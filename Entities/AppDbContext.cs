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

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Thiết lập kiểu dữ liệu cho các thuộc tính decimal trong Product
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Product>()
                .Property(p => p.SalePrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Product>()
                .Property(p => p.AverageRating)
                .HasColumnType("decimal(18,2)");

            // Thiết lập kiểu dữ liệu cho các thuộc tính decimal trong ProductVariant
            modelBuilder.Entity<ProductVariant>()
                .Property(pv => pv.PriceVariant)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ProductVariant>()
                .Property(pv => pv.SalePriceVariant)
                .HasColumnType("decimal(18,2)");

            // Thiết lập kiểu dữ liệu cho các thuộc tính decimal trong Cart
            modelBuilder.Entity<Cart>()
                .Property(c => c.TotalPrice)
                .HasColumnType("decimal(18,2)");

            // Thiết lập kiểu dữ liệu cho các thuộc tính decimal trong CartItem
            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.ProductPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.ProductSalePrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.ItemsPrice)
                .HasColumnType("decimal(18,2)");

            // Thiết lập khóa ngoại và quan hệ 1-n giữa Product và ProductVariant
            modelBuilder.Entity<Product>()
                .HasMany(p => p.Variants)
                .WithOne(v => v.Product)
                .HasForeignKey(v => v.ProductId);

            // Thiết lập khóa ngoại và quan hệ 1-n giữa Product và Review
            modelBuilder.Entity<Product>()
                .HasMany(p => p.Reviews)
                .WithOne(r => r.Product)
                .HasForeignKey(r => r.ProductId);

            // Thiết lập khóa ngoại và quan hệ 1-n giữa User và Review
            modelBuilder.Entity<User>()
                .HasMany(u => u.Reviews)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId);

            // Thiết lập khóa ngoại và quan hệ 1-n giữa User và Favorite
            modelBuilder.Entity<User>()
                .HasMany(u => u.Favorites)
                .WithOne(f => f.User)
                .HasForeignKey(f => f.UserId);

            // Thiết lập khóa ngoại và quan hệ 1-n giữa Product và Favorite
            modelBuilder.Entity<Product>()
                .HasMany(p => p.Favorites)
                .WithOne(f => f.Product)
                .HasForeignKey(f => f.ProductId);

            // Thiết lập bảng nối nhiều-nhiều giữa ProductType và Category
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

            // Thiết lập bảng nối nhiều-nhiều giữa Product và Category
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

            // Thiết lập bảng nối nhiều-nhiều giữa Product và ProductType
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

            // Thiết lập mối quan hệ 1-n giữa Brand và Product
            modelBuilder.Entity<Brand>()
                .HasMany(b => b.Products)
                .WithOne(p => p.Brand)
                .HasForeignKey(p => p.BrandId);

            // Thiết lập mối quan hệ 1-1 giữa User và Cart
            modelBuilder.Entity<User>()
                .HasOne(u => u.Cart)
                .WithOne(c => c.User)
                .HasForeignKey<Cart>(c => c.UserId);

            // Thiết lập mối quan hệ 1-n giữa Cart và CartItem
            modelBuilder.Entity<Cart>()
                .HasMany(c => c.CartItems)
                .WithOne(ci => ci.Cart)
                .HasForeignKey(ci => ci.CartId);

            // Thiết lập mối quan hệ 1-n giữa Product và CartItem
            modelBuilder.Entity<Product>()
                .HasMany(p => p.CartItems)
                .WithOne(ci => ci.Product)
                .HasForeignKey(ci => ci.ProductId);

            // Thiết lập mối quan hệ 1-n giữa ProductVariant và CartItem
            modelBuilder.Entity<ProductVariant>()
                .HasMany(v => v.CartItems)
                .WithOne(ci => ci.Variant)
                .HasForeignKey(ci => ci.VariantId);

            // Thiết lập chỉ số unique cho CartItem
            modelBuilder.Entity<CartItem>()
                .HasIndex(ci => new { ci.CartId, ci.ProductId, ci.VariantId })
                .IsUnique();
        }
    }
}

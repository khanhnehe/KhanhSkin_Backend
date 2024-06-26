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
        public DbSet<User> Users { get; set; } // Bổ sung DbSet cho User

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Thiết lập kiểu dữ liệu cho các thuộc tính decimal
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

            // Thiết lập khóa ngoại và quan hệ giữa Product và ProductVariant
            modelBuilder.Entity<Product>()
                .HasMany(p => p.Variants)
                .WithOne(v => v.Product)
                .HasForeignKey(v => v.ProductId);

            // Thiết lập khóa ngoại và quan hệ giữa Product và Review
            modelBuilder.Entity<Product>()
                .HasMany(p => p.Reviews)
                .WithOne(r => r.Product)
                .HasForeignKey(r => r.ProductId);

            // Thiết lập khóa ngoại và quan hệ giữa Review và ProductVariant
            modelBuilder.Entity<Review>()
                .HasOne(r => r.ProductVariant)
                .WithMany()
                .HasForeignKey(r => r.ProductVariantId);

            // Thiết lập khóa ngoại và quan hệ giữa Category và ProductType
            modelBuilder.Entity<Category>()
                .HasMany(c => c.ProductTypes)
                .WithOne(t => t.Category)
                .HasForeignKey(t => t.CategoryId);

            // Thiết lập bảng nối giữa Product và Category
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

            // Thiết lập bảng nối giữa Product và ProductType
            modelBuilder.Entity<Product>()
                .HasMany(p => p.ProductTypes)
                .WithMany(t => t.Products)
                .UsingEntity<Dictionary<string, object>>(
                    "ProductProductType",
                    j => j
                        .HasOne<ProductType>()
                        .WithMany()
                        .HasForeignKey("ProductTypeId")
                        .HasConstraintName("FK_ProductProductType_ProductTypes_ProductTypeId")
                        .OnDelete(DeleteBehavior.Cascade),
                    j => j
                        .HasOne<Product>()
                        .WithMany()
                        .HasForeignKey("ProductId")
                        .HasConstraintName("FK_ProductProductType_Products_ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                );
        }
    }
}

﻿using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using KhanhSkin_BackEnd.Entities;
using KhanhSkin_BackEnd.Repositories;
using KhanhSkin_BackEnd.Services.Users;
using KhanhSkin_BackEnd.Services.CurrentUser;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using AutoWrapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using KhanhSkin_BackEnd.Services.Brands;
using KhanhSkin_BackEnd.Services.ProductTypes;
using KhanhSkin_BackEnd.Services.Categories;
using KhanhSkin_BackEnd.Services.Products;
using System.Text.Json.Serialization;
using KhanhSkin_BackEnd.Services.ProductVariants;
using KhanhSkin_BackEnd.Services.Carts;
using System.Security.Claims;
using KhanhSkin_BackEnd.Services.Voucher;
using KhanhSkin_BackEnd.Services.Address;
using KhanhSkin_BackEnd.Services.Orders;
using Microsoft.Extensions.Configuration;
using CloudinaryDotNet;
using KhanhSkin_BackEnd.Services;
using KhanhSkin_BackEnd.Services.Reviews;
using KhanhSkin_BackEnd.Services.Suppliers;
using KhanhSkin_BackEnd.Services.InventoryLog;
using KhanhSkin_BackEnd.Services.InventoryLogs;
using KhanhSkin_BackEnd.Dtos.Payment;
using KhanhSkin_BackEnd.Services.PaymentVNpay;

var builder = WebApplication.CreateBuilder(args);

// Đọc cấu hình JWT từ appsettings.json
var jwtSettings = builder.Configuration.GetSection("Jwt");

// Cấu hình Cloudinary từ appsettings.json
var cloudinarySettings = builder.Configuration.GetSection("Cloudinary");
var cloudinaryAccount = new Account(
    cloudinarySettings["CloudName"], // Cloud Name
    cloudinarySettings["ApiKey"],    // API Key
    cloudinarySettings["ApiSecret"]  // API Secret
);

var cloudinary = new Cloudinary(cloudinaryAccount);
builder.Services.AddSingleton(cloudinary);
builder.Services.AddSingleton<ICloudinary>(cloudinary);


// Đăng ký PaymentSetting từ appsettings.json
//builder.Services.Configure<PaymentSetting>(builder.Configuration.GetSection("Vnpay"));



// Cấu hình DbContext sử dụng chuỗi kết nối từ appsettings
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery); // Thêm dòng này
        }));


// Đăng ký IHttpContextAccessor để truy cập HttpContext trong ứng dụng
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// Cấu hình AutoMapper để ánh xạ đối tượng DTO

// Cấu hình AutoMapper để ánh xạ đối tượng DTO
builder.Services.AddAutoMapper(
    typeof(UserAutoMapperProfile),
    typeof(ProductAutoMapperProfile),
    typeof(BrandAutoMapperProfile),
    typeof(CategoryAutoMapperProfile),
    typeof(ProductTypeAutoMapperProfile),
    typeof(ProductVariantAutoMapperProfile),
    typeof(CartAutoMapperProfile),
    typeof(VoucherAutoMapperProfile),
    typeof(AddressMapperProfile),
    typeof(OrderMapperProfile),
    typeof(SupplierAutoMapperProfile),
    typeof(InventoryLogAutoMapper),
    typeof(ReviewAutoMapperProfile)

);

// Đăng ký Repository và Services cho Dependency Injection
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IRepository<User>, Repository<User>>();
builder.Services.AddScoped<IRepository<Brand>, Repository<Brand>>();
builder.Services.AddScoped<IRepository<Category>, Repository<Category>>();
builder.Services.AddScoped<IRepository<ProductType>, Repository<ProductType>>();
builder.Services.AddScoped<IRepository<Product>, Repository<Product>>();
builder.Services.AddScoped<IRepository<ProductVariant>, Repository<ProductVariant>>();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<IRepository<Cart>, Repository<Cart>>();
builder.Services.AddScoped<IRepository<CartItem>, Repository<CartItem>>();
builder.Services.AddScoped<IRepository<Order>, Repository<Order>>();
builder.Services.AddScoped<IRepository<Review>, Repository<Review>>();
builder.Services.AddScoped<IRepository<Supplier>, Repository<Supplier>>();
builder.Services.AddScoped<IRepository<InventoryLog>, Repository<InventoryLog>>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<BrandService>();
builder.Services.AddScoped<ProductTypeService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<ProductVariantService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<VoucherService>();
builder.Services.AddScoped<AddressService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<ReviewService>();
builder.Services.AddScoped<SupplierService>();
builder.Services.AddScoped<InventoryLogService>();
builder.Services.AddScoped<CloudinaryService>();
builder.Services.AddScoped<ProductRecommenService>();
// Đăng ký PaymentService
builder.Services.AddScoped<IVnPayService, VnPayService>();

// Đăng ký LocationService với HttpClient
builder.Services.AddHttpClient<LocationService>();
builder.Services.AddHttpClient<ProductRecommenService>();


// Cấu hình CORS để cho phép kết nối từ Frontend chạy trên localhost:3000
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        builder =>
        {
            builder.WithOrigins("http://localhost:3679") // Địa chỉ của Frontend (React/Angular/Vue)
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .AllowCredentials(); // Cho phép gửi thông tin xác thực (cookies, headers...)
        });
});


// Thêm hỗ trợ cho Controllers và API Endpoints với JSON serialization
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.MaxDepth = 32; // Bạn có thể điều chỉnh giá trị này nếu cần thiết
});


// Thêm hỗ trợ cho Controllers và API Endpoints
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Cấu hình Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Khanh Skin API",
        Version = "v1"
    });

    // Cấu hình Swagger để sử dụng Bearer Token
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
    //c.OperationFilter<KhanhSkin_BackEnd.Helper.SwaggerFileOpen>();

});

// Thêm JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Xác thực người phát token
            ValidateIssuer = true,
            // Xác thực người nhận token
            ValidateAudience = true,
            // Xác thực thời gian sống của token
            ValidateLifetime = true,
            // Xác thực chữ ký của token
            ValidateIssuerSigningKey = true,
            // Định nghĩa người phát token hợp lệ
            ValidIssuer = jwtSettings["Issuer"],
            // Định nghĩa người nhận token hợp lệ
            ValidAudience = jwtSettings["Audience"],
            // Định nghĩa khóa bí mật dùng để ký token
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"])),
            NameClaimType = ClaimTypes.NameIdentifier // Đảm bảo sử dụng NameIdentifier claim

        };
    });

var app = builder.Build();

// Cấu hình pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Khanh Skin API v1"));
}

// Sử dụng HTTPS Redirection để đảm bảo an toàn
app.UseHttpsRedirection();

// Sử dụng wrapper để xử lý response và exceptions một cách nhất quán
app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions
{
    ExcludePaths = new List<AutoWrapperExcludePath> { new AutoWrapperExcludePath("/api/report/export", ExcludeMode.Strict) }
});

// Sử dụng CORS trong pipeline để chấp nhận yêu cầu từ Frontend
app.UseCors("AllowSpecificOrigins");

// Kích hoạt middleware xác thực
app.UseAuthentication();
app.UseAuthorization();

// Đăng ký các controller
app.MapControllers();

app.Run();

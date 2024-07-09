using Microsoft.AspNetCore.Builder;
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

var builder = WebApplication.CreateBuilder(args);

// Đọc cấu hình JWT từ appsettings.json
var jwtSettings = builder.Configuration.GetSection("Jwt");

// Cấu hình DbContext sử dụng chuỗi kết nối từ appsettings
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Đăng ký IHttpContextAccessor để truy cập HttpContext trong ứng dụng
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// Cấu hình AutoMapper để ánh xạ đối tượng DTO
builder.Services.AddAutoMapper(typeof(UserAutoMapperProfile));

// Đăng ký Repository và Services cho Dependency Injection
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IRepository<User>, Repository<User>>();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<UserService>();

// Thêm hỗ trợ cho Controllers và API Endpoints
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Cấu hình Swagger
// Cấu hình Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Khanh Skin API",
        Version = "v1"
    });
});


// Thêm JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]))
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

// Kích hoạt middleware xác thực
app.UseAuthentication();
app.UseAuthorization();

// Đăng ký các controller
app.MapControllers();

app.Run();

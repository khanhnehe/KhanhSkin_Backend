using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using KhanhSkin_BackEnd.Entities;
using KhanhSkin_BackEnd.Repositories; // Ensure you have the correct namespace for IRepository
using KhanhSkin_BackEnd.Services.Users; // Ensure UserService is correctly referenced
using KhanhSkin_BackEnd.Services.CurrentUser; // For ICurrentUser
using AutoMapper; // For IMapper
using Microsoft.Extensions.Logging; // For ILogger
using Microsoft.OpenApi.Models; // For Swagger configuration
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register IHttpContextAccessor
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// AutoMapper configuration
builder.Services.AddAutoMapper(typeof(UserAutoMapperProfile));

// Repository and service injections
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>)); // Adjust if your generic repository setup differs
builder.Services.AddScoped<IRepository<User>, Repository<User>>(); // Specific repository for User if needed
builder.Services.AddScoped<ICurrentUser, CurrentUser>(); // Assuming CurrentUser implements ICurrentUser
builder.Services.AddScoped<UserService>(); // Injecting UserService

// Controller support
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger configuration
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Khanh Skin API",
        Version = "v1",
        Description = "An API to manage Khanh Skin operations.",
        Contact = new OpenApiContact
        {
            Name = "Khanh Skin Support",
            Email = "support@khanhskin.com",
            Url = new Uri("https://khanhskin.com/support")
        }
    });
    // You can re-enable these if you decide to use JWT tokens
    // c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    // {
    //     Name = "Authorization",
    //     Type = OpenApiSecuritySchemeType.ApiKey,
    //     Scheme = "Bearer",
    //     BearerFormat = "JWT",
    //     In = ParameterLocation.Header,
    //     Description = "Please insert JWT token into field"
    // });
    // c.AddSecurityRequirement(new OpenApiSecurityRequirement
    // {
    //     {
    //         new OpenApiSecurityScheme
    //         {
    //             Reference = new OpenApiReference
    //             {
    //                 Type = ReferenceType.SecurityScheme,
    //                 Id = "Bearer"
    //             }
    //         },
    //         new string[] {}
    //     }
    // });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Khanh Skin API v1"));
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Ensure the AppDbContext is migrated
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

app.MapControllers();

app.Run();

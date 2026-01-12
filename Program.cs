using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PastirmaApi.API.Extensions;
using PastirmaApi.API.Hubs;
using PastirmaApi.Application.Interfaces.Repositories;
using PastirmaApi.Application.Interfaces.Services;
using PastirmaApi.Application.Services;
using PastirmaApi.Infrastructure.Data;
using PastirmaApi.Infrastructure.Data.Repositories;
using PastirmaApi.Infrastructure.Email;
using PastirmaApi.Infrastructure.Identity;
using Resend;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Railway provides PORT environment variable
var port = Environment.GetEnvironmentVariable("PORT") ?? "5296";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var jwtKey = builder.Configuration["Jwt:Key"];
var keyBytes = Encoding.UTF8.GetBytes(jwtKey!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            ),
            ValidateIssuer = false,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = false,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,

            ClockSkew = TimeSpan.Zero
        };

        // Read JWT token from cookies OR Authorization header
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // 1. Try to get token from cookie (for frontend)
                var token = context.Request.Cookies["accessToken"];               

                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }

                return Task.CompletedTask;
            }
        };
    });

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// DEBUG: Log connection string info (temporary - remove after fixing)
Console.WriteLine($"[DEBUG] Connection string is null: {connectionString == null}");
Console.WriteLine($"[DEBUG] Connection string is empty: {string.IsNullOrEmpty(connectionString)}");
Console.WriteLine($"[DEBUG] Connection string length: {connectionString?.Length ?? 0}");
Console.WriteLine($"[DEBUG] Connection string first 20 chars: {(connectionString?.Length > 20 ? connectionString.Substring(0, 20) : connectionString ?? "NULL")}");

if (string.IsNullOrEmpty(connectionString))
    throw new InvalidOperationException("Connection string is not configured");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IEmailTemplateProvider, EmailTemplateProvider>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<IBlogPostService, BlogPostService>();
builder.Services.AddScoped<IBlogCategoryService, BlogCategoryService>();
builder.Services.AddScoped<IHeroSlideService, HeroSlideService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IAddressService, AddressService>();

builder.Services.AddCaptchaServices();

builder.Services.AddControllers();

builder.Services.AddSignalR();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Auth Header: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });

    options.SwaggerDoc("v1", new() { Title = "JwtAuth Development", Version = "v1" });
});

// Read CORS origins from configuration
var allowedOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>()
    ?? throw new InvalidOperationException("CorsSettings:AllowedOrigins is not configured");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithExposedHeaders("set-cookie");
    });
});

builder.Services.AddOptions();
builder.Services.AddHttpClient<ResendClient>();
builder.Services.Configure<ResendClientOptions>(o =>
{
    o.ApiToken = builder.Configuration["Resend:RESEND_API_KEY"]!;
});
builder.Services.AddTransient<IResend, ResendClient>();

// Add Rate Limiting (security: prevent brute force and spam)
builder.Services.AddRateLimiter(options =>
{
    // Strict limit for auth endpoints (login, register, etc.)
    options.AddFixedWindowLimiter("auth", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 5;  // 5 requests per minute per IP
        opt.QueueLimit = 0;   // No queueing - reject immediately
    });

    // Medium limit for contact form (prevent spam)
    options.AddFixedWindowLimiter("contact", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(5);
        opt.PermitLimit = 3;  // 3 submissions per 5 minutes per IP
        opt.QueueLimit = 0;
    });

    // General API limit
    options.AddFixedWindowLimiter("api", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 100;  // 100 requests per minute per IP
        opt.QueueLimit = 0;
    });

    // Rejection response
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429; // Too Many Requests
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            errors = new[] { "Çok fazla istek gönderildi. Lütfen daha sonra tekrar deneyin." }
        }, cancellationToken: token);
    };
});

var app = builder.Build();

// --- Migrations otomatik uygulama ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate(); // eksik migration varsa uygular

    // Seed database with sample data
    var seeder = new DatabaseSeeder(db);
    await seeder.SeedAsync();
}
// ------------------------------------


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReactApp");

app.UseCustomMiddlewares();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();

// Use rate limiting (must be before authentication/authorization)
app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

app.MapHub<OrderHub>("/hubs/order");

app.MapControllers();

app.Run();
using FluentValidation;
using MarketLink.Application.Options;
using MarketLink.Application.Service;
using MarketLink.Application.Service.Impl;
using MarketLink.DataAccess.Persistence;
using MarketLink.DataAccess.Repositories;
using MarketLink.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Minio;
using System.Text;

namespace MarketLink.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ── Database ──
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // ── JWT Authentication ──
            var jwtSettings = builder.Configuration.GetSection("Jwt");
            var secretKey   = jwtSettings["SecretKey"]
                ?? throw new Exception("Jwt:SecretKey topilmadi appsettings.json da");

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidIssuer              = jwtSettings["Issuer"],
                    ValidateAudience         = true,
                    ValidAudience            = jwtSettings["Audience"],
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey         = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

            builder.Services.AddAuthorization();

            // ── Memory Cache ──
            builder.Services.AddMemoryCache();

            // ── Application Services ──
            builder.Services.AddScoped<IAuthService,       AuthService>();
            builder.Services.AddScoped<IJwtService,        JwtService>();
            builder.Services.AddScoped<IUserService,       UserService>();
            builder.Services.AddScoped<IOtpService,        OtpService>();
            builder.Services.AddScoped<IPermissionService, PermissionService>();
            builder.Services.AddScoped<IPhoneNumberService, PhoneNumberService>();
            builder.Services.AddScoped<ICompanyService, CompanyService>();
            builder.Services.AddScoped<IShopService, ShopService>();
            builder.Services.AddScoped<ICompanyProductService, CompanyProductService>();
            builder.Services.AddScoped<ICompanyOrderService, CompanyOrderService>();
            builder.Services.AddScoped<IStatisticsService, StatisticsService>();

            // ── Shop Services ──
            builder.Services.AddScoped<IShopProfileService, ShopProfileService>();
            builder.Services.AddScoped<ICatalogService, CatalogService>();
            builder.Services.AddScoped<ICartService, CartService>();
            builder.Services.AddScoped<IShopOrderService, ShopOrderService>();
            builder.Services.AddScoped<IRatingService, RatingService>();

            // ── Unit of Work / Repositories ──
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<MarketLink.Domain.Interfaces.ICartRepository,
                                       MarketLink.DataAccess.Repositories.CartRepository>();

            // ── FluentValidation ──
            builder.Services.AddValidatorsFromAssemblyContaining<Program>();

            // SMS xizmati — HttpClient bilan
            builder.Services.AddHttpClient<ISmsService, SmsService>();

            // ── MinIO File Service ──
            builder.Services.Configure<MinioOptions>(
                builder.Configuration.GetSection(MinioOptions.Section));

            var minioOpts = builder.Configuration.GetSection(MinioOptions.Section).Get<MinioOptions>()
                ?? new MinioOptions();

            builder.Services.AddSingleton<IMinioClient>(sp =>
            {
                var client = new MinioClient()
                    .WithEndpoint(minioOpts.Endpoint)
                    .WithCredentials(minioOpts.AccessKey, minioOpts.SecretKey);

                if (minioOpts.UseSSL)
                    client = client.WithSSL();

                return client.Build();
            });

            builder.Services.AddScoped<IFileService, MinioFileService>();

            // ── Controllers ──
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // ── Swagger (faqat Development rejimida) ──
            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title   = "Market Link API",
                        Version = "v1"
                    });

                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Description = "JWT Bearer token. Misol: Bearer {token}",
                        Name        = "Authorization",
                        In          = ParameterLocation.Header,
                        Type        = SecuritySchemeType.ApiKey,
                        Scheme      = "Bearer"
                    });

                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id   = "Bearer"
                                }
                            },
                            Array.Empty<string>()
                        }
                    });

                    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                });
            }

            // ── CORS ──
            builder.Services.AddCors(options =>
                options.AddPolicy("AllowAll", p =>
                    p.AllowAnyOrigin()
                     .AllowAnyMethod()
                     .AllowAnyHeader()));

            var app = builder.Build();

            // ── Database migration (production-safe) ──
            using (var scope = app.Services.CreateScope())
            {
                var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await ctx.Database.MigrateAsync();
                await DataSeeder.SeedAsync(ctx);
            }

            // ── Swagger (faqat Development) ──
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Market Link API v1");
                    c.RoutePrefix = "swagger";
                });
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowAll");

            app.UseMiddleware<MarketLink.API.Middleware.ExceptionMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}

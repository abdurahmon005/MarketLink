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
            var secretKey   = jwtSettings["SecretKey"];
            if (string.IsNullOrWhiteSpace(secretKey))
                throw new Exception($"Jwt:SecretKey topilmadi yoki bo'sh. Barcha Jwt config: Issuer={jwtSettings["Issuer"]}, Audience={jwtSettings["Audience"]}");

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

                // Allow JWT via query string for SignalR WebSocket connections
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        var accessToken = ctx.Request.Query["access_token"];
                        var path        = ctx.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            path.StartsWithSegments("/hubs"))
                        {
                            ctx.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddAuthorization();

            // ── Health Check ──
            builder.Services.AddHealthChecks();

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

            // ── Tracking / Notification / Dashboard ──
            builder.Services.AddScoped<ITrackingService, TrackingService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();
            builder.Services.AddScoped<ISupplierNotificationService, SupplierNotificationService>();

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
                var endpoint = string.IsNullOrWhiteSpace(minioOpts.Endpoint)
                    ? "localhost:9000"
                    : minioOpts.Endpoint;

                var client = new MinioClient()
                    .WithEndpoint(endpoint)
                    .WithCredentials(minioOpts.AccessKey ?? "minioadmin", minioOpts.SecretKey ?? "minioadmin");

                if (minioOpts.UseSSL)
                    client = client.WithSSL();

                return client.Build();
            });

            builder.Services.AddScoped<IFileService, MinioFileService>();

            // ── SignalR ──
            builder.Services.AddSignalR();

            // ── Controllers ──
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // ── Swagger ──
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

            // ── CORS ──
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", p =>
                    p.AllowAnyOrigin()
                     .AllowAnyMethod()
                     .AllowAnyHeader());

                // SignalR requires AllowCredentials + specific origins (can't use AllowAnyOrigin)
                options.AddPolicy("SignalRPolicy", p =>
                    p.WithOrigins(
                        "http://localhost:5173",
                        "http://localhost:3000",
                        "http://localhost:4200")
                     .AllowAnyMethod()
                     .AllowAnyHeader()
                     .AllowCredentials());
            });

            var app = builder.Build();

            // ── Database migration — HTTP server ishga tushgandan keyin background da ──
            app.Lifetime.ApplicationStarted.Register(() =>
            {
                _ = Task.Run(async () =>
                {
                    using var scope = app.Services.CreateScope();
                    var ctx    = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    for (int attempt = 1; attempt <= 10; attempt++)
                    {
                        try
                        {
                            await ctx.Database.MigrateAsync();
                            await DataSeeder.SeedAsync(ctx);
                            logger.LogInformation("Migration va seeding muvaffaqiyatli yakunlandi.");
                            break;
                        }
                        catch (Exception ex) when (attempt < 10)
                        {
                            logger.LogWarning("DB ulanish #{Attempt} muvaffaqiyatsiz: {Message}. 5s kutilmoqda...", attempt, ex.Message);
                            await Task.Delay(5000);
                        }
                    }
                });
            });

            // ── Swagger ──
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Market Link API v1");
                c.RoutePrefix = "swagger";
            });

            app.UseCors("AllowAll");

            app.UseMiddleware<MarketLink.API.Middleware.ExceptionMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapHealthChecks("/health");

            // Vaqtincha debug endpoint
            app.MapGet("/debug/jwt", (IConfiguration cfg) => new
            {
                JwtKeyLength  = cfg["Jwt:SecretKey"]?.Length ?? 0,
                JwtKeyIsEmpty = string.IsNullOrWhiteSpace(cfg["Jwt:SecretKey"]),
                JwtIssuer     = cfg["Jwt:Issuer"],
                JwtAudience   = cfg["Jwt:Audience"],
                Environment   = cfg["ASPNETCORE_ENVIRONMENT"]
            });

            app.MapControllers();

            app.MapHub<MarketLink.API.Hubs.TrackingHub>("/hubs/tracking")
               .RequireCors("SignalRPolicy");

            app.Run();
        }
    }
}

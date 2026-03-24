using MarketLink.Application.Options;
using MarketLink.Application.Service;
using MarketLink.Application.Service.Impl;
using MarketLink.DataAccess.Persistence;
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
            });

            // ── CORS ──
            builder.Services.AddCors(options =>
                options.AddPolicy("AllowAll", p =>
                    p.AllowAnyOrigin()
                     .AllowAnyMethod()
                     .AllowAnyHeader()));

            var app = builder.Build();

            // ── Database seed ──
            using (var scope = app.Services.CreateScope())
            {
                var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await ctx.Database.EnsureCreatedAsync();
                await DataSeeder.SeedAsync(ctx);
            }

            // ── Middleware ──
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowAll");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}

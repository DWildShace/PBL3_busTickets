using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Npgsql;
using Pbl3.Configurations;
using Pbl3.Data;
using Pbl3.Enums;
using Pbl3.Models;
using Pbl3.Services;
using Pbl3.Services.Admin;
using Pbl3.Services.BusAdmin;
using Pbl3.Services.Users;

namespace Pbl3.Extensions
{
    public static class ServiceCollectionExtensions
    {
        private static string GetEnv(string key, string defaultValue = "") =>
            Environment.GetEnvironmentVariable(key) ?? defaultValue;

        public static IServiceCollection AddPostgresDatabase(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            var connectionString =
                Environment.GetEnvironmentVariable("DATABASE_URL")
                ?? configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("Connection string not found.");

            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
            dataSourceBuilder.MapEnum<UserRole>();
            dataSourceBuilder.MapEnum<CompanyStatus>();
            dataSourceBuilder.MapEnum<TripStatus>();
            dataSourceBuilder.MapEnum<SeatType>();
            dataSourceBuilder.MapEnum<StationType>();
            dataSourceBuilder.MapEnum<BookingStatus>();
            dataSourceBuilder.MapEnum<TicketStatus>();
            dataSourceBuilder.MapEnum<SeatHoldStatus>();
            dataSourceBuilder.MapEnum<PaymentProvider>();
            dataSourceBuilder.MapEnum<PaymentIntentStatus>();
            dataSourceBuilder.MapEnum<RefundStatus>();
            dataSourceBuilder.MapEnum<ReviewStatus>();
            dataSourceBuilder.MapEnum<NotificationType>();
            dataSourceBuilder.MapEnum<NotificationStatus>();

            var dataSource = dataSourceBuilder.Build();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(dataSource)
            );

            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<DbInitializer>();
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            services.AddScoped<ICurrentUserContext, CurrentUserContext>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();

            // Business Services
            services.AddScoped<ITripSearchService, TripSearchService>();
            services.AddScoped<IBusAdminOwnershipService, BusAdminOwnershipService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<ILocationSearchService, LocationSearchService>();
            services.AddScoped<ITripDetailService, TripDetailService>();
            services.AddScoped<IRefundManagementService, RefundManagementService>();
            services.AddScoped<IBusAdminUpgradeResponseService, BusAdminUpgradeResponseService>();
            services.AddScoped<ICompanyProfileUpdateRequestService, CompanyProfileUpdateRequestService>();
            services.AddScoped<IRevenueAnalyticsService, RevenueAnalyticsService>();
            services.AddScoped<IReviewManagementService, ReviewManagementService>();
            services.AddScoped<ISystemAdminManagementService, SystemAdminManagementService>();
            services.AddScoped<ITransactionManagementService, TransactionManagementService>();
            services.AddScoped<ITripMonitoringService, TripMonitoringService>();
            services.AddScoped<IBusCompanyRegistrationService, BusCompanyRegistrationService>();
            services.AddScoped<IBusAdminProfileService, BusAdminProfileService>();
            services.AddScoped<IBusAdminBusesService, BusAdminBusesService>();
            services.AddScoped<IUserMeService, UserMeService>();
            services.AddScoped<IPassengersService, PassengersService>();

            return services;
        }

        public static IServiceCollection AddPaymentServices(this IServiceCollection services)
        {
            // MoMo Configuration
            services.Configure<MomoOptions>(options =>
            {
                options.PartnerCode = GetEnv("MOMO_PARTNER_CODE");
                options.AccessKey = GetEnv("MOMO_ACCESS_KEY");
                options.SecretKey = GetEnv("MOMO_SECRET_KEY");
                options.Endpoint = GetEnv("MOMO_ENDPOINT");
                options.RedirectUrl = GetEnv("MOMO_REDIRECT_URL");
                options.FrontendRedirectUrl = GetEnv("MOMO_FRONTEND_REDIRECT_URL");
                options.IpnUrl = GetEnv("MOMO_IPN_URL");
                options.PartnerName = GetEnv("MOMO_PARTNER_NAME");
                options.StoreId = GetEnv("MOMO_STORE_ID");
                options.RequestType = GetEnv("MOMO_REQUEST_TYPE", "captureWallet");
                options.Lang = GetEnv("MOMO_LANG", "vi");
            });

            services.AddHttpClient<IPaymentService, PaymentService>((serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<MomoOptions>>().Value;
                if (!string.IsNullOrWhiteSpace(options.Endpoint))
                {
                    client.BaseAddress = new Uri(options.Endpoint.TrimEnd('/'));
                }
            });

            // VNPay Configuration
            services.Configure<VnpayOptions>(options =>
            {
                options.TmnCode = GetEnv("VNPAY_TMN_CODE");
                options.HashSecret = GetEnv("VNPAY_HASH_SECRET");
                options.BaseUrl = GetEnv("VNPAY_BASE_URL");
                options.ReturnUrl = GetEnv("VNPAY_RETURN_URL");
                options.IpnUrl = GetEnv("VNPAY_IPN_URL");
            });

            services.AddScoped<IVnpayService, VnpayService>();

            return services;
        }

        public static IServiceCollection AddJwtAuthenticationAndAuthorization(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            var jwtKey = GetEnv("JWT_KEY", configuration["Jwt:Key"] ?? "");
            var jwtIssuer = GetEnv("JWT_ISSUER", configuration["Jwt:Issuer"] ?? "");
            var jwtAudience = GetEnv("JWT_AUDIENCE", configuration["Jwt:Audience"] ?? "");

            if (string.IsNullOrWhiteSpace(jwtKey))
                throw new InvalidOperationException("JWT key not found.");
            if (string.IsNullOrWhiteSpace(jwtIssuer))
                throw new InvalidOperationException("JWT issuer not found.");
            if (string.IsNullOrWhiteSpace(jwtAudience))
                throw new InvalidOperationException("JWT audience not found.");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.MapInboundClaims = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtIssuer,
                        ValidateAudience = true,
                        ValidAudience = jwtAudience,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        NameClaimType = JwtRegisteredClaimNames.Sub,
                        RoleClaimType = "role",
                    };
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("UserOnly", policy =>
                    policy.RequireRole(
                        UserRole.Passenger.ToString(),
                        UserRole.BusAdmin.ToString(),
                        UserRole.SysAdmin.ToString()
                    ));
                options.AddPolicy("BusAdmin", policy =>
                    policy.RequireRole(
                        UserRole.BusAdmin.ToString(),
                        UserRole.SysAdmin.ToString()
                    ));
                options.AddPolicy("AdminOnly", policy =>
                    policy.RequireRole(UserRole.SysAdmin.ToString()));
            });

            return services;
        }

        public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("FrontendDev", policy =>
                    policy.WithOrigins(
                        "http://localhost:5173",
                        "https://localhost:5173",
                        "http://localhost:3000",
                        "https://localhost:3000",
                        "http://134.209.209.6",
                        "https://134.209.209.6",
                        "http://134.209.209.6:5173",
                        "http://134.209.209.6:80",
                        "http://134.209.209.6.nip.io",
                        "https://134.209.209.6.nip.io",
                        "http://134.209.209.6.nip.io:5173",
                        "http://134.209.209.6.nip.io:80"
                    )
                    .SetIsOriginAllowed(_ => true)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                );
            });

            return services;
        }

        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "PBL3", Version = "v1" });
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Nhập JWT token thuần, Swagger sẽ tự thêm tiền tố Bearer.",
                });

                options.AddSecurityRequirement(document => new()
                {
                    {
                        new OpenApiSecuritySchemeReference("Bearer", document, null),
                        new List<string>()
                    },
                });
            });

            return services;
        }
    }
}

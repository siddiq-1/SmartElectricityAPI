using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartElectricityAPI.AutoMapper;
using SmartElectricityAPI.BackgroundServices;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Interfaces;
using SmartElectricityAPI.Middleware;
using SmartElectricityAPI.Services;
using System.Text.Json.Serialization;
using System.Text;
using System.Diagnostics;

namespace SmartElectricityAPI;

public static class ServiceRegistrationExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure Host Options
        services.Configure<HostOptions>(hostOptions =>
        {
            hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
        });

        var dbContextOptions = new DbContextOptionsBuilder<MySQLDBContext>()
        .UseMySql(configuration.GetConnectionString("DefaultConnection"), ServerVersion.AutoDetect(configuration.GetConnectionString("DefaultConnection")))
        .LogTo(Console.WriteLine, LogLevel.Information).EnableDetailedErrors()
      .Options;

        services.AddSingleton(dbContextOptions);



        // Add Controllers and JSON Options
        services.AddControllers().AddJsonOptions(x =>
        {
            x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            x.JsonSerializerOptions.IgnoreNullValues = true;
        });

        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.JsonSerializerOptions.WriteIndented = true;
        });

        // Add AutoMapper
        services.AddAutoMapper(typeof(Program));

        // Add Swagger
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(option =>
        {
            option.SwaggerDoc("v1", new OpenApiInfo { Title = "Tark Elekter API", Version = "v1" });
            option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            option.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type=ReferenceType.SecurityScheme,
                            Id="Bearer"
                        }
                    },
                    new string[]{}
                }
            });
        });

        // Add Authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ClockSkew = TimeSpan.Zero,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "TarkElekter",
                    ValidAudience = "TarkElekterCustomers",
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(Constants.Tokens.AccessTokenSecret)
                    ),
                };
            });

        // Add HttpClient and other services
        services.AddHttpClient();
        services.AddScoped<TokenService, TokenService>();
        services.AddTransient<IMqttLogger, MqttMessageLogService>();
        var mapper = AutoMapperConfig.InitializeAutoMapper();
        services.AddSingleton(mapper);
        services.AddSingleton<IConfiguration>(configuration);

        // Add Hosted Services
        services.AddHostedService<SpotPriceEngine>();
        services.AddSingleton<InverterMinuteService>();
        services.AddHostedService(sp => sp.GetRequiredService<InverterMinuteService>());
        services.AddSingleton<InverterHourService>();
        services.AddHostedService(sp => sp.GetRequiredService<InverterHourService>());
        services.AddSingleton<DeviceCompanyHourService>();
        services.AddHostedService(sp => sp.GetRequiredService<DeviceCompanyHourService>());

        // Add Scoped Services
        services.AddScoped<IUserInfo, UserData>();
        services.AddScoped<UserService>();
        services.AddScoped<InverterApiService, InverterApiService>();
        services.AddScoped<MqttSystemMessageService, MqttSystemMessageService>();
        services.AddScoped<WeatherApiComService, WeatherApiComService>();

        // Add Memory Cache
        services.AddMemoryCache();

        // Add CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowReactFrontend",
                builder =>
                {
                    builder.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173", "http://192.168.88.234:5173", "http://192.168.88.248:5173", "https://risto.tmerps.ee", "https://risto.tmerps.ee/smart", "https://mygrid.tmerps.ee")
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();
                });
        });

        if (Debugger.IsAttached)
        {
            services.AddApplicationInsightsTelemetry();
        }

        return services;
    }
}

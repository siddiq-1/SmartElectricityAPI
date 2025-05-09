using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartElectricityAPI;
using SmartElectricityAPI.AutoMapper;
using SmartElectricityAPI.BackgroundServices;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Interfaces;
using SmartElectricityAPI.Middleware;
using SmartElectricityAPI.Services;
using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;

ThreadPool.SetMinThreads(workerThreads: 100, completionPortThreads: 100);
var builder = WebApplication.CreateBuilder(args);


var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

builder.Services.Configure<HostOptions>(hostOptions =>
{
    hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
});


builder.Services.AddTransient(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    var dbContextOptions = new DbContextOptionsBuilder<MySQLDBContext>()
         .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
         .LogTo(Console.WriteLine, LogLevel.Error).EnableDetailedErrors()
         .EnableSensitiveDataLogging()
       .Options;

    return new MySQLDBContext(dbContextOptions);
});

builder.Services.AddControllers().AddJsonOptions(x =>
{
    x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    x.JsonSerializerOptions.IgnoreNullValues = true;
});
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.WriteIndented = true;
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(option =>
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

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

var dbContextOptions = new DbContextOptionsBuilder<MySQLDBContext>()
   .UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")))
   .LogTo(Console.WriteLine, LogLevel.Information).EnableDetailedErrors()
 .Options;
builder.Services.AddHttpClient();
builder.Services.AddScoped<TokenService, TokenService>();
builder.Services.AddTransient<IMqttLogger, MqttMessageLogService>();
var mapper = AutoMapperConfig.InitializeAutoMapper();
builder.Services.AddSingleton(mapper);
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddSingleton(dbContextOptions);

builder.Services.AddHostedService<SpotPriceEngine>();
builder.Services.AddSingleton<InverterMinuteService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<InverterMinuteService>());

builder.Services.AddSingleton<InverterHourService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<InverterHourService>());
builder.Services.AddSingleton<DeviceCompanyHourService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<DeviceCompanyHourService>());
builder.Services.AddScoped<IUserInfo, UserData>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<InverterApiService, InverterApiService>();
builder.Services.AddScoped<MqttSystemMessageService, MqttSystemMessageService>();
builder.Services.AddScoped<WeatherApiComService, WeatherApiComService>();

builder.Services.AddMemoryCache();


builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173", "http://localhost:5173/") // ✅ exact frontend origin
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // ✅ needed for cookies/tokens
    });
});

if (Debugger.IsAttached)
{
    builder.Services.AddApplicationInsightsTelemetry();
}

// Use the middleware configuration
var app = builder.Build();


app.UseApplicationMiddleware();

app.Run();





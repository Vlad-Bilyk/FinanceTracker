using FinanceTracker.Api.Middlewares;
using FinanceTracker.Api.Services;
using FinanceTracker.Application.DTOs.Auth;
using FinanceTracker.Application.DTOs.Operation;
using FinanceTracker.Application.DTOs.OperationType;
using FinanceTracker.Application.DTOs.User;
using FinanceTracker.Application.DTOs.Wallet;
using FinanceTracker.Application.Interfaces;
using FinanceTracker.Application.Interfaces.Common;
using FinanceTracker.Application.Interfaces.Repositories;
using FinanceTracker.Application.Interfaces.Services;
using FinanceTracker.Application.Services;
using FinanceTracker.Application.Validators;
using FinanceTracker.Infrastructure.Data;
using FinanceTracker.Infrastructure.Data.Seed;
using FinanceTracker.Infrastructure.Repositories;
using FinanceTracker.Infrastructure.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

// Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware", LogEventLevel.Fatal)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] [{UserId}] {Message:lj}{NewLine}")
    .WriteTo.File(
        path: "Logs/app-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        fileSizeLimitBytes: 10485760,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{CorrelationId}] [{UserId}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Host.UseSerilog();

    // Error handlers
    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    // Db connection
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultDbConnection")));

    builder.Services.AddTransient<IDbSeeder, DbSeeder>();

    // User context
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<IUserContext, UserContext>();

    // Repositories
    builder.Services.AddScoped<IFinancialOperationTypeRepository, FinancialOperationTypeRepository>();
    builder.Services.AddScoped<IFinancialOperationRepository, FinancialOperationRepository>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IWalletRepository, WalletRepository>();
    builder.Services.AddScoped<ICurrencyRepository, CurrencyRepository>();
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

    // Services
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IWalletService, WalletService>();
    builder.Services.AddScoped<IOperationTypeService, OperationTypeService>();
    builder.Services.AddScoped<IFinancialOperationService, FinancialOperationService>();
    builder.Services.AddScoped<IReportService, ReportService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddScoped<ICurrencyService, CurrencyService>();

    // Http clients
    builder.Services.AddHttpClient<ICurrencyCatalogProvider, ExternalCurrencyCatalogProvider>();
    builder.Services.AddHttpClient<IExchangeRateService, ExchangeRateService>();

    // Validators
    builder.Services.AddScoped<IValidator<OperationTypeCreateDto>, OperationTypeCreateValidator>();
    builder.Services.AddScoped<IValidator<OperationTypeUpdateDto>, OperationTypeUpdateValidator>();
    builder.Services.AddScoped<IValidator<FinancialOperationUpsertDto>, FinancialOperationUpsertValidator>();
    builder.Services.AddScoped<IValidator<RegisterRequest>, RegisterRequestValidator>();
    builder.Services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
    builder.Services.AddScoped<IValidator<UserUpdateDto>, UserUpdateValidator>();
    builder.Services.AddScoped<IValidator<ChangePasswordRequest>, ChangePasswordRequestValidator>();
    builder.Services.AddScoped<IValidator<WalletCreateDto>, WalletCreateValidator>();
    builder.Services.AddScoped<IValidator<WalletUpdateDto>, WalletUpdateValidator>();

    // JWT Authentication
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured.");

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
        };
    });

    builder.Services.AddAuthorization();

    // Configure Swagger
    builder.Services.AddControllers()
        .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Finance Tracker API",
            Version = "v1"
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT Authorization header using the Bearer scheme. " +
                "Enter only token without 'Bearer' prefix"
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
                Array.Empty<string>()
            }
        });

        var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
    });

    // Configure CORS
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
            policy.AllowAnyHeader()
                  .AllowAnyMethod()
                  .WithOrigins("http://localhost:5022"));
    });

    var app = builder.Build();

    // Apply migrations and seed the database
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        var seeder = scope.ServiceProvider.GetService<IDbSeeder>();
        await seeder!.SeedAsync();

        Log.Information("Apply migrations and seed database with initial data");
    }

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Finance Tracker API v1");
            c.RoutePrefix = "swagger";
            c.ConfigObject.PersistAuthorization = true;
        });
    }

    app.UseHttpsRedirection();
    app.UseCors();

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseMiddleware<RequestResponseLoggingMiddleware>();
    app.UseExceptionHandler();

    app.MapControllers();

    Log.Information("Starting FinanceTracker API");
    await app.RunAsync();
    Log.Information("FinanceTracker API stopped cleanly");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    await Log.CloseAndFlushAsync();
}
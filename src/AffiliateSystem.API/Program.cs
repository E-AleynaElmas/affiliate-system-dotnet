using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using AspNetCoreRateLimit;
using FluentValidation;
using FluentValidation.AspNetCore;
using AffiliateSystem.Infrastructure.Data;
using AffiliateSystem.Infrastructure.Repositories;
using AffiliateSystem.Infrastructure.Services;
using AffiliateSystem.Infrastructure.Middleware;
using AffiliateSystem.Infrastructure.Filters;
using AffiliateSystem.Infrastructure.Configuration;
using AffiliateSystem.Application.Services;
using AffiliateSystem.Application.Interfaces;
using AffiliateSystem.Application.Mappings;
using AffiliateSystem.Application.Validators;
using AffiliateSystem.Domain.Interfaces;
using System.Text;
using AffiliateSystem.Application.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container

// Configuration
builder.Services.Configure<SecuritySettings>(builder.Configuration.GetSection(SecuritySettings.SectionName));

// Add Entity Framework Core with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "AffiliateSystem",
        ValidAudience = jwtSettings["Audience"] ?? "AffiliateSystemUsers",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

// Add Authorization
builder.Services.AddAuthorization();

// Register Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IBlockedIpRepository, BlockedIpRepository>();
builder.Services.AddScoped<ILoginAttemptRepository, LoginAttemptRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register Application Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtService, JwtService>();

// Register Infrastructure Services
builder.Services.AddScoped<ICaptchaService, CaptchaService>();
builder.Services.AddSingleton<ICacheService, MemoryCacheService>(); // Changed to Singleton for caching
builder.Services.AddScoped<IIpBlockingService, IpBlockingService>();
builder.Services.AddScoped<ILoginAttemptService, LoginAttemptService>();
builder.Services.AddHttpClient();

// Add Rate Limiting
builder.Services.AddRateLimiting(builder.Configuration);

// Add Memory Cache (required for rate limiting and CAPTCHA)
builder.Services.AddMemoryCache();

// Configure SecuritySettings
builder.Services.Configure<SecuritySettings>(
    builder.Configuration.GetSection(SecuritySettings.SectionName));

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Add Controllers with Global Filters
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalXssProtectionAttribute>();
    options.Filters.Add<LoggingActionFilter>();
});

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

// Configure CORS (Allow any origin during development)
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevelopmentPolicy",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Add API documentation with JWT support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Affiliate System API",
        Version = "v1",
        Description = "Secure affiliate management system with role-based access control"
    });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\""
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Affiliate System API v1");
    });
    app.UseCors("DevelopmentPolicy");
}

app.UseHttpsRedirection();

// Add Global Exception Handling (should be first)
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// Add Client Info Extraction
app.UseMiddleware<ClientInfoMiddleware>();

// Add IP Rate Limiting middleware
app.UseIpRateLimiting();

// Add custom IP Blocking middleware
app.UseMiddleware<IpBlockingMiddleware>();

// Add authentication before authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
}

app.Run();

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
using Npgsql.EntityFrameworkCore.PostgreSQL;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container

// Configuration
builder.Services.Configure<SecuritySettings>(builder.Configuration.GetSection(SecuritySettings.SectionName));

// Add Entity Framework Core with PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("database");

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
    // All Endpoints - Full API
    c.SwaggerDoc("all", new OpenApiInfo
    {
        Title = "All Endpoints",
        Version = "v1",
        Description = "Complete API documentation with all endpoints"
    });

    // Admin Role - Full access
    c.SwaggerDoc("admin", new OpenApiInfo
    {
        Title = "Admin Role",
        Version = "v1",
        Description = "Admin: admin@affiliate.com / Admin@123"
    });

    // Manager Role
    c.SwaggerDoc("manager", new OpenApiInfo
    {
        Title = "Manager Role",
        Version = "v1",
        Description = "Manager: manager@affiliate.com / Manager@123"
    });

    // Customer Role
    c.SwaggerDoc("customer", new OpenApiInfo
    {
        Title = "Customer Role",
        Version = "v1",
        Description = "Customer: customer1@affiliate.com / Customer@123"
    });

    // Document filter to include/exclude endpoints based on role
    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        // Show all endpoints in "all" view
        if (docName == "all")
            return true;

        try
        {
            var controllerName = apiDesc.ActionDescriptor.RouteValues["controller"];

            // Auth endpoints available to all roles
            if (controllerName == "Auth")
                return true;

            // Get endpoint metadata
            var endpoint = apiDesc.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
            if (endpoint == null)
                return false;

            // Get authorize attributes from method
            var methodAuthorize = endpoint.MethodInfo
                .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true)
                .Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>()
                .ToList();

            // Get authorize attributes from controller
            var controllerAuthorize = endpoint.ControllerTypeInfo
                .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true)
                .Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>()
                .ToList();

            // Combine all authorize attributes
            var allAuthorizeAttrs = methodAuthorize.Concat(controllerAuthorize).ToList();

            // Get all required roles
            var requiredRoles = allAuthorizeAttrs
                .Where(a => !string.IsNullOrEmpty(a.Roles))
                .SelectMany(a => a.Roles.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(r => r.Trim().ToLower())
                .Distinct()
                .ToList();

            // If endpoint requires authentication but no specific role
            if (!requiredRoles.Any() && allAuthorizeAttrs.Any())
            {
                // Show to all authenticated users
                return true;
            }

            // If no authorization required, show to all
            if (!allAuthorizeAttrs.Any())
                return true;

            // Filter by role
            return docName switch
            {
                // Admin sees everything
                "admin" => !requiredRoles.Any() || requiredRoles.Contains("admin"),
                // Manager sees only if Manager role is explicitly allowed (not Admin-only)
                "manager" => !requiredRoles.Any() || requiredRoles.Contains("manager"),
                // Customer sees only endpoints without specific role requirements
                "customer" => !requiredRoles.Any(),
                _ => false
            };
        }
        catch
        {
            // In case of any error, don't include the endpoint
            return false;
        }
    });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme"
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
        c.SwaggerEndpoint("/swagger/all/swagger.json", "All Endpoints");
        c.SwaggerEndpoint("/swagger/admin/swagger.json", "Admin Role");
        c.SwaggerEndpoint("/swagger/manager/swagger.json", "Manager Role");
        c.SwaggerEndpoint("/swagger/customer/swagger.json", "Customer Role");
        c.DocumentTitle = "Affiliate System API";
    });
    app.UseCors("DevelopmentPolicy");
}

// Serve static files from wwwroot
app.UseStaticFiles();

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

// Map Health Check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();

    // Seed test data in development environment
    if (app.Environment.IsDevelopment())
    {
        await DataSeeder.SeedAsync(app.Services);
    }
}

app.Run();

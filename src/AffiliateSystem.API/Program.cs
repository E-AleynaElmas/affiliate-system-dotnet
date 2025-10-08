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

builder.Services.Configure<SecuritySettings>(builder.Configuration.GetSection(SecuritySettings.SectionName));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
// JWT Authentication
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

builder.Services.AddAuthorization();

// Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IBlockedIpRepository, BlockedIpRepository>();
builder.Services.AddScoped<ILoginAttemptRepository, LoginAttemptRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Application Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtService, JwtService>();

// Infrastructure Services
builder.Services.AddScoped<ICaptchaService, CaptchaService>();
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
builder.Services.AddScoped<IIpBlockingService, IpBlockingService>();
builder.Services.AddScoped<ILoginAttemptService, LoginAttemptService>();
builder.Services.AddHttpClient();

builder.Services.AddRateLimiting(builder.Configuration);
builder.Services.AddMemoryCache();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("database");

builder.Services.Configure<SecuritySettings>(
    builder.Configuration.GetSection(SecuritySettings.SectionName));

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalXssProtectionAttribute>();
    options.Filters.Add<LoggingActionFilter>();
});

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevelopmentPolicy",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Swagger with role-based documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("all", new OpenApiInfo
    {
        Title = "All Endpoints",
        Version = "v1",
        Description = "Complete API documentation with all endpoints"
    });

    c.SwaggerDoc("admin", new OpenApiInfo
    {
        Title = "Admin Role",
        Version = "v1",
        Description = "Admin: admin@affiliate.com / Admin@123"
    });

    c.SwaggerDoc("manager", new OpenApiInfo
    {
        Title = "Manager Role",
        Version = "v1",
        Description = "Manager: manager@affiliate.com / Manager@123"
    });

    c.SwaggerDoc("customer", new OpenApiInfo
    {
        Title = "Customer Role",
        Version = "v1",
        Description = "Customer: customer1@affiliate.com / Customer@123"
    });

    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        if (docName == "all")
            return true;

        try
        {
            var controllerName = apiDesc.ActionDescriptor.RouteValues["controller"];

            if (controllerName == "Auth")
                return true;

            var endpoint = apiDesc.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
            if (endpoint == null)
                return false;

            var methodAuthorize = endpoint.MethodInfo
                .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true)
                .Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>()
                .ToList();

            var controllerAuthorize = endpoint.ControllerTypeInfo
                .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true)
                .Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>()
                .ToList();

            var allAuthorizeAttrs = methodAuthorize.Concat(controllerAuthorize).ToList();

            var requiredRoles = allAuthorizeAttrs
                .Where(a => !string.IsNullOrEmpty(a.Roles))
                .SelectMany(a => a.Roles.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(r => r.Trim().ToLower())
                .Distinct()
                .ToList();

            if (!requiredRoles.Any() && allAuthorizeAttrs.Any())
                return true;

            if (!allAuthorizeAttrs.Any())
                return true;

            return docName switch
            {
                "admin" => !requiredRoles.Any() || requiredRoles.Contains("admin"),
                "manager" => !requiredRoles.Any() || requiredRoles.Contains("manager"),
                "customer" => !requiredRoles.Any(),
                _ => false
            };
        }
        catch
        {
            return false;
        }
    });

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

app.UseStaticFiles();
app.UseHttpsRedirection();

// Middleware pipeline
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseMiddleware<ClientInfoMiddleware>();
app.UseIpRateLimiting();
app.UseMiddleware<IpBlockingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});

// Database initialization
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();

    if (app.Environment.IsDevelopment())
    {
        await DataSeeder.SeedAsync(app.Services);
    }
}

app.Run();

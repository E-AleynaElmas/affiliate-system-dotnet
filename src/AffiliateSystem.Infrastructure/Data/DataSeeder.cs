using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AffiliateSystem.Domain.Entities;
using AffiliateSystem.Domain.Enums;
using AffiliateSystem.Application.Interfaces;

namespace AffiliateSystem.Infrastructure.Data;

/// <summary>
/// Database seeder for test data
/// </summary>
public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            // Ensure database is created
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migration completed");

            // Check if data already exists
            if (await context.Users.AnyAsync())
            {
                logger.LogInformation("Database already has data - skipping seed");
                return;
            }

            // Create test users
            var users = new List<User>();

            // Admin user
            var (adminHash, adminSalt) = passwordHasher.HashPassword("Admin@123");
            var admin = new User
            {
                Id = Guid.NewGuid(),
                Email = "admin@affiliate.com",
                FirstName = "Admin",
                LastName = "User",
                PasswordSalt = adminSalt,
                PasswordHash = adminHash,
                Role = UserRole.Admin,
                IsActive = true,
                EmailConfirmed = true,
                PhoneNumber = "+1234567890",
                CreatedAt = DateTime.UtcNow
            };
            users.Add(admin);

            // Manager user with referral code
            var (managerHash, managerSalt) = passwordHasher.HashPassword("Manager@123");
            var manager = new User
            {
                Id = Guid.NewGuid(),
                Email = "manager@affiliate.com",
                FirstName = "Manager",
                LastName = "User",
                PasswordSalt = managerSalt,
                PasswordHash = managerHash,
                Role = UserRole.Manager,
                ReferralCode = "MGR12345",
                IsActive = true,
                EmailConfirmed = true,
                PhoneNumber = "+1234567891",
                CreatedAt = DateTime.UtcNow
            };
            users.Add(manager);

            // Create referral link for manager
            var referralLink = new ReferralLink
            {
                Id = Guid.NewGuid(),
                Code = manager.ReferralCode,
                CreatedByUserId = manager.Id,
                IsActive = true,
                UsageCount = 0,
                MaxUsages = 10,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow
            };

            // Customer users
            for (int i = 1; i <= 5; i++)
            {
                var (customerHash, customerSalt) = passwordHasher.HashPassword("Customer@123");
                var customer = new User
                {
                    Id = Guid.NewGuid(),
                    Email = $"customer{i}@affiliate.com",
                    FirstName = $"Customer{i}",
                    LastName = "User",
                    PasswordSalt = customerSalt,
                    PasswordHash = customerHash,
                    Role = UserRole.Customer,
                    IsActive = true,
                    EmailConfirmed = true,
                    PhoneNumber = $"+123456789{i}",
                    ReferredById = i <= 2 ? manager.Id : null, // First 2 customers referred by manager
                    CreatedAt = DateTime.UtcNow.AddDays(-i)
                };
                users.Add(customer);
            }

            // Add users to database
            await context.Users.AddRangeAsync(users);
            await context.ReferralLinks.AddAsync(referralLink);

            // Add some login attempts for testing
            var loginAttempts = new List<LoginAttempt>();

            // Successful login attempts
            loginAttempts.Add(new LoginAttempt
            {
                Id = Guid.NewGuid(),
                UserId = admin.Id,
                IpAddress = "127.0.0.1",
                IsSuccessful = true,
                UserAgent = "Mozilla/5.0 Test Browser",
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            });

            // Failed login attempts for testing IP blocking
            for (int i = 0; i < 3; i++)
            {
                loginAttempts.Add(new LoginAttempt
                {
                    Id = Guid.NewGuid(),
                    UserId = null,
                    IpAddress = "192.168.1.100",
                    IsSuccessful = false,
                    FailureReason = "Invalid credentials",
                    UserAgent = "Mozilla/5.0 Test Browser",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-30 + i)
                });
            }

            await context.LoginAttempts.AddRangeAsync(loginAttempts);

            // Save all changes
            await context.SaveChangesAsync();

            logger.LogInformation("Database seeded successfully with test data");
            logger.LogInformation("=== Test User Credentials ===");
            logger.LogInformation("Admin: admin@affiliate.com / Admin@123");
            logger.LogInformation("Manager: manager@affiliate.com / Manager@123");
            logger.LogInformation("Customers: customer1-5@affiliate.com / Customer@123");
            logger.LogInformation("Manager Referral Code: MGR12345");
            logger.LogInformation("=============================");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }
}
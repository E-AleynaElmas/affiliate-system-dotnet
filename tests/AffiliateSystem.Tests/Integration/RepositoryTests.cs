using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using AffiliateSystem.Domain.Entities;
using AffiliateSystem.Domain.Enums;
using AffiliateSystem.Infrastructure.Data;
using AffiliateSystem.Infrastructure.Repositories;
using AffiliateSystem.Tests.Helpers;

namespace AffiliateSystem.Tests.Integration;

/// <summary>
/// Integration tests for Repository implementation
/// </summary>
public class RepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Repository<User> _userRepository;
    private readonly UnitOfWork _unitOfWork;

    public RepositoryTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _userRepository = new Repository<User>(_context);
        _unitOfWork = new UnitOfWork(_context);
    }

    [Fact]
    public async Task AddAsync_ShouldAddEntityToDatabase()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser("test@example.com");

        // Act
        await _userRepository.AddAsync(user);
        await _unitOfWork.CompleteAsync();

        // Assert
        var savedUser = await _userRepository.GetByIdAsync(user.Id);
        savedUser.Should().NotBeNull();
        savedUser!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_ExistingEntity_ShouldReturnEntity()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser();
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRepository.GetByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingEntity_ShouldReturnNull()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await _userRepository.GetByIdAsync(nonExistingId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllNonDeletedEntities()
    {
        // Arrange
        var user1 = TestDataBuilder.CreateUser("user1@example.com");
        var user2 = TestDataBuilder.CreateUser("user2@example.com");
        var user3 = TestDataBuilder.CreateUser("user3@example.com");
        user3.IsDeleted = true; // Soft deleted

        await _context.Users.AddRangeAsync(user1, user2, user3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRepository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().NotContain(u => u.IsDeleted);
    }

    [Fact]
    public async Task FindAsync_WithPredicate_ShouldReturnMatchingEntities()
    {
        // Arrange
        var customer = TestDataBuilder.CreateUser("customer@example.com", UserRole.Customer);
        var manager = TestDataBuilder.CreateManager("manager@example.com");
        var admin = TestDataBuilder.CreateAdmin("admin@example.com");

        await _context.Users.AddRangeAsync(customer, manager, admin);
        await _context.SaveChangesAsync();

        // Act
        var managers = await _userRepository.FindAsync(u => u.Role == UserRole.Manager);

        // Assert
        managers.Should().HaveCount(1);
        managers.First().Email.Should().Be("manager@example.com");
    }

    [Fact]
    public async Task FindAsync_WithOrdering_ShouldReturnOrderedEntities()
    {
        // Arrange
        var user1 = TestDataBuilder.CreateUser("a@example.com");
        var user2 = TestDataBuilder.CreateUser("b@example.com");
        var user3 = TestDataBuilder.CreateUser("c@example.com");

        await _context.Users.AddRangeAsync(user2, user3, user1); // Add in random order
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRepository.FindAsync(
            predicate: null,
            orderBy: q => q.OrderBy(u => u.Email));

        // Assert
        result.Should().HaveCount(3);
        result.ElementAt(0).Email.Should().Be("a@example.com");
        result.ElementAt(1).Email.Should().Be("b@example.com");
        result.ElementAt(2).Email.Should().Be("c@example.com");
    }

    [Fact]
    public async Task FindAsync_WithTake_ShouldLimitResults()
    {
        // Arrange
        for (int i = 0; i < 10; i++)
        {
            await _context.Users.AddAsync(TestDataBuilder.CreateUser($"user{i}@example.com"));
        }
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRepository.FindAsync(take: 5);

        // Assert
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task SingleOrDefaultAsync_OneMatch_ShouldReturnEntity()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser("unique@example.com");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRepository.SingleOrDefaultAsync(u => u.Email == "unique@example.com");

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("unique@example.com");
    }

    [Fact]
    public async Task SingleOrDefaultAsync_NoMatch_ShouldReturnNull()
    {
        // Act
        var result = await _userRepository.SingleOrDefaultAsync(u => u.Email == "nonexistent@example.com");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Update_ShouldModifyEntity()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser();
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        user.FirstName = "Updated";
        _userRepository.Update(user);
        await _unitOfWork.CompleteAsync();

        // Assert
        var updatedUser = await _userRepository.GetByIdAsync(user.Id);
        updatedUser!.FirstName.Should().Be("Updated");
        updatedUser.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Remove_ShouldSoftDeleteEntity()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser();
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        _userRepository.Remove(user);
        await _unitOfWork.CompleteAsync();

        // Assert
        var deletedUser = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        deletedUser.Should().NotBeNull();
        deletedUser!.IsDeleted.Should().BeTrue();
        deletedUser.UpdatedAt.Should().NotBeNull();

        // Should not be found by normal queries
        var notFound = await _userRepository.GetByIdAsync(user.Id);
        notFound.Should().BeNull();
    }

    [Fact]
    public async Task AnyAsync_WithMatchingEntity_ShouldReturnTrue()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser("exists@example.com");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRepository.AnyAsync(u => u.Email == "exists@example.com");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task AnyAsync_WithNoMatchingEntity_ShouldReturnFalse()
    {
        // Act
        var result = await _userRepository.AnyAsync(u => u.Email == "notexists@example.com");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        for (int i = 0; i < 5; i++)
        {
            await _context.Users.AddAsync(TestDataBuilder.CreateUser($"user{i}@example.com"));
        }
        await _context.SaveChangesAsync();

        // Act
        var count = await _userRepository.CountAsync();

        // Assert
        count.Should().Be(5);
    }

    [Fact]
    public async Task CountAsync_WithPredicate_ShouldReturnCorrectCount()
    {
        // Arrange
        await _context.Users.AddAsync(TestDataBuilder.CreateUser("user1@example.com", UserRole.Customer));
        await _context.Users.AddAsync(TestDataBuilder.CreateUser("user2@example.com", UserRole.Customer));
        await _context.Users.AddAsync(TestDataBuilder.CreateManager("manager@example.com"));
        await _context.SaveChangesAsync();

        // Act
        var customerCount = await _userRepository.CountAsync(u => u.Role == UserRole.Customer);

        // Assert
        customerCount.Should().Be(2);
    }

    [Fact]
    public async Task Transaction_Rollback_ShouldNotPersistChanges()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser();

        // Act
        using (var transaction = await _unitOfWork.BeginTransactionAsync())
        {
            await _userRepository.AddAsync(user);
            await _unitOfWork.CompleteAsync();

            // Rollback transaction
            await _unitOfWork.RollbackAsync();
        }

        // Assert
        var notFound = await _userRepository.GetByIdAsync(user.Id);
        notFound.Should().BeNull();
    }

    [Fact]
    public async Task Transaction_Commit_ShouldPersistChanges()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser();

        // Act
        using (var transaction = await _unitOfWork.BeginTransactionAsync())
        {
            await _userRepository.AddAsync(user);
            await _unitOfWork.CompleteAsync();

            // Commit transaction
            await _unitOfWork.CommitAsync();
        }

        // Assert
        var found = await _userRepository.GetByIdAsync(user.Id);
        found.Should().NotBeNull();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
using FluentAssertions;
using AffiliateSystem.Application.Services;

namespace AffiliateSystem.Tests.Unit.Services;

/// <summary>
/// Unit tests for PasswordHasher service
/// </summary>
public class PasswordHasherTests
{
    private readonly PasswordHasher _passwordHasher;

    public PasswordHasherTests()
    {
        _passwordHasher = new PasswordHasher();
    }

    [Fact]
    public void HashPassword_ShouldReturnHashAndSalt()
    {
        // Arrange
        var password = "MySecurePassword123!";

        // Act
        var result = _passwordHasher.HashPassword(password);

        // Assert
        result.hash.Should().NotBeNullOrEmpty();
        result.salt.Should().NotBeNullOrEmpty();
        result.hash.Should().NotBe(password); // Hash should be different from original
        result.salt.Should().HaveLength(44); // Base64 encoded 32 bytes
    }

    [Fact]
    public void HashPassword_SamePasswordDifferentSalt_ShouldReturnDifferentHashes()
    {
        // Arrange
        var password = "MySecurePassword123!";

        // Act
        var result1 = _passwordHasher.HashPassword(password);
        var result2 = _passwordHasher.HashPassword(password);

        // Assert
        result1.hash.Should().NotBe(result2.hash);
        result1.salt.Should().NotBe(result2.salt);
    }

    [Fact]
    public void VerifyPassword_CorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "MySecurePassword123!";
        var (hash, salt) = _passwordHasher.HashPassword(password);

        // Act
        var result = _passwordHasher.VerifyPassword(password, hash, salt);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_IncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var correctPassword = "MySecurePassword123!";
        var incorrectPassword = "WrongPassword456!";
        var (hash, salt) = _passwordHasher.HashPassword(correctPassword);

        // Act
        var result = _passwordHasher.VerifyPassword(incorrectPassword, hash, salt);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_EmptyPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "MySecurePassword123!";
        var (hash, salt) = _passwordHasher.HashPassword(password);

        // Act
        var result = _passwordHasher.VerifyPassword(string.Empty, hash, salt);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_NullPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "MySecurePassword123!";
        var (hash, salt) = _passwordHasher.HashPassword(password);

        // Act
        var result = _passwordHasher.VerifyPassword(null!, hash, salt);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WrongSalt_ShouldReturnFalse()
    {
        // Arrange
        var password = "MySecurePassword123!";
        var (hash, _) = _passwordHasher.HashPassword(password);
        var (_, wrongSalt) = _passwordHasher.HashPassword("AnotherPassword");

        // Act
        var result = _passwordHasher.VerifyPassword(password, hash, wrongSalt);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("a")] // Too short
    [InlineData("short")]
    [InlineData("12345678")] // Only numbers
    [InlineData("Password123")] // Common password
    [InlineData("P@ssw0rd!")] // Another common password
    public void HashPassword_WeakPasswords_ShouldStillHash(string weakPassword)
    {
        // Note: Password strength validation should be done at a higher level
        // The hasher should hash any password given to it

        // Act
        var result = _passwordHasher.HashPassword(weakPassword);

        // Assert
        result.hash.Should().NotBeNullOrEmpty();
        result.salt.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void HashPassword_VeryLongPassword_ShouldHandleCorrectly()
    {
        // Arrange
        var longPassword = new string('a', 1000) + "Password123!";

        // Act
        var result = _passwordHasher.HashPassword(longPassword);

        // Assert
        result.hash.Should().NotBeNullOrEmpty();
        result.salt.Should().NotBeNullOrEmpty();

        // Verify it works
        var verifyResult = _passwordHasher.VerifyPassword(longPassword, result.hash, result.salt);
        verifyResult.Should().BeTrue();
    }

    [Fact]
    public void HashPassword_SpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var passwordWithSpecialChars = "P@$$w0rd!#%&*()_+{}[]|\\:;<>?,./~`";

        // Act
        var result = _passwordHasher.HashPassword(passwordWithSpecialChars);

        // Assert
        result.hash.Should().NotBeNullOrEmpty();
        result.salt.Should().NotBeNullOrEmpty();

        // Verify it works
        var verifyResult = _passwordHasher.VerifyPassword(passwordWithSpecialChars, result.hash, result.salt);
        verifyResult.Should().BeTrue();
    }
}
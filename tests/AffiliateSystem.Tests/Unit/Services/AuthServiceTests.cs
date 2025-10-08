using Moq;
using FluentAssertions;
using AutoMapper;
using AffiliateSystem.Application.Services;
using AffiliateSystem.Application.Interfaces;
using AffiliateSystem.Application.DTOs.Auth;
using AffiliateSystem.Application.DTOs.Common;
using AffiliateSystem.Domain.Entities;
using AffiliateSystem.Domain.Interfaces;
using AffiliateSystem.Domain.Enums;
using AffiliateSystem.Tests.Helpers;
using System.Linq.Expressions;

namespace AffiliateSystem.Tests.Unit.Services;

/// <summary>
/// Unit tests for AuthService
/// </summary>
public class AuthServiceTests
{
    private readonly Mock<IRepository<User>> _userRepositoryMock;
    private readonly Mock<IRepository<LoginAttempt>> _loginAttemptRepositoryMock;
    private readonly Mock<IRepository<BlockedIp>> _blockedIpRepositoryMock;
    private readonly Mock<IRepository<ReferralLink>> _referralLinkRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<ICaptchaService> _captchaServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IRepository<User>>();
        _loginAttemptRepositoryMock = new Mock<IRepository<LoginAttempt>>();
        _blockedIpRepositoryMock = new Mock<IRepository<BlockedIp>>();
        _referralLinkRepositoryMock = new Mock<IRepository<ReferralLink>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtServiceMock = new Mock<IJwtService>();
        _captchaServiceMock = new Mock<ICaptchaService>();
        _mapperMock = new Mock<IMapper>();

        _authService = new AuthService(
            _userRepositoryMock.Object,
            _loginAttemptRepositoryMock.Object,
            _blockedIpRepositoryMock.Object,
            _referralLinkRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _passwordHasherMock.Object,
            _jwtServiceMock.Object,
            _captchaServiceMock.Object,
            _mapperMock.Object
        );
    }

    #region Login Tests

    [Fact]
    public async Task LoginAsync_ValidCredentials_ShouldReturnSuccess()
    {
        // Arrange
        var loginRequest = TestDataBuilder.CreateLoginRequest();
        var user = TestDataBuilder.CreateUser(email: loginRequest.Email);

        _captchaServiceMock.Setup(x => x.ValidateCaptchaAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _blockedIpRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<BlockedIp, bool>>>(), default))
            .ReturnsAsync((BlockedIp?)null);

        _userRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), default))
            .ReturnsAsync(user);

        _passwordHasherMock.Setup(x => x.VerifyPassword(loginRequest.Password, user.PasswordHash, user.PasswordSalt))
            .Returns(true);

        _jwtServiceMock.Setup(x => x.GenerateToken(user))
            .Returns("test-jwt-token");

        _mapperMock.Setup(x => x.Map<UserInfo>(user))
            .Returns(new UserInfo { Id = user.Id, Email = user.Email });

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Token.Should().Be("test-jwt-token");
        result.Message.Should().Be("Login successful");

        _unitOfWorkMock.Verify(x => x.CompleteAsync(default), Times.AtLeastOnce());
    }

    [Fact]
    public async Task LoginAsync_InvalidCaptcha_ShouldReturnError()
    {
        // Arrange
        var loginRequest = TestDataBuilder.CreateLoginRequest(captchaToken: "invalid-captcha");

        _captchaServiceMock.Setup(x => x.ValidateCaptchaAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid CAPTCHA. Please try again.");
        result.Data.Should().BeNull();

        _userRepositoryMock.Verify(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), default), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_BlockedIp_ShouldReturnError()
    {
        // Arrange
        var loginRequest = TestDataBuilder.CreateLoginRequest();
        var blockedIp = TestDataBuilder.CreateBlockedIp(ipAddress: loginRequest.IpAddress!);

        _captchaServiceMock.Setup(x => x.ValidateCaptchaAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _blockedIpRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<BlockedIp, bool>>>(), default))
            .ReturnsAsync(blockedIp);

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("blocked");
        result.Data.Should().BeNull();

        _userRepositoryMock.Verify(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), default), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ShouldReturnError()
    {
        // Arrange
        var loginRequest = TestDataBuilder.CreateLoginRequest();

        _captchaServiceMock.Setup(x => x.ValidateCaptchaAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _blockedIpRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<BlockedIp, bool>>>(), default))
            .ReturnsAsync((BlockedIp?)null);

        _userRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), default))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid email or password");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_IncorrectPassword_ShouldReturnError()
    {
        // Arrange
        var loginRequest = TestDataBuilder.CreateLoginRequest();
        var user = TestDataBuilder.CreateUser(email: loginRequest.Email);

        _captchaServiceMock.Setup(x => x.ValidateCaptchaAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _blockedIpRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<BlockedIp, bool>>>(), default))
            .ReturnsAsync((BlockedIp?)null);

        _userRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), default))
            .ReturnsAsync(user);

        _passwordHasherMock.Setup(x => x.VerifyPassword(loginRequest.Password, user.PasswordHash, user.PasswordSalt))
            .Returns(false);

        _loginAttemptRepositoryMock.Setup(x => x.FindAsync(
            It.IsAny<Expression<Func<LoginAttempt, bool>>>(),
            null, null, null, default))
            .ReturnsAsync(new List<LoginAttempt>());

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid email or password");
        result.Data.Should().BeNull();

        _userRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.CompleteAsync(default), Times.AtLeastOnce());
    }

    [Fact]
    public async Task LoginAsync_AccountLocked_ShouldReturnError()
    {
        // Arrange
        var loginRequest = TestDataBuilder.CreateLoginRequest();
        var user = TestDataBuilder.CreateUser(email: loginRequest.Email);
        user.LockoutEndDate = DateTime.UtcNow.AddMinutes(30);

        _captchaServiceMock.Setup(x => x.ValidateCaptchaAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _blockedIpRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<BlockedIp, bool>>>(), default))
            .ReturnsAsync((BlockedIp?)null);

        _userRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), default))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Account is locked");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_InactiveUser_ShouldReturnError()
    {
        // Arrange
        var loginRequest = TestDataBuilder.CreateLoginRequest();
        var user = TestDataBuilder.CreateUser(email: loginRequest.Email, isActive: false);

        _captchaServiceMock.Setup(x => x.ValidateCaptchaAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _blockedIpRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<BlockedIp, bool>>>(), default))
            .ReturnsAsync((BlockedIp?)null);

        _userRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), default))
            .ReturnsAsync(user);

        _passwordHasherMock.Setup(x => x.VerifyPassword(loginRequest.Password, user.PasswordHash, user.PasswordSalt))
            .Returns(true);

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("inactive");
        result.Data.Should().BeNull();
    }

    #endregion

    #region Register Tests

    [Fact]
    public async Task RegisterAsync_ValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var registerRequest = TestDataBuilder.CreateRegisterRequest();

        _captchaServiceMock.Setup(x => x.ValidateCaptchaAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        _userRepositoryMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<User, bool>>>(), default))
            .ReturnsAsync(false);

        _passwordHasherMock.Setup(x => x.HashPassword(registerRequest.Password))
            .Returns(("hashedPassword", "salt"));

        _jwtServiceMock.Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("test-jwt-token");

        _mapperMock.Setup(x => x.Map<UserInfo>(It.IsAny<User>()))
            .Returns(new UserInfo { Email = registerRequest.Email });

        // Act
        var result = await _authService.RegisterAsync(registerRequest);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Token.Should().Be("test-jwt-token");
        result.Message.Should().Be("Registration successful");

        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), default), Times.Once);
        _unitOfWorkMock.Verify(x => x.CompleteAsync(default), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_InvalidCaptcha_ShouldReturnError()
    {
        // Arrange
        var registerRequest = TestDataBuilder.CreateRegisterRequest();

        _captchaServiceMock.Setup(x => x.ValidateCaptchaAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        var result = await _authService.RegisterAsync(registerRequest);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid CAPTCHA. Please try again.");
        result.Data.Should().BeNull();

        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), default), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_EmailAlreadyExists_ShouldReturnError()
    {
        // Arrange
        var registerRequest = TestDataBuilder.CreateRegisterRequest();

        _captchaServiceMock.Setup(x => x.ValidateCaptchaAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        _userRepositoryMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<User, bool>>>(), default))
            .ReturnsAsync(true);

        // Act
        var result = await _authService.RegisterAsync(registerRequest);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Email already registered");
        result.Data.Should().BeNull();

        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), default), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WithValidReferralCode_ShouldCreateManagerRole()
    {
        // Arrange
        var registerRequest = TestDataBuilder.CreateRegisterRequest(referralCode: "REF12345");
        var referralLink = TestDataBuilder.CreateReferralLink(code: "REF12345");

        _captchaServiceMock.Setup(x => x.ValidateCaptchaAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        _userRepositoryMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<User, bool>>>(), default))
            .ReturnsAsync(false);

        _referralLinkRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<ReferralLink, bool>>>(), default))
            .ReturnsAsync(referralLink);

        _passwordHasherMock.Setup(x => x.HashPassword(registerRequest.Password))
            .Returns(("hashedPassword", "salt"));

        _jwtServiceMock.Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("test-jwt-token");

        User? capturedUser = null;
        _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>(), default))
            .Callback<User, CancellationToken>((user, _) => capturedUser = user)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.RegisterAsync(registerRequest);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();

        capturedUser.Should().NotBeNull();
        capturedUser!.Role.Should().Be(UserRole.Manager);
        capturedUser.ReferredById.Should().Be(referralLink.CreatedByUserId);

        _referralLinkRepositoryMock.Verify(x => x.Update(It.IsAny<ReferralLink>()), Times.Once);
    }

    #endregion

    #region Other Methods Tests

    [Fact]
    public async Task ValidateReferralCodeAsync_ValidCode_ShouldReturnTrue()
    {
        // Arrange
        var referralCode = "REF12345";
        var referralLink = TestDataBuilder.CreateReferralLink(code: referralCode);

        _referralLinkRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<ReferralLink, bool>>>(), default))
            .ReturnsAsync(referralLink);

        // Act
        var result = await _authService.ValidateReferralCodeAsync(referralCode);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateReferralCodeAsync_InvalidCode_ShouldReturnFalse()
    {
        // Arrange
        var referralCode = "INVALID";

        _referralLinkRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<ReferralLink, bool>>>(), default))
            .ReturnsAsync((ReferralLink?)null);

        // Act
        var result = await _authService.ValidateReferralCodeAsync(referralCode);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsIpBlockedAsync_BlockedIp_ShouldReturnTrue()
    {
        // Arrange
        var ipAddress = "192.168.1.100";
        var blockedIp = TestDataBuilder.CreateBlockedIp(ipAddress: ipAddress);

        _blockedIpRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<BlockedIp, bool>>>(), default))
            .ReturnsAsync(blockedIp);

        // Act
        var result = await _authService.IsIpBlockedAsync(ipAddress);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsIpBlockedAsync_NotBlockedIp_ShouldReturnFalse()
    {
        // Arrange
        var ipAddress = "192.168.1.200";

        _blockedIpRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<BlockedIp, bool>>>(), default))
            .ReturnsAsync((BlockedIp?)null);

        // Act
        var result = await _authService.IsIpBlockedAsync(ipAddress);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
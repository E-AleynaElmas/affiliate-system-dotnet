using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using AffiliateSystem.API.Controllers;
using AffiliateSystem.Application.DTOs.Auth;
using AffiliateSystem.Application.DTOs.Common;
using AffiliateSystem.Application.Interfaces;
using AffiliateSystem.Tests.Helpers;
using Microsoft.AspNetCore.Http;

namespace AffiliateSystem.Tests.Integration;

/// <summary>
/// Integration tests for AuthController
/// </summary>
public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<ILogger<AuthController>> _loggerMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _loggerMock = new Mock<ILogger<AuthController>>();
        _controller = new AuthController(_authServiceMock.Object, _loggerMock.Object);

        // Setup HTTP context
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.1");
        httpContext.Request.Headers["User-Agent"] = "Test Browser";

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    [Fact]
    public async Task Login_ValidCredentials_ShouldReturnOk()
    {
        // Arrange
        var loginRequest = TestDataBuilder.CreateLoginRequest();
        var loginResponse = new LoginResponse
        {
            Token = "test-jwt-token",
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            User = new UserInfo { Email = loginRequest.Email }
        };

        _authServiceMock.Setup(x => x.LoginAsync(It.IsAny<LoginRequest>()))
            .ReturnsAsync(BaseResponse<LoginResponse>.SuccessResponse(loginResponse, "Login successful"));

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeOfType<BaseResponse<LoginResponse>>();

        var response = okResult.Value as BaseResponse<LoginResponse>;
        response!.Success.Should().BeTrue();
        response.Data!.Token.Should().Be("test-jwt-token");
    }

    [Fact]
    public async Task Login_InvalidCredentials_ShouldReturnBadRequest()
    {
        // Arrange
        var loginRequest = TestDataBuilder.CreateLoginRequest();

        _authServiceMock.Setup(x => x.LoginAsync(It.IsAny<LoginRequest>()))
            .ReturnsAsync(BaseResponse<LoginResponse>.ErrorResponse("Invalid email or password"));

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().BeOfType<BaseResponse<LoginResponse>>();

        var response = badRequestResult.Value as BaseResponse<LoginResponse>;
        response!.Success.Should().BeFalse();
        response.Message.Should().Be("Invalid email or password");
    }

    [Fact]
    public async Task Login_ShouldExtractClientIpAddress()
    {
        // Arrange
        var loginRequest = TestDataBuilder.CreateLoginRequest();
        loginRequest.IpAddress = null; // Will be set by controller

        LoginRequest? capturedRequest = null;
        _authServiceMock.Setup(x => x.LoginAsync(It.IsAny<LoginRequest>()))
            .Callback<LoginRequest>(req => capturedRequest = req)
            .ReturnsAsync(BaseResponse<LoginResponse>.SuccessResponse(new LoginResponse(), "Success"));

        // Act
        await _controller.Login(loginRequest);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.IpAddress.Should().Be("192.168.1.1");
        capturedRequest.UserAgent.Should().Be("Test Browser");
    }

    [Fact]
    public async Task Login_WithForwardedHeader_ShouldUseForwardedIp()
    {
        // Arrange
        _controller.HttpContext.Request.Headers["X-Forwarded-For"] = "10.0.0.1, 192.168.1.1";

        var loginRequest = TestDataBuilder.CreateLoginRequest();
        LoginRequest? capturedRequest = null;
        _authServiceMock.Setup(x => x.LoginAsync(It.IsAny<LoginRequest>()))
            .Callback<LoginRequest>(req => capturedRequest = req)
            .ReturnsAsync(BaseResponse<LoginResponse>.SuccessResponse(new LoginResponse(), "Success"));

        // Act
        await _controller.Login(loginRequest);

        // Assert
        capturedRequest!.IpAddress.Should().Be("10.0.0.1");
    }

    [Fact]
    public async Task Login_ServiceThrowsException_ShouldReturn500()
    {
        // Arrange
        var loginRequest = TestDataBuilder.CreateLoginRequest();

        _authServiceMock.Setup(x => x.LoginAsync(It.IsAny<LoginRequest>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Register_ValidRequest_ShouldReturnOk()
    {
        // Arrange
        var registerRequest = TestDataBuilder.CreateRegisterRequest();
        var loginResponse = new LoginResponse
        {
            Token = "test-jwt-token",
            User = new UserInfo { Email = registerRequest.Email }
        };

        _authServiceMock.Setup(x => x.RegisterAsync(It.IsAny<RegisterRequest>()))
            .ReturnsAsync(BaseResponse<LoginResponse>.SuccessResponse(loginResponse, "Registration successful"));

        // Act
        var result = await _controller.Register(registerRequest);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as BaseResponse<LoginResponse>;
        response!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Register_EmailAlreadyExists_ShouldReturnBadRequest()
    {
        // Arrange
        var registerRequest = TestDataBuilder.CreateRegisterRequest();

        _authServiceMock.Setup(x => x.RegisterAsync(It.IsAny<RegisterRequest>()))
            .ReturnsAsync(BaseResponse<LoginResponse>.ErrorResponse("Email already registered"));

        // Act
        var result = await _controller.Register(registerRequest);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        var response = badRequestResult!.Value as BaseResponse<LoginResponse>;
        response!.Success.Should().BeFalse();
        response.Message.Should().Be("Email already registered");
    }

    [Fact]
    public async Task ValidateReferralCode_ValidCode_ShouldReturnOk()
    {
        // Arrange
        var referralCode = "REF12345";

        _authServiceMock.Setup(x => x.ValidateReferralCodeAsync(referralCode))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ValidateReferralCode(referralCode);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        dynamic response = okResult!.Value!;
        ((bool)response.isValid).Should().BeTrue();
    }

    [Fact]
    public async Task ValidateReferralCode_InvalidCode_ShouldReturnOkWithFalse()
    {
        // Arrange
        var referralCode = "INVALID";

        _authServiceMock.Setup(x => x.ValidateReferralCodeAsync(referralCode))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.ValidateReferralCode(referralCode);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        dynamic response = okResult!.Value!;
        ((bool)response.isValid).Should().BeFalse();
    }

    [Fact]
    public async Task CheckIpStatus_BlockedIp_ShouldReturnOkWithTrue()
    {
        // Arrange
        var ipAddress = "192.168.1.100";

        _authServiceMock.Setup(x => x.IsIpBlockedAsync(ipAddress))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.CheckIpStatus(ipAddress);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        dynamic response = okResult!.Value!;
        ((bool)response.isBlocked).Should().BeTrue();
    }

    [Fact]
    public async Task CheckIpStatus_NotBlockedIp_ShouldReturnOkWithFalse()
    {
        // Arrange
        var ipAddress = "192.168.1.200";

        _authServiceMock.Setup(x => x.IsIpBlockedAsync(ipAddress))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.CheckIpStatus(ipAddress);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        dynamic response = okResult!.Value!;
        ((bool)response.isBlocked).Should().BeFalse();
    }
}
using AutoMapper;
using AffiliateSystem.Application.DTOs.Auth;
using AffiliateSystem.Application.DTOs.Common;
using AffiliateSystem.Application.Interfaces;
using AffiliateSystem.Domain.Entities;
using AffiliateSystem.Domain.Enums;
using AffiliateSystem.Domain.Interfaces;

namespace AffiliateSystem.Application.Services;

/// <summary>
/// Authentication service implementation
/// </summary>
public class AuthService : IAuthService
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<LoginAttempt> _loginAttemptRepository;
    private readonly IRepository<BlockedIp> _blockedIpRepository;
    private readonly IRepository<ReferralLink> _referralLinkRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly ICaptchaService _captchaService;
    private readonly IMapper _mapper;
    private readonly IIpBlockingService _ipBlockingService;
    private readonly ILoginAttemptService _loginAttemptService;

    public AuthService(
        IRepository<User> userRepository,
        IRepository<LoginAttempt> loginAttemptRepository,
        IRepository<BlockedIp> blockedIpRepository,
        IRepository<ReferralLink> referralLinkRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        ICaptchaService captchaService,
        IMapper mapper,
        IIpBlockingService ipBlockingService,
        ILoginAttemptService loginAttemptService)
    {
        _userRepository = userRepository;
        _loginAttemptRepository = loginAttemptRepository;
        _blockedIpRepository = blockedIpRepository;
        _referralLinkRepository = referralLinkRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _captchaService = captchaService;
        _mapper = mapper;
        _ipBlockingService = ipBlockingService;
        _loginAttemptService = loginAttemptService;
    }

    /// <summary>
    /// User login
    /// </summary>
    public async Task<BaseResponse<LoginResponse>> LoginAsync(LoginRequest request)
    {
        // Validate CAPTCHA
        if (!string.IsNullOrEmpty(request.CaptchaToken))
        {
            var captchaValid = await _captchaService.ValidateCaptchaAsync(request.CaptchaToken, request.IpAddress);
            if (!captchaValid)
            {
                await RecordLoginAttemptWithServiceAsync(request.Email, request.IpAddress ?? "", false, request.UserAgent, "Invalid CAPTCHA");
                return BaseResponse<LoginResponse>.ErrorResponse("Invalid CAPTCHA. Please try again.");
            }
        }

        // Check if IP is blocked using the new service
        if (!string.IsNullOrEmpty(request.IpAddress) && await _ipBlockingService.IsBlockedAsync(request.IpAddress))
        {
            // Don't record attempt when IP is already blocked
            return BaseResponse<LoginResponse>.ErrorResponse("Your IP address has been temporarily blocked. Please try again later.");
        }

        // Find user
        var user = await _userRepository.SingleOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            await RecordLoginAttemptWithServiceAsync(request.Email, request.IpAddress ?? "", false, request.UserAgent, "User not found");
            return BaseResponse<LoginResponse>.ErrorResponse("Invalid email or password");
        }

        // Check if account is locked
        if (user.LockoutEndDate.HasValue && user.LockoutEndDate > DateTime.UtcNow)
        {
            await RecordLoginAttemptWithServiceAsync(request.Email, request.IpAddress ?? "", false, request.UserAgent, "Account locked", user.Id);
            return BaseResponse<LoginResponse>.ErrorResponse($"Account is locked until {user.LockoutEndDate.Value:yyyy-MM-dd HH:mm}");
        }

        // Verify password
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            user.FailedLoginAttempts++;

            // Lock account after 5 failed attempts
            if (user.FailedLoginAttempts >= 5)
            {
                user.LockoutEndDate = DateTime.UtcNow.AddMinutes(30);
            }

            _userRepository.Update(user);
            await _unitOfWork.CompleteAsync();

            await RecordLoginAttemptWithServiceAsync(request.Email, request.IpAddress ?? "", false, request.UserAgent, "Invalid password", user.Id);

            return BaseResponse<LoginResponse>.ErrorResponse("Invalid email or password");
        }

        // Check if user is active
        if (!user.IsActive)
        {
            await RecordLoginAttemptWithServiceAsync(request.Email, request.IpAddress ?? "", false, request.UserAgent, "Account inactive", user.Id);
            return BaseResponse<LoginResponse>.ErrorResponse("Your account is inactive. Please contact support.");
        }

        // Successful login
        user.FailedLoginAttempts = 0;
        user.LastLoginAt = DateTime.UtcNow;
        _userRepository.Update(user);
        await _unitOfWork.CompleteAsync();

        await RecordLoginAttemptWithServiceAsync(request.Email, request.IpAddress ?? "", true, request.UserAgent, null, user.Id);

        // Create response with JWT token
        var response = new LoginResponse
        {
            Token = _jwtService.GenerateToken(user),
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            User = _mapper.Map<UserInfo>(user)
        };

        return BaseResponse<LoginResponse>.SuccessResponse(response, "Login successful");
    }

    /// <summary>
    /// User registration
    /// </summary>
    public async Task<BaseResponse<LoginResponse>> RegisterAsync(RegisterRequest request)
    {
        // Validate CAPTCHA for registration
        if (!string.IsNullOrEmpty(request.CaptchaToken))
        {
            var captchaValid = await _captchaService.ValidateCaptchaAsync(request.CaptchaToken);
            if (!captchaValid)
            {
                return BaseResponse<LoginResponse>.ErrorResponse("Invalid CAPTCHA. Please try again.");
            }
        }

        // Check if email already exists
        if (await _userRepository.AnyAsync(u => u.Email == request.Email))
        {
            return BaseResponse<LoginResponse>.ErrorResponse("Email already registered");
        }

        // Hash password
        var (hash, salt) = _passwordHasher.HashPassword(request.Password);

        // Create new user
        var user = new User
        {
            Email = request.Email,
            PasswordHash = hash,
            PasswordSalt = salt,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = UserRole.Customer,
            IsActive = true,
            EmailConfirmed = false
        };

        // Check referral code
        if (!string.IsNullOrEmpty(request.ReferralCode))
        {
            var referralLink = await _referralLinkRepository.SingleOrDefaultAsync(r => r.Code == request.ReferralCode);

            if (referralLink != null && referralLink.CanBeUsed())
            {
                // Set role as Manager if referral is valid
                user.Role = UserRole.Manager;
                user.ReferredById = referralLink.CreatedByUserId;

                // Update referral link usage
                referralLink.UsageCount++;
                _referralLinkRepository.Update(referralLink);
            }
        }

        await _userRepository.AddAsync(user);
        await _unitOfWork.CompleteAsync();

        // Auto-login after registration with JWT token
        var response = new LoginResponse
        {
            Token = _jwtService.GenerateToken(user),
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            User = _mapper.Map<UserInfo>(user)
        };

        return BaseResponse<LoginResponse>.SuccessResponse(response, "Registration successful");
    }

    /// <summary>
    /// Validate referral code
    /// </summary>
    public async Task<bool> ValidateReferralCodeAsync(string referralCode)
    {
        var referralLink = await _referralLinkRepository.SingleOrDefaultAsync(r => r.Code == referralCode);
        return referralLink != null && referralLink.CanBeUsed();
    }

    /// <summary>
    /// Check if IP is blocked
    /// </summary>
    public async Task<bool> IsIpBlockedAsync(string ipAddress)
    {
        var blockedIp = await _blockedIpRepository.SingleOrDefaultAsync(b => b.IpAddress == ipAddress);
        return blockedIp != null && blockedIp.IsActive;
    }

    /// <summary>
    /// Record login attempt
    /// </summary>
    public async Task RecordLoginAttemptAsync(string email, string ipAddress, bool isSuccessful,
        string? userAgent = null, string? failureReason = null)
    {
        var attempt = new LoginAttempt
        {
            Email = email,
            IpAddress = ipAddress,
            IsSuccessful = isSuccessful,
            UserAgent = userAgent,
            FailureReason = failureReason
        };

        if (isSuccessful)
        {
            var user = await _userRepository.SingleOrDefaultAsync(u => u.Email == email);
            if (user != null)
            {
                attempt.UserId = user.Id;
            }
        }

        await _loginAttemptRepository.AddAsync(attempt);
        await _unitOfWork.CompleteAsync();
    }

    /// <summary>
    /// Check and block IP if necessary
    /// </summary>
    private async Task CheckAndBlockIpAsync(string ipAddress)
    {
        var failedAttempts = await _loginAttemptRepository.FindAsync(
            a => a.IpAddress == ipAddress &&
            !a.IsSuccessful &&
            a.CreatedAt > DateTime.UtcNow.AddHours(-1));

        if (failedAttempts.Count() >= 10)
        {
            var existingBlock = await _blockedIpRepository.SingleOrDefaultAsync(b => b.IpAddress == ipAddress);

            if (existingBlock == null)
            {
                var blockedIp = new BlockedIp
                {
                    IpAddress = ipAddress,
                    Reason = "10 failed login attempts within 1 hour",
                    BlockedUntil = DateTime.UtcNow.AddHours(24),
                    FailedAttemptCount = failedAttempts.Count()
                };

                await _blockedIpRepository.AddAsync(blockedIp);
                await _unitOfWork.CompleteAsync();
            }
        }
    }

    /// <summary>
    /// Record login attempt using the new service
    /// </summary>
    private async Task RecordLoginAttemptWithServiceAsync(string email, string ipAddress, bool isSuccessful,
        string? userAgent, string? failureReason = null, Guid? userId = null)
    {
        var attemptDto = new DTOs.User.LoginAttemptDto
        {
            UserId = userId,
            Email = email,
            IpAddress = ipAddress,
            IsSuccessful = isSuccessful,
            AttemptedAt = DateTime.UtcNow,
            UserAgent = userAgent,
            FailureReason = failureReason
        };

        await _loginAttemptService.RecordAttemptAsync(attemptDto);
    }
}
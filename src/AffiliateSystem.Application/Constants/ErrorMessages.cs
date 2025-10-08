namespace AffiliateSystem.Application.Constants;

/// <summary>
/// Centralized error messages for consistency
/// </summary>
public static class ErrorMessages
{
    // Authentication
    public const string InvalidCaptcha = "Invalid CAPTCHA. Please try again.";
    public const string IpBlocked = "Your IP address has been blocked due to multiple failed login attempts.";
    public const string InvalidCredentials = "Invalid email or password";
    public const string AccountLocked = "Account is locked until {0:yyyy-MM-dd HH:mm}";
    public const string AccountInactive = "Your account is inactive. Please contact support.";

    // Registration
    public const string EmailAlreadyRegistered = "Email already registered";
    public const string InvalidReferralCode = "Invalid or expired referral code";

    // User Management
    public const string UserNotFound = "User not found";
    public const string CurrentPasswordIncorrect = "Current password is incorrect";
    public const string PasswordChangedSuccessfully = "Password changed successfully";
    public const string ProfileUpdatedSuccessfully = "Profile updated successfully";

    // Authorization
    public const string UnauthorizedAccess = "You are not authorized to perform this action";
    public const string InsufficientPrivileges = "Insufficient privileges for this operation";
    public const string OnlyManagersCanCreateReferralLinks = "Only managers and admins can create referral links";

    // Validation
    public const string InvalidEmailFormat = "Please provide a valid email address";
    public const string PasswordTooWeak = "Password must be at least {0} characters long";
    public const string PasswordsDoNotMatch = "Password confirmation does not match";

    // Success Messages
    public const string LoginSuccessful = "Login successful";
    public const string RegistrationSuccessful = "Registration successful";
    public const string ReferralLinkCreatedSuccessfully = "Referral link created successfully";
    public const string UserActivatedSuccessfully = "User {0} successfully";
    public const string UserDeletedSuccessfully = "User deleted successfully";

    // Generic
    public const string OperationFailed = "Operation failed. Please try again.";
    public const string UnexpectedError = "An unexpected error occurred. Please try again later.";
    public const string InvalidRequest = "Invalid request data";
}
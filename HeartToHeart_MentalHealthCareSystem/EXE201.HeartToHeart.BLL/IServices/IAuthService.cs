using EXE201.HeartToHeart.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.BLL.IServices
{
    public interface IAuthService
    {
        #region Registration Methods

        /// <summary>
        /// Register a new Member (basic user) - This is the default registration for regular users
        /// </summary>
        Task<AuthResult> RegisterAsync(RegisterDto registerDto);

        /// <summary>
        /// Register a new Member (basic user) - Explicit method for member registration
        /// </summary>
        Task<AuthResult> RegisterMemberAsync(RegisterDto registerDto);

        /// <summary>
        /// Register a new Counselor - Only accessible by Admin or Staff
        /// </summary>
        Task<AuthResult> RegisterCounselorAsync(RegisterCounselorDto registerDto, Guid createdByUserId);

        /// <summary>
        /// Register a new Staff member - Only accessible by Admin
        /// </summary>
        Task<AuthResult> RegisterStaffAsync(RegisterDto registerDto);

        /// <summary>
        /// Register a new Admin - Only accessible by existing Admin (Super Admin functionality)
        /// </summary>
        Task<AuthResult> RegisterAdminAsync(RegisterDto registerDto);

        #endregion

        #region Authentication Methods

        /// <summary>
        /// Login method - Works for all user types (Member, Premium, Counselor, Staff, Admin)
        /// </summary>
        Task<AuthResult> LoginAsync(LoginDto loginDto);

        /// <summary>
        /// External login method - Handles Google, Facebook, etc. login
        /// </summary>
        Task<AuthResult> ExternalLoginAsync(ExternalLoginDto externalLoginDto);

        /// <summary>
        /// Google OAuth login - Enhanced version with proper user info extraction
        /// </summary>
        Task<AuthResult> GoogleLoginAsync(string googleAccessToken);

        /// <summary>
        /// Logout method - Works for all authenticated users
        /// </summary>
        Task<bool> LogoutAsync(Guid userId);

        #endregion

        #region Token Management

        /// <summary>
        /// Refresh token method - Works for all authenticated users
        /// </summary>
        Task<AuthResult> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);

        #endregion

        #region Subscription Management

        /// <summary>
        /// Upgrade Member to Premium - This should be called after successful payment
        /// </summary>
        Task<AuthResult> UpgradeToPremiumAsync(Guid userId);

        /// <summary>
        /// Downgrade Premium to Member - This should be called when subscription expires or is cancelled
        /// </summary>
        Task<AuthResult> DowngradeToMemberAsync(Guid userId);

        #endregion

        #region Password Management

        /// <summary>
        /// Change password method - Works for all authenticated users
        /// </summary>
        Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto);

        /// <summary>
        /// Forgot password method - Sends password reset email
        /// </summary>
        Task<AuthResult> ForgotPasswordAsync(string email);

        /// <summary>
        /// Reset password method - Resets password using token
        /// </summary>
        Task<AuthResult> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);

        #endregion

        #region Email Management

        /// <summary>
        /// Confirm email method - Confirms user email using token
        /// </summary>
        Task<bool> ConfirmEmailAsync(Guid userId, string token);

        /// <summary>
        /// Resend email confirmation - Resends confirmation email
        /// </summary>
        Task<AuthResult> ResendEmailConfirmationAsync(string email);

        #endregion

        #region Authorization & Permissions

        /// <summary>
        /// Check if user has specific permission - Used for authorization
        /// </summary>
        Task<bool> HasPermissionAsync(Guid userId, string permission);

        /// <summary>
        /// Get user roles - Returns the roles assigned to a user
        /// </summary>
        Task<List<string>> GetUserRolesAsync(Guid userId);

        #endregion

        #region User Management (Admin/Staff Functions)

        /// <summary>
        /// Deactivate user account - Admin/Staff functionality
        /// </summary>
        Task<bool> DeactivateUserAsync(Guid userId);

        /// <summary>
        /// Activate user account - Admin/Staff functionality
        /// </summary>
        Task<bool> ActivateUserAsync(Guid userId);

        #endregion
    }
}
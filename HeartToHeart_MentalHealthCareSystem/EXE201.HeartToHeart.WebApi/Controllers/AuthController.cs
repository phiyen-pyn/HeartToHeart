using EXE201.HeartToHeart.BLL.IServices;
using EXE201.HeartToHeart.Common.Constants;
using EXE201.HeartToHeart.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EXE201.HeartToHeart.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Public registration for Members (regular users)
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(registerDto);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Public registration for Members (explicit method)
        /// </summary>
        [HttpPost("register/member")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterMember([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterMemberAsync(registerDto);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Register Counselor - Only accessible by Admin or Staff
        /// </summary>

        [HttpPost("register/counselor")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Staff}")]
        public async Task<IActionResult> RegisterCounselor([FromBody] RegisterCounselorDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // FIXED: Get the current user ID (Admin/Staff who is creating the counselor)
            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty)
                return Unauthorized(new { Message = "Unable to identify current user" });

            // FIXED: Pass the current user ID to track who created the counselor
            var result = await _authService.RegisterCounselorAsync(registerDto, currentUserId);

            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Register Staff - Only accessible by Admin
        /// </summary>
        [HttpPost("register/staff")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> RegisterStaff([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterStaffAsync(registerDto);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Register Admin - Only accessible by existing Admin
        /// </summary>
        [HttpPost("register/admin")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterAdminAsync(registerDto);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Login for all user types
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(loginDto);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Google OAuth login - New method added
        /// </summary>
        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto googleLoginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrEmpty(googleLoginDto.AccessToken))
                return BadRequest(new { Message = "Google access token is required" });

            var result = await _authService.GoogleLoginAsync(googleLoginDto.AccessToken);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Upgrade Member to Premium - Requires Member role
        /// </summary>
        [HttpPost("upgrade-to-premium")]
        [Authorize(Roles = Roles.Member)]
        public async Task<IActionResult> UpgradeToPremium()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized(new { Message = "Invalid user" });

            var result = await _authService.UpgradeToPremiumAsync(userId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Downgrade Premium to Member - Requires Premium role or Admin/Staff
        /// </summary>
        [HttpPost("downgrade-to-member")]
        [Authorize(Roles = $"{Roles.Premium},{Roles.Admin},{Roles.Staff}")]
        public async Task<IActionResult> DowngradeToMember([FromQuery] Guid? targetUserId = null)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty)
                return Unauthorized(new { Message = "Invalid user" });

            // If targetUserId is provided, check if current user has permission to downgrade others
            var userToDowngrade = targetUserId ?? currentUserId;

            if (targetUserId.HasValue && targetUserId != currentUserId)
            {
                var currentUserRoles = GetCurrentUserRoles();
                if (!currentUserRoles.Contains(Roles.Admin) && !currentUserRoles.Contains(Roles.Staff))
                {
                    return Forbid("Only Admin or Staff can downgrade other users");
                }
            }

            var result = await _authService.DowngradeToMemberAsync(userToDowngrade);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Refresh JWT token
        /// </summary>
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RefreshTokenAsync(refreshTokenDto);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// External login (Google, Facebook, etc.)
        /// </summary>
        [HttpPost("external-login")]
        public async Task<IActionResult> ExternalLogin([FromBody] ExternalLoginDto externalLoginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.ExternalLoginAsync(externalLoginDto);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Logout current user
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized(new { Message = "Invalid user" });

            var result = await _authService.LogoutAsync(userId);
            if (result)
                return Ok(new { Message = "Logout successful" });
            return BadRequest(new { Message = "Logout failed" });
        }

        /// <summary>
        /// Change password for current user
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized(new { Message = "Invalid user" });

            var result = await _authService.ChangePasswordAsync(userId, changePasswordDto);
            if (result)
                return Ok(new { Message = "Password changed successfully" });
            return BadRequest(new { Message = "Password change failed" });
        }

        /// <summary>
        /// Forgot password - Send reset email
        /// </summary>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.ForgotPasswordAsync(forgotPasswordDto.Email);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Reset password using token
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.ResetPasswordAsync(resetPasswordDto);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// FIXED: Confirm email using token - Changed to GET method to handle email links
        /// </summary>
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] Guid userId, [FromQuery] string token)
        {
            if (userId == Guid.Empty || string.IsNullOrEmpty(token))
                return BadRequest(new { Message = "Invalid confirmation parameters" });

            var decodedToken = Uri.UnescapeDataString(token);
            var result = await _authService.ConfirmEmailAsync(userId, decodedToken);

            var htmlSuccess = $@"
                                <!DOCTYPE html>
                                <html lang='vi'>
                                <head>
                                    <meta charset='UTF-8'>
                                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                                    <title>Xác Nhận Email Thành Công - HeartToHeart</title>
                                    <style>
                                        @import url('https://fonts.googleapis.com/css2?family=Poppins:wght@400;600;700&display=swap');
                                        body {{
                                            margin: 0; padding: 0;
                                            font-family: 'Poppins', Arial, sans-serif;
                                            background: linear-gradient(135deg, #FFFFFF 0%, #FADEDA 50%, #ACB9E7 100%);
                                        }}
                                        .container {{
                                            max-width: 600px;
                                            margin: 80px auto;
                                            background: #fff;
                                            border-radius: 20px;
                                            padding: 40px;
                                            box-shadow: 0 20px 40px rgba(172,185,231,0.3);
                                            text-align: center;
                                        }}
                                        .icon {{
                                            font-size: 48px;
                                            color: #4CAF50;
                                            margin-bottom: 20px;
                                        }}
                                        h2 {{
                                            font-size: 26px;
                                            font-weight: 700;
                                            color: #4a4a4a;
                                            margin-bottom: 10px;
                                        }}
                                        p {{
                                            color: #6b6b6b;
                                            font-size: 16px;
                                            margin-bottom: 30px;
                                        }}
                                        .btn {{
                                            background: linear-gradient(135deg, #ACB9E7, #FADEDA);
                                            color: #4a4a4a;
                                            padding: 14px 30px;
                                            text-decoration: none;
                                            border-radius: 50px;
                                            font-weight: 600;
                                            box-shadow: 0 10px 30px rgba(172,185,231,0.4);
                                            transition: all 0.3s ease;
                                        }}
                                        .btn:hover {{
                                            transform: translateY(-2px);
                                            box-shadow: 0 15px 40px rgba(172,185,231,0.6);
                                        }}
                                    </style>
                                </head>
                                <body>
                                    <div class='container'>
                                        <div class='icon'>✅</div>
                                        <h2>Email Xác Nhận Thành Công!</h2>
                                        <p>Cảm ơn bạn đã xác nhận email. Tài khoản của bạn hiện đã được kích hoạt và sẵn sàng sử dụng HeartToHeart.</p>
                                        <a href='https://hearttoheart.vercel.app/login' class='btn'>Đăng Nhập Ngay</a>
                                    </div>
                                </body>
                                </html>";

            var htmlFailure = $@"
                                <!DOCTYPE html>
                                <html lang='vi'>
                                <head>
                                    <meta charset='UTF-8'>
                                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                                    <title>Xác Nhận Email Thất Bại - HeartToHeart</title>
                                    <style>
                                        @import url('https://fonts.googleapis.com/css2?family=Poppins:wght@400;600;700&display=swap');
                                        body {{
                                            margin: 0; padding: 0;
                                            font-family: 'Poppins', Arial, sans-serif;
                                            background: linear-gradient(135deg, #FFFFFF 0%, #FADEDA 50%, #ACB9E7 100%);
                                        }}
                                        .container {{
                                            max-width: 600px;
                                            margin: 80px auto;
                                            background: #fff;
                                            border-radius: 20px;
                                            padding: 40px;
                                            box-shadow: 0 20px 40px rgba(172,185,231,0.3);
                                            text-align: center;
                                        }}
                                        .icon {{
                                            font-size: 48px;
                                            color: #e74c3c;
                                            margin-bottom: 20px;
                                        }}
                                        h2 {{
                                            font-size: 26px;
                                            font-weight: 700;
                                            color: #4a4a4a;
                                            margin-bottom: 10px;
                                        }}
                                        p {{
                                            color: #6b6b6b;
                                            font-size: 16px;
                                            margin-bottom: 30px;
                                        }}
                                        .btn {{
                                            background: linear-gradient(135deg, #FADEDA, #ACB9E7);
                                            color: #4a4a4a;
                                            padding: 14px 30px;
                                            text-decoration: none;
                                            border-radius: 50px;
                                            font-weight: 600;
                                            box-shadow: 0 10px 30px rgba(172,185,231,0.4);
                                            transition: all 0.3s ease;
                                        }}
                                        .btn:hover {{
                                            transform: translateY(-2px);
                                            box-shadow: 0 15px 40px rgba(172,185,231,0.6);
                                        }}
                                    </style>
                                </head>
                                <body>
                                    <div class='container'>
                                        <div class='icon'>❌</div>
                                        <h2>Xác Nhận Email Thất Bại</h2>
                                        <p>Liên kết xác nhận không hợp lệ hoặc đã hết hạn. Vui lòng yêu cầu gửi lại liên kết xác nhận mới.</p>
                                        <a href='https://localhost:7166/resend-confirmation' class='btn'>Gửi Lại Liên Kết</a>
                                    </div>
                                </body>
                                </html>";

            return Content(result ? htmlSuccess : htmlFailure, "text/html");
        }
        /// <summary>
        /// ADDITIONAL: API endpoint for programmatic email confirmation (for API testing)
        /// </summary>
        [HttpPost("confirm-email-api")]
        public async Task<IActionResult> ConfirmEmailApi([FromBody] ConfirmEmailDto confirmEmailDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (confirmEmailDto.UserId == Guid.Empty || string.IsNullOrEmpty(confirmEmailDto.Token))
                return BadRequest(new { Message = "Invalid confirmation parameters" });

            var result = await _authService.ConfirmEmailAsync(confirmEmailDto.UserId, confirmEmailDto.Token);
            if (result)
                return Ok(new { Message = "Email confirmed successfully" });
            return BadRequest(new { Message = "Email confirmation failed" });
        }

        /// <summary>
        /// Resend email confirmation
        /// </summary>
        [HttpPost("resend-email-confirmation")]
        public async Task<IActionResult> ResendEmailConfirmation([FromBody] ResendEmailConfirmationDto resendEmailDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.ResendEmailConfirmationAsync(resendEmailDto.Email);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Check if current user has specific permission
        /// </summary>
        [HttpGet("check-permission")]
        [Authorize]
        public async Task<IActionResult> CheckPermission([FromQuery] string permission)
        {
            if (string.IsNullOrEmpty(permission))
                return BadRequest(new { Message = "Permission parameter is required" });

            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized(new { Message = "Invalid user" });

            var hasPermission = await _authService.HasPermissionAsync(userId, permission);
            if (hasPermission)
                return Ok(new { Message = $"User has permission: {permission}", HasPermission = true });
            return Forbid(new { Message = $"User does not have permission: {permission}", HasPermission = false });
        }

        /// <summary>
        /// Get current user's roles
        /// </summary>
        [HttpGet("user-roles")]
        [Authorize]
        public async Task<IActionResult> GetUserRoles()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized(new { Message = "Invalid user" });

            var roles = await _authService.GetUserRolesAsync(userId);
            return Ok(new { UserId = userId, Roles = roles });
        }

        /// <summary>
        /// Deactivate user - Admin/Staff only
        /// </summary>
        [HttpPost("deactivate-user")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Staff}")]
        public async Task<IActionResult> DeactivateUser([FromBody] UserActionDto userActionDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.DeactivateUserAsync(userActionDto.UserId);
            if (result)
                return Ok(new { Message = "User deactivated successfully" });
            return BadRequest(new { Message = "Failed to deactivate user" });
        }

        /// <summary>
        /// Activate user - Admin/Staff only
        /// </summary>
        [HttpPost("activate-user")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Staff}")]
        public async Task<IActionResult> ActivateUser([FromBody] UserActionDto userActionDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.ActivateUserAsync(userActionDto.UserId);
            if (result)
                return Ok(new { Message = "User activated successfully" });
            return BadRequest(new { Message = "Failed to activate user" });
        }

        /// <summary>
        /// Get current user info
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized(new { Message = "Invalid user" });

            var roles = await _authService.GetUserRolesAsync(userId);
            var userClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();

            return Ok(new
            {
                UserId = userId,
                Email = User.FindFirstValue(ClaimTypes.Email),
                Roles = roles,
                Claims = userClaims
            });
        }

        #region Helper Methods

        /// <summary>
        /// Get current user ID from JWT token
        /// </summary>
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdClaim, out var userId))
                return userId;
            return Guid.Empty;
        }

        /// <summary>
        /// Get current user roles from JWT token
        /// </summary>
        private List<string> GetCurrentUserRoles()
        {
            return User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        }

        /// <summary>
        /// Return Forbid result with custom message
        /// </summary>
        private IActionResult Forbid(object value)
        {
            return StatusCode(403, value);
        }

        #endregion
    }
}
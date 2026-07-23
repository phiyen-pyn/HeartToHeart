using EXE201.HeartToHeart.BLL.IServices;
using EXE201.HeartToHeart.Common.Constants;
using EXE201.HeartToHeart.DAL.Entities.Application;
using EXE201.HeartToHeart.DAL.Entities.Identity;
using EXE201.HeartToHeart.DAL.IRepositories;
using EXE201.HeartToHeart.DAL.Models;
using EXE201.HeartToHeart.DAL.Repostories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Json;

namespace EXE201.HeartToHeart.BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ICounselorRepository _counselorRepository;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager,
            ITokenService tokenService,
            IRefreshTokenRepository refreshTokenRepository,
            IEmailService emailService,
            ILogger<AuthService> logger,
            IConfiguration configuration,
            ICounselorRepository counselorRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
            _refreshTokenRepository = refreshTokenRepository;
            _emailService = emailService;
            _logger = logger;
            _configuration = configuration;
            _counselorRepository = counselorRepository;
        }

        /// <summary>
        /// Register a new Member (basic user) - This is the default registration for regular users
        /// This method is for public user registration on the platform
        /// </summary>
        public async Task<AuthResult> RegisterAsync(RegisterDto registerDto)
        {
            return await RegisterUserWithRoleAsync(registerDto, Roles.Member);
        }

        /// <summary>
        /// Register a new Member (basic user) - Explicit method for member registration
        /// This method is for public user registration on the platform
        /// </summary>
        public async Task<AuthResult> RegisterMemberAsync(RegisterDto registerDto)
        {
            return await RegisterUserWithRoleAsync(registerDto, Roles.Member);
        }

        /// <summary>
        /// Register a new Counselor - Only accessible by Admin or Staff
        /// This method is for creating counselor accounts by administrators
        /// </summary>
        public async Task<AuthResult> RegisterCounselorAsync(RegisterCounselorDto registerDto, Guid createdByUserId)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
                if (existingUser != null)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "User with this email already exists"
                    };
                }

                // Create new ApplicationUser
                var user = new ApplicationUser
                {
                    UserName = registerDto.Email,
                    Email = registerDto.Email,
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    DateOfBirth = registerDto.DateOfBirth,
                    Gender = registerDto.Gender,
                    EmailConfirmed = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Create the user with password
                var result = await _userManager.CreateAsync(user, registerDto.Password);
                if (!result.Succeeded)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = string.Join("; ", result.Errors.Select(e => e.Description))
                    };
                }

                // Assign Counselor role
                await _userManager.AddToRoleAsync(user, Roles.Counselor);

                // Create the Counselor entity with explicit audit trail using Guid.ToString()
                var counselor = new Counselor
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Description = registerDto.Bio,
                    Specialization = registerDto.Specializations,
                    LicenseNumber = registerDto.LicenseNumber,
                    ExperienceYears = registerDto.YearsOfExperience,
                    IsVerified = false,
                    HourlyRate = null,
                    IsAvailable = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    // FIXED: Set audit fields with userId converted to string
                    CreatedBy = createdByUserId.ToString(),
                    UpdatedBy = createdByUserId.ToString()
                };

                var counselorCreated = await _counselorRepository.CreateCounselorAsync(counselor);
                if (!counselorCreated)
                {
                    // If counselor creation fails, remove the user to maintain consistency
                    await _userManager.DeleteAsync(user);
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Failed to create counselor profile. User account was not created."
                    };
                }

                // Generate email confirmation token and send email
                var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                await _emailService.SendEmailConfirmationAsync(user.Email, user.FirstName, emailToken, user.Id);

                _logger.LogInformation("Counselor registered successfully: {Email} by user {CreatedBy}", registerDto.Email, createdByUserId);

                return new AuthResult
                {
                    Success = true,
                    Message = "Counselor registration successful. Please check email to confirm account.",
                    UserId = user.Id
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Counselor registration failed for email: {Email}", registerDto.Email);
                return new AuthResult
                {
                    Success = false,
                    Message = "Counselor registration failed. Please try again."
                };
            }
        }

        /// <summary>
        /// Register a new Staff member - Only accessible by Admin
        /// This method is for creating staff accounts by administrators
        /// </summary>
        public async Task<AuthResult> RegisterStaffAsync(RegisterDto registerDto)
        {
            return await RegisterUserWithRoleAsync(registerDto, Roles.Staff);
        }

        /// <summary>
        /// Register a new Admin - Only accessible by existing Admin (Super Admin functionality)
        /// This method is for creating admin accounts by existing administrators
        /// </summary>
        public async Task<AuthResult> RegisterAdminAsync(RegisterDto registerDto)
        {
            return await RegisterUserWithRoleAsync(registerDto, Roles.Admin);
        }

        /// <summary>
        /// Core registration method that handles role assignment
        /// This is the private method that contains the main registration logic
        /// </summary>
        private async Task<AuthResult> RegisterUserWithRoleAsync(RegisterDto registerDto, string role, Guid? createdByUserId = null)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
                if (existingUser != null)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "User with this email already exists"
                    };
                }

                // Create new user
                var user = new ApplicationUser
                {
                    UserName = registerDto.Email,
                    Email = registerDto.Email,
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    DateOfBirth = registerDto.DateOfBirth,
                    Gender = registerDto.Gender,
                    EmailConfirmed = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Note: ApplicationUser doesn't inherit from AuditableEntityBase, 
                // so we don't set CreatedBy/UpdatedBy here. But if you have other entities
                // that inherit from AuditableEntityBase in this flow, they will be handled
                // automatically by the DbContext.UpdateAuditFields() method.

                var result = await _userManager.CreateAsync(user, registerDto.Password);
                if (!result.Succeeded)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = string.Join("; ", result.Errors.Select(e => e.Description))
                    };
                }

                // Assign role
                await _userManager.AddToRoleAsync(user, role);

                // Generate email confirmation token and pass userId to email service
                var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                await _emailService.SendEmailConfirmationAsync(user.Email, user.FirstName, emailToken, user.Id);

                var creatorInfo = createdByUserId?.ToString() ?? "Self-Registration";
                _logger.LogInformation("User registered successfully with role {Role}: {Email} by {CreatedBy}", role, registerDto.Email, creatorInfo);

                return new AuthResult
                {
                    Success = true,
                    Message = $"Registration successful as {role}. Please check your email to confirm your account.",
                    UserId = user.Id
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed for email: {Email} with role: {Role}", registerDto.Email, role);
                return new AuthResult
                {
                    Success = false,
                    Message = "Registration failed. Please try again."
                };
            }
        }

        /// <summary>
        /// Login method - Works for all user types (Member, Premium, Counselor, Staff, Admin)
        /// This method handles authentication for all types of users
        /// </summary>
        public async Task<AuthResult> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(loginDto.Email);
                if (user == null)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Account not found. Please check your email or register for a new account."
                    };
                }

                if (!user.IsActive)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Account is deactivated"
                    };
                }

                if (!user.EmailConfirmed)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Please confirm your email before logging in"
                    };
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
                if (!result.Succeeded)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    };
                }

                // Generate tokens
                var jwtToken = await _tokenService.GenerateJwtTokenAsync(user);
                var refreshToken = await _tokenService.GenerateRefreshTokenAsync();

                // Save refresh token
                var refreshTokenEntity = new RefreshToken
                {
                    UserId = user.Id,
                    Token = refreshToken,
                    JwtId = Guid.NewGuid().ToString(),
                    ExpiryDate = DateTime.UtcNow.AddDays(30)
                };

                await _refreshTokenRepository.CreateAsync(refreshTokenEntity);

                // Get user roles for logging and response
                var userRoles = await _userManager.GetRolesAsync(user);
                _logger.LogInformation("User logged in successfully: {Email} with roles: {Roles}", loginDto.Email, string.Join(", ", userRoles));
                var counselorProfile = await _counselorRepository.GetCounselorByUserIdAsync(user.Id);
                return new AuthResult
                {
                    Success = true,
                    Message = "Login successful",
                    Token = jwtToken,
                    RefreshToken = refreshToken,
                    UserId = user.Id,
                    CounselorId = counselorProfile?.Id,
                    UserRoles = userRoles.ToList(),
                    ExpiresAt = DateTime.UtcNow.AddHours(Convert.ToDouble(_configuration["Jwt:ExpiryInHours"]))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for email: {Email}", loginDto.Email);
                return new AuthResult
                {
                    Success = false,
                    Message = "Login failed. Please try again."
                };
            }
        }

        /// <summary>
        /// Upgrade Member to Premium - This should be called after successful payment
        /// This method handles subscription upgrades from Member to Premium
        /// </summary>
        public async Task<AuthResult> UpgradeToPremiumAsync(Guid userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                var userRoles = await _userManager.GetRolesAsync(user);

                // Remove Member role if exists
                if (userRoles.Contains(Roles.Member))
                {
                    await _userManager.RemoveFromRoleAsync(user, Roles.Member);
                }

                // Add Premium role
                await _userManager.AddToRoleAsync(user, Roles.Premium);

                _logger.LogInformation("User upgraded to Premium: {UserId}", userId);

                return new AuthResult
                {
                    Success = true,
                    Message = "Successfully upgraded to Premium subscription",
                    UserId = userId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Premium upgrade failed for user: {UserId}", userId);
                return new AuthResult
                {
                    Success = false,
                    Message = "Premium upgrade failed. Please try again."
                };
            }
        }

        /// <summary>
        /// Downgrade Premium to Member - This should be called when subscription expires or is cancelled
        /// This method handles subscription downgrades from Premium to Member
        /// </summary>
        public async Task<AuthResult> DowngradeToMemberAsync(Guid userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                var userRoles = await _userManager.GetRolesAsync(user);

                // Remove Premium role if exists
                if (userRoles.Contains(Roles.Premium))
                {
                    await _userManager.RemoveFromRoleAsync(user, Roles.Premium);
                }

                // Add Member role if not exists
                if (!userRoles.Contains(Roles.Member))
                {
                    await _userManager.AddToRoleAsync(user, Roles.Member);
                }

                _logger.LogInformation("User downgraded to Member: {UserId}", userId);

                return new AuthResult
                {
                    Success = true,
                    Message = "Successfully downgraded to Member",
                    UserId = userId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Member downgrade failed for user: {UserId}", userId);
                return new AuthResult
                {
                    Success = false,
                    Message = "Downgrade failed. Please try again."
                };
            }
        }

        /// <summary>
        /// Refresh token method - Works for all authenticated users
        /// This method handles token refresh for maintaining user sessions
        /// </summary>
        public async Task<AuthResult> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
        {
            try
            {
                var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshTokenDto.RefreshToken);

                if (storedToken == null || storedToken.IsUsed || storedToken.IsRevoked || storedToken.ExpiryDate < DateTime.UtcNow)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Invalid refresh token"
                    };
                }

                // Mark token as used
                storedToken.IsUsed = true;
                await _refreshTokenRepository.UpdateAsync(storedToken);

                // Get user
                var user = await _userManager.FindByIdAsync(storedToken.UserId.ToString());
                if (user == null)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                // Generate new tokens
                var jwtToken = await _tokenService.GenerateJwtTokenAsync(user);
                var newRefreshToken = await _tokenService.GenerateRefreshTokenAsync();

                // Save new refresh token
                var newRefreshTokenEntity = new RefreshToken
                {
                    UserId = storedToken.UserId,
                    Token = newRefreshToken,
                    JwtId = Guid.NewGuid().ToString(),
                    ExpiryDate = DateTime.UtcNow.AddDays(30)
                };

                await _refreshTokenRepository.CreateAsync(newRefreshTokenEntity);

                // Get user roles
                var userRoles = await _userManager.GetRolesAsync(user);

                return new AuthResult
                {
                    Success = true,
                    Message = "Token refreshed successfully",
                    Token = jwtToken,
                    RefreshToken = newRefreshToken,
                    UserId = storedToken.UserId,
                    UserRoles = userRoles.ToList(),
                    ExpiresAt = DateTime.UtcNow.AddHours(Convert.ToDouble(_configuration["Jwt:ExpiryInHours"]))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token refresh failed");
                return new AuthResult
                {
                    Success = false,
                    Message = "Token refresh failed. Please login again."
                };
            }
        }

        /// <summary>
        /// Check if user has specific permission - Used for authorization
        /// This method is used by authorization middleware to check user permissions
        /// </summary>
        public async Task<bool> HasPermissionAsync(Guid userId, string permission)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || !user.IsActive) return false;

                var roles = await _userManager.GetRolesAsync(user);

                var claims = new List<Claim>();
                foreach (var role in roles)
                {
                    var roleEntity = await _roleManager.FindByNameAsync(role);
                    if (roleEntity != null)
                    {
                        var roleClaims = await _roleManager.GetClaimsAsync(roleEntity);
                        claims.AddRange(roleClaims);
                    }
                }

                return claims.Any(c => c.Type == "permission" && c.Value == permission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Permission check failed for user: {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Google OAuth login - Enhanced version with proper user info extraction
        /// This method handles authentication via Google OAuth provider
        /// </summary>
        public async Task<AuthResult> GoogleLoginAsync(string googleAccessToken)
        {
            try
            {
                // Verify Google token and get user info
                var userInfo = await GetGoogleUserInfoAsync(googleAccessToken);
                if (userInfo == null)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Invalid Google token"
                    };
                }

                var email = userInfo.Email;
                // Fixed: Use proper Google API response fields with fallbacks
                var firstName = userInfo.FirstName; // Uses the helper property
                var lastName = userInfo.LastName;   // Uses the helper property
                var picture = userInfo.Picture;

                if (string.IsNullOrEmpty(email))
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Email is required from Google"
                    };
                }

                // Check if user exists
                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    // User exists, check if Google login is already linked
                    var logins = await _userManager.GetLoginsAsync(existingUser);
                    var googleLogin = logins.FirstOrDefault(l => l.LoginProvider == "Google");

                    if (googleLogin == null)
                    {
                        // Link Google account to existing user
                        var loginInfoExistingUser = new UserLoginInfo("Google", userInfo.Id, "Google");
                        var addLoginResult = await _userManager.AddLoginAsync(existingUser, loginInfoExistingUser);
                        if (!addLoginResult.Succeeded)
                        {
                            return new AuthResult
                            {
                                Success = false,
                                Message = "Failed to link Google account"
                            };
                        }
                    }

                    // Update user info if needed
                    var needsUpdate = false;
                    if (string.IsNullOrEmpty(existingUser.FirstName) && !string.IsNullOrEmpty(firstName))
                    {
                        existingUser.FirstName = firstName;
                        needsUpdate = true;
                    }
                    if (string.IsNullOrEmpty(existingUser.LastName) && !string.IsNullOrEmpty(lastName))
                    {
                        existingUser.LastName = lastName;
                        needsUpdate = true;
                    }
                    if (string.IsNullOrEmpty(existingUser.ProfilePicture) && !string.IsNullOrEmpty(picture))
                    {
                        existingUser.ProfilePicture = picture;
                        needsUpdate = true;
                    }

                    if (needsUpdate)
                    {
                        existingUser.UpdatedAt = DateTime.UtcNow;
                        await _userManager.UpdateAsync(existingUser);
                    }

                    return await GenerateAuthResult(existingUser);
                }

                // Create new user
                var newUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    ProfilePicture = picture,
                    EmailConfirmed = true, // Google emails are pre-verified
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createResult = await _userManager.CreateAsync(newUser);
                if (!createResult.Succeeded)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = string.Join("; ", createResult.Errors.Select(e => e.Description))
                    };
                }

                // Assign Member role by default for Google logins
                await _userManager.AddToRoleAsync(newUser, Roles.Member);

                // Add Google login info
                var loginInfo = new UserLoginInfo("Google", userInfo.Id, "Google");
                await _userManager.AddLoginAsync(newUser, loginInfo);

                _logger.LogInformation("New user created via Google OAuth: {Email}", email);

                // Send welcome email
                try
                {
                    await _emailService.SendWelcomeEmailAsync(newUser.Email, newUser.FirstName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send welcome email to {Email}", newUser.Email);
                }

                return await GenerateAuthResult(newUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Google login failed");
                return new AuthResult
                {
                    Success = false,
                    Message = "Google login failed"
                };
            }
        }

        /// <summary>
        /// Get Google user information from access token
        /// </summary>
        private async Task<GoogleUserInfo> GetGoogleUserInfoAsync(string accessToken)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await httpClient.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get Google user info. Status: {StatusCode}", response.StatusCode);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Google API response: {Json}", json); // Log raw JSON response

                var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(json, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                    PropertyNameCaseInsensitive = true
                });

                if (userInfo != null)
                {
                    _logger.LogInformation("Deserialized GoogleUserInfo: Id={Id}, Email={Email}, Name={Name}, GivenName={GivenName}, FamilyName={FamilyName}, FirstName={FirstName}, LastName={LastName}",
                        userInfo.Id, userInfo.Email, userInfo.Name, userInfo.Given_Name, userInfo.Family_Name, userInfo.FirstName, userInfo.LastName);
                }
                else
                {
                    _logger.LogWarning("Failed to deserialize GoogleUserInfo");
                }

                return userInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Google user info");
                return null;
            }
        }

        /// <summary>
        /// External login method - Updated to work with the new flow
        /// This method handles authentication via external providers (kept for compatibility)
        /// </summary>
        public async Task<AuthResult> ExternalLoginAsync(ExternalLoginDto externalLoginDto)
        {
            try
            {
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return new AuthResult { Success = false, Message = "External login information not found" };
                }

                var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);

                if (result.Succeeded)
                {
                    var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                    return await GenerateAuthResult(user);
                }

                // Create new user if external login is successful but user doesn't exist
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
                var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname);
                var picture = info.Principal.FindFirstValue("picture");

                if (string.IsNullOrEmpty(email))
                {
                    return new AuthResult { Success = false, Message = "Email is required from external provider" };
                }

                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    var addLoginResult = await _userManager.AddLoginAsync(existingUser, info);
                    if (addLoginResult.Succeeded)
                    {
                        // Update user info if needed
                        var needsUpdate = false;
                        if (string.IsNullOrEmpty(existingUser.FirstName) && !string.IsNullOrEmpty(firstName))
                        {
                            existingUser.FirstName = firstName;
                            needsUpdate = true;
                        }
                        if (string.IsNullOrEmpty(existingUser.LastName) && !string.IsNullOrEmpty(lastName))
                        {
                            existingUser.LastName = lastName;
                            needsUpdate = true;
                        }
                        if (string.IsNullOrEmpty(existingUser.ProfilePicture) && !string.IsNullOrEmpty(picture))
                        {
                            existingUser.ProfilePicture = picture;
                            needsUpdate = true;
                        }

                        if (needsUpdate)
                        {
                            existingUser.UpdatedAt = DateTime.UtcNow;
                            await _userManager.UpdateAsync(existingUser);
                        }

                        return await GenerateAuthResult(existingUser);
                    }
                    return new AuthResult { Success = false, Message = "Failed to link external account" };
                }

                var newUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName ?? "Unknown",
                    LastName = lastName ?? "User",
                    ProfilePicture = picture,
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createResult = await _userManager.CreateAsync(newUser);
                if (createResult.Succeeded)
                {
                    // Assign Member role by default for external logins
                    await _userManager.AddToRoleAsync(newUser, Roles.Member);
                    await _userManager.AddLoginAsync(newUser, info);

                    // Send welcome email
                    try
                    {
                        await _emailService.SendWelcomeEmailAsync(newUser.Email, newUser.FirstName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to send welcome email to {Email}", newUser.Email);
                    }

                    return await GenerateAuthResult(newUser);
                }

                return new AuthResult { Success = false, Message = string.Join("; ", createResult.Errors.Select(e => e.Description)) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "External login failed");
                return new AuthResult { Success = false, Message = "External login failed" };
            }
        }

        /// <summary>
        /// Logout method - Works for all authenticated users
        /// This method handles user logout and token revocation
        /// </summary>
        public async Task<bool> LogoutAsync(Guid userId)
        {
            try
            {
                await _tokenService.RevokeAllUserTokensAsync(userId);
                await _signInManager.SignOutAsync();
                _logger.LogInformation("User logged out successfully: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logout failed for user: {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Change password method - Works for all authenticated users
        /// This method handles password changes for authenticated users
        /// </summary>
        public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null) return false;

                var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);

                if (result.Succeeded)
                {
                    // Revoke all tokens to force re-login
                    await _tokenService.RevokeAllUserTokensAsync(userId);
                    _logger.LogInformation("Password changed successfully for user: {UserId}", userId);
                }

                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Password change failed for user: {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Forgot password method - Sends password reset email
        /// This method handles password reset requests
        /// </summary>
        public async Task<AuthResult> ForgotPasswordAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    // Don't reveal that the user doesn't exist
                    return new AuthResult { Success = true, Message = "If an account with that email exists, a password reset email has been sent." };
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _emailService.SendPasswordResetEmailAsync(user.Email, user.FirstName, token);

                _logger.LogInformation("Password reset email sent for: {Email}", email);

                return new AuthResult { Success = true, Message = "If an account with that email exists, a password reset email has been sent." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Forgot password failed for email: {Email}", email);
                return new AuthResult { Success = false, Message = "Failed to process password reset request" };
            }
        }

        /// <summary>
        /// Reset password method - Resets password using token
        /// This method handles password reset using the token sent via email
        /// </summary>
        public async Task<AuthResult> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
                if (user == null)
                {
                    return new AuthResult { Success = false, Message = "Invalid reset request" };
                }

                var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);

                if (result.Succeeded)
                {
                    // Revoke all tokens
                    await _tokenService.RevokeAllUserTokensAsync(user.Id);
                    _logger.LogInformation("Password reset successfully for: {Email}", resetPasswordDto.Email);
                    return new AuthResult { Success = true, Message = "Password has been reset successfully" };
                }

                return new AuthResult { Success = false, Message = string.Join("; ", result.Errors.Select(e => e.Description)) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Password reset failed for email: {Email}", resetPasswordDto.Email);
                return new AuthResult { Success = false, Message = "Password reset failed" };
            }
        }

        /// <summary>
        /// Confirm email method - Confirms user email using token with better error handling
        /// This method handles email confirmation for new registrations
        /// </summary>
        public async Task<bool> ConfirmEmailAsync(Guid userId, string token)
        {
            try
            {
                _logger.LogInformation("Attempting email confirmation for user: {UserId}", userId);

                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    _logger.LogWarning("Email confirmation failed: User not found for userId: {UserId}", userId);
                    return false;
                }

                // Check if email is already confirmed
                if (user.EmailConfirmed)
                {
                    _logger.LogInformation("Email already confirmed for user: {UserId}", userId);
                    return true; // Consider already confirmed as success
                }

                // Log token information for debugging (be careful not to log sensitive data in production)
                _logger.LogInformation("Confirming email for user: {UserId}, Email: {Email}", userId, user.Email);

                var result = await _userManager.ConfirmEmailAsync(user, token);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Email confirmed successfully for user: {UserId}", userId);

                    // Optionally send welcome email after successful confirmation
                    try
                    {
                        await _emailService.SendWelcomeEmailAsync(user.Email, user.FirstName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to send welcome email to {Email}, but email confirmation was successful", user.Email);
                        // Don't fail the confirmation if welcome email fails
                    }

                    return true;
                }
                else
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("Email confirmation failed for user: {UserId}. Errors: {Errors}", userId, errors);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email confirmation failed with exception for user: {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Resend email confirmation - Resends confirmation email
        /// This method resends email confirmation for unconfirmed accounts
        /// </summary>
        public async Task<AuthResult> ResendEmailConfirmationAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null || user.EmailConfirmed)
                {
                    return new AuthResult { Success = true, Message = "If an unconfirmed account with that email exists, a confirmation email has been sent." };
                }

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                await _emailService.SendEmailConfirmationAsync(user.Email, user.FirstName, token, user.Id);

                _logger.LogInformation("Email confirmation resent for: {Email}", email);

                return new AuthResult { Success = true, Message = "If an unconfirmed account with that email exists, a confirmation email has been sent." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Resend email confirmation failed for email: {Email}", email);
                return new AuthResult { Success = false, Message = "Failed to resend confirmation email" };
            }
        }

        /// <summary>
        /// Get user roles - Returns the roles assigned to a user
        /// This method is used to get user roles for authorization purposes
        /// </summary>
        public async Task<List<string>> GetUserRolesAsync(Guid userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null) return new List<string>();

                var roles = await _userManager.GetRolesAsync(user);
                return roles.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user roles for: {UserId}", userId);
                return new List<string>();
            }
        }

        /// <summary>
        /// Deactivate user account - Admin/Staff functionality
        /// This method allows admins/staff to deactivate user accounts
        /// </summary>
        public async Task<bool> DeactivateUserAsync(Guid userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null) return false;

                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    await _tokenService.RevokeAllUserTokensAsync(userId);
                    _logger.LogInformation("User deactivated: {UserId}", userId);
                }

                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deactivate user: {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Activate user account - Admin/Staff functionality
        /// This method allows admins/staff to reactivate user accounts
        /// </summary>
        public async Task<bool> ActivateUserAsync(Guid userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null) return false;

                user.IsActive = true;
                user.UpdatedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User activated: {UserId}", userId);
                }

                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to activate user: {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Helper method to generate authentication result with tokens
        /// This is a private helper method used by external login and other methods
        /// </summary>
        private async Task<AuthResult> GenerateAuthResult(ApplicationUser user)
        {
            var jwtToken = await _tokenService.GenerateJwtTokenAsync(user);
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync();

            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                JwtId = Guid.NewGuid().ToString(),
                ExpiryDate = DateTime.UtcNow.AddDays(30)
            };

            await _refreshTokenRepository.CreateAsync(refreshTokenEntity);

            var userRoles = await _userManager.GetRolesAsync(user);

            return new AuthResult
            {
                Success = true,
                Message = "Login successful",
                Token = jwtToken,
                RefreshToken = refreshToken,
                UserId = user.Id,
                UserRoles = userRoles.ToList(),
                ExpiresAt = DateTime.UtcNow.AddHours(Convert.ToDouble(_configuration["Jwt:ExpiryInHours"]))
            };
        }
    }
}
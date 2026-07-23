using EXE201.HeartToHeart.BLL.Services;
using EXE201.HeartToHeart.DAL.DBContext;
using EXE201.HeartToHeart.DAL.Entities.Application;
using EXE201.HeartToHeart.DAL.IRepositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Repostories
{
    public class GoogleCalendarTokenRepository : IGoogleCalendarTokenRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GoogleCalendarTokenRepository> _logger;

        public GoogleCalendarTokenRepository(ApplicationDbContext context, ILogger<GoogleCalendarTokenRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<GoogleCalendarToken?> GetTokenAsync(Guid userId)
        {
            try
            {
                var token = await _context.GoogleCalendarTokens
                    .FirstOrDefaultAsync(t => t.UserId == userId && !t.IsRevoked);

                if (token != null)
                {
                    _logger.LogDebug("Retrieved token for user {UserId}, expires at {ExpiresAt}", userId, token.ExpiresAt);
                }

                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving token for user {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> SaveTokenAsync(Guid userId, string accessToken, string? refreshToken, long? expiresInSeconds)
        {
            try
            {
                // ENHANCED: Better validation and error handling
                if (string.IsNullOrEmpty(accessToken))
                {
                    _logger.LogWarning("Cannot save empty access token for user {UserId}", userId);
                    return false;
                }

                var existingToken = await GetTokenAsync(userId);
                if (existingToken != null)
                {
                    return await UpdateTokenAsync(userId, accessToken, refreshToken, expiresInSeconds);
                }

                // ENHANCED: Better expiry calculation
                var validExpiresInSeconds = expiresInSeconds ?? 3600; // Default to 1 hour
                if (validExpiresInSeconds <= 0)
                {
                    validExpiresInSeconds = 3600; // Reset to default if invalid
                    _logger.LogWarning("Invalid expiry seconds for user {UserId}, using default 3600", userId);
                }

                var token = new GoogleCalendarToken
                {
                    Id = Guid.NewGuid(), // Ensure ID is set
                    UserId = userId,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddSeconds(validExpiresInSeconds),
                    Scope = "https://www.googleapis.com/auth/calendar",
                    IsRevoked = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.GoogleCalendarTokens.Add(token);
                var result = await _context.SaveChangesAsync() > 0;

                if (result)
                {
                    _logger.LogInformation("Successfully saved new token for user {UserId}, expires at {ExpiresAt}",
                        userId, token.ExpiresAt);
                }
                else
                {
                    _logger.LogError("Failed to save new token for user {UserId}", userId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving token for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> UpdateTokenAsync(Guid userId, string accessToken, string? refreshToken, long? expiresInSeconds)
        {
            try
            {
                var token = await GetTokenAsync(userId);
                if (token == null)
                {
                    _logger.LogWarning("No existing token found to update for user {UserId}", userId);
                    return false;
                }

                // ENHANCED: Better validation
                if (string.IsNullOrEmpty(accessToken))
                {
                    _logger.LogWarning("Cannot update with empty access token for user {UserId}", userId);
                    return false;
                }

                var validExpiresInSeconds = expiresInSeconds ?? 3600; // Default to 1 hour
                if (validExpiresInSeconds <= 0)
                {
                    validExpiresInSeconds = 3600; // Reset to default if invalid
                    _logger.LogWarning("Invalid expiry seconds for user {UserId}, using default 3600", userId);
                }

                var oldExpiresAt = token.ExpiresAt;

                token.AccessToken = accessToken;
                if (!string.IsNullOrEmpty(refreshToken))
                    token.RefreshToken = refreshToken;
                token.ExpiresAt = DateTime.UtcNow.AddSeconds(validExpiresInSeconds);
                token.UpdatedAt = DateTime.UtcNow;
                token.IsRevoked = false; // Ensure token is not revoked when updating

                _context.GoogleCalendarTokens.Update(token);
                var result = await _context.SaveChangesAsync() > 0;

                if (result)
                {
                    _logger.LogInformation("Successfully updated token for user {UserId}, expires at {ExpiresAt} (was {OldExpiresAt})",
                        userId, token.ExpiresAt, oldExpiresAt);
                }
                else
                {
                    _logger.LogError("Failed to update token for user {UserId}", userId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating token for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> RevokeTokenAsync(Guid userId)
        {
            try
            {
                var token = await GetTokenAsync(userId);
                if (token == null)
                {
                    _logger.LogInformation("No token found to revoke for user {UserId}", userId);
                    return true; // Consider it successful if there's nothing to revoke
                }

                token.IsRevoked = true;
                token.UpdatedAt = DateTime.UtcNow;

                _context.GoogleCalendarTokens.Update(token);
                var result = await _context.SaveChangesAsync() > 0;

                if (result)
                {
                    _logger.LogInformation("Successfully revoked token for user {UserId}", userId);
                }
                else
                {
                    _logger.LogError("Failed to revoke token for user {UserId}", userId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking token for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> HasValidTokenAsync(Guid userId)
        {
            try
            {
                var token = await GetTokenAsync(userId);
                if (token == null || token.IsRevoked)
                {
                    _logger.LogDebug("No valid token found for user {UserId}", userId);
                    return false;
                }

                // Use the extension method for better consistency
                var isValid = !token.IsExpiredWithBuffer(5); // 5 minute buffer

                if (!isValid)
                {
                    _logger.LogDebug("Token expired for user {UserId}, expires at {ExpiresAt}", userId, token.ExpiresAt);
                }

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking token validity for user {UserId}", userId);
                return false;
            }
        }

        // ENHANCED: Additional helper methods
        public async Task<DateTime?> GetTokenExpiryAsync(Guid userId)
        {
            try
            {
                var token = await GetTokenAsync(userId);
                if (token?.ExpiresAt != null)
                {
                    _logger.LogDebug("Token for user {UserId} expires at {ExpiresAt}", userId, token.ExpiresAt);
                }
                return token?.ExpiresAt;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting token expiry for user {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> IsTokenExpiredAsync(Guid userId)
        {
            try
            {
                var token = await GetTokenAsync(userId);
                if (token == null || token.IsRevoked)
                {
                    _logger.LogDebug("No token or revoked token for user {UserId}", userId);
                    return true;
                }

                var isExpired = token.IsExpiredWithBuffer(0);
                if (isExpired)
                {
                    _logger.LogDebug("Token expired for user {UserId}, expired at {ExpiresAt}", userId, token.ExpiresAt);
                }

                return isExpired;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if token is expired for user {UserId}", userId);
                return true; // Assume expired if there's an error
            }
        }

        // ENHANCED: Clean up expired tokens (can be called periodically)
        public async Task<int> CleanupExpiredTokensAsync()
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-30);
                _logger.LogInformation("Starting cleanup of tokens expired before {CutoffDate}", cutoffDate);

                var expiredTokens = await _context.GoogleCalendarTokens
                    .Where(t => t.ExpiresAt < cutoffDate)
                    .ToListAsync();

                if (expiredTokens.Any())
                {
                    _logger.LogInformation("Found {Count} expired tokens to clean up", expiredTokens.Count);

                    _context.GoogleCalendarTokens.RemoveRange(expiredTokens);
                    var deletedCount = await _context.SaveChangesAsync();

                    _logger.LogInformation("Successfully cleaned up {Count} expired tokens", deletedCount);
                    return deletedCount;
                }
                else
                {
                    _logger.LogInformation("No expired tokens found to clean up");
                }

                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired tokens");
                return -1;
            }
        }
    }
}
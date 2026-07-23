using EXE201.HeartToHeart.DAL.Entities.Application;
using EXE201.HeartToHeart.DAL.IRepositories;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.BLL.Services
{
    public class GoogleCalendarTokenStore : IDataStore
    {
        private readonly IGoogleCalendarTokenRepository _tokenRepository;
        private readonly ILogger<GoogleCalendarTokenStore> _logger;

        public GoogleCalendarTokenStore(IGoogleCalendarTokenRepository tokenRepository, ILogger<GoogleCalendarTokenStore> logger = null)
        {
            _tokenRepository = tokenRepository;
            _logger = logger;
        }

        public async Task ClearAsync()
        {
            // This would clear all tokens - implement if needed
            await Task.CompletedTask;
        }

        public async Task DeleteAsync<T>(string key)
        {
            if (Guid.TryParse(key, out var userId))
            {
                await _tokenRepository.RevokeTokenAsync(userId);
                _logger?.LogInformation("Deleted token for user {UserId}", userId);
            }
        }

        public async Task<T> GetAsync<T>(string key)
        {
            if (typeof(T) == typeof(TokenResponse) && Guid.TryParse(key, out var userId))
            {
                try
                {
                    var token = await _tokenRepository.GetTokenAsync(userId);
                    if (token != null && !token.IsRevoked)
                    {
                        // Better expiry calculation and handling
                        var expiresInSeconds = Math.Max(0, (long)(token.ExpiresAt - DateTime.UtcNow).TotalSeconds);

                        // Don't return expired tokens without buffer
                        if (expiresInSeconds <= 60) // 1 minute buffer for token operations
                        {
                            _logger?.LogDebug("Token for user {UserId} is about to expire in {ExpiresInSeconds} seconds",
                                userId, expiresInSeconds);
                            // Still return the token as the service layer will handle refresh
                        }

                        var tokenResponse = new TokenResponse
                        {
                            AccessToken = token.AccessToken,
                            RefreshToken = token.RefreshToken,
                            ExpiresInSeconds = expiresInSeconds,
                            Scope = token.Scope,
                            TokenType = "Bearer"
                        };

                        _logger?.LogDebug("Retrieved token for user {UserId}, expires in {ExpiresInSeconds} seconds",
                            userId, expiresInSeconds);

                        return (T)(object)tokenResponse;
                    }
                    else
                    {
                        _logger?.LogDebug("No valid token found for user {UserId}", userId);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error retrieving token for user {UserId}", userId);
                }
            }
            return default(T);
        }

        public async Task StoreAsync<T>(string key, T value)
        {
            if (typeof(T) == typeof(TokenResponse) && Guid.TryParse(key, out var userId))
            {
                var tokenResponse = value as TokenResponse;
                if (tokenResponse != null)
                {
                    try
                    {
                        // Better validation and error handling for expires in seconds
                        var expiresInSeconds = tokenResponse.ExpiresInSeconds ?? 3600; // Default to 1 hour if null

                        // Ensure we don't have negative or extremely large values
                        if (expiresInSeconds < 0)
                        {
                            _logger?.LogWarning("Invalid negative expiry seconds {ExpiresInSeconds} for user {UserId}, setting to 3600",
                                expiresInSeconds, userId);
                            expiresInSeconds = 3600;
                        }
                        else if (expiresInSeconds > 86400 * 365) // More than 1 year
                        {
                            _logger?.LogWarning("Extremely large expiry seconds {ExpiresInSeconds} for user {UserId}, setting to 3600",
                                expiresInSeconds, userId);
                            expiresInSeconds = 3600;
                        }

                        var success = await _tokenRepository.SaveTokenAsync(
                            userId,
                            tokenResponse.AccessToken,
                            tokenResponse.RefreshToken,
                            expiresInSeconds);

                        if (success)
                        {
                            _logger?.LogInformation("Successfully stored token for user {UserId}, expires in {ExpiresInSeconds} seconds",
                                userId, expiresInSeconds);
                        }
                        else
                        {
                            _logger?.LogError("Failed to store token for user {UserId}", userId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error storing token for user {UserId}", userId);
                    }
                }
            }
        }
    }
}
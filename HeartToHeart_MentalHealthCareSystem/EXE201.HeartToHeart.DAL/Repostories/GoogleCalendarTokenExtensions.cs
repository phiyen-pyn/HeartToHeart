using EXE201.HeartToHeart.DAL.Entities.Application;
using Google.Apis.Auth.OAuth2.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.BLL.Services
{
    public static class GoogleCalendarTokenExtensions
    {
        public static TokenResponse ToTokenResponse(this GoogleCalendarToken token)
        {
            // Safe expiry calculation with better handling
            var expiresInSeconds = Math.Max(0, (long)(token.ExpiresAt - DateTime.UtcNow).TotalSeconds);

            return new TokenResponse
            {
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken,
                ExpiresInSeconds = expiresInSeconds,
                Scope = token.Scope,
                TokenType = "Bearer"
            };
        }

        public static bool IsExpiredWithBuffer(this GoogleCalendarToken token, int bufferMinutes = 5)
        {
            return token.ExpiresAt <= DateTime.UtcNow.AddMinutes(bufferMinutes);
        }
    }
}

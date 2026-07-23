using EXE201.HeartToHeart.DAL.Entities.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.IRepositories
{
    public interface IGoogleCalendarTokenRepository
    {
        Task<GoogleCalendarToken?> GetTokenAsync(Guid userId);
        Task<bool> SaveTokenAsync(Guid userId, string accessToken, string? refreshToken, long? expiresInSeconds);
        Task<bool> UpdateTokenAsync(Guid userId, string accessToken, string? refreshToken, long? expiresInSeconds);
        Task<bool> RevokeTokenAsync(Guid userId);
        Task<bool> HasValidTokenAsync(Guid userId);
    }
}

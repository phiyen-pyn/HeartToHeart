using EXE201.HeartToHeart.DAL.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.IRepositories
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task<RefreshToken?> GetByJwtIdAsync(string jwtId);
        Task<IEnumerable<RefreshToken>> GetUserTokensAsync(Guid userId);
        Task<bool> CreateAsync(RefreshToken refreshToken);
        Task<bool> UpdateAsync(RefreshToken refreshToken);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> RevokeTokenAsync(string token);
        Task<bool> RevokeAllUserTokensAsync(Guid userId);
        Task<bool> CleanupExpiredTokensAsync();
    }
}

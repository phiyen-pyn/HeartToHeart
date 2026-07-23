using EXE201.HeartToHeart.DAL.DBContext;
using EXE201.HeartToHeart.DAL.Entities.Identity;
using EXE201.HeartToHeart.DAL.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Repostories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly ApplicationDbContext _context;

        public RefreshTokenRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task<RefreshToken?> GetByJwtIdAsync(string jwtId)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.JwtId == jwtId);
        }

        public async Task<IEnumerable<RefreshToken>> GetUserTokensAsync(Guid userId)
        {
            return await _context.RefreshTokens
                .Where(rt => rt.UserId == userId)
                .ToListAsync();
        }

        public async Task<bool> CreateAsync(RefreshToken refreshToken)
        {
            try
            {
                _context.RefreshTokens.Add(refreshToken);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(RefreshToken refreshToken)
        {
            try
            {
                _context.RefreshTokens.Update(refreshToken);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var token = await _context.RefreshTokens.FindAsync(id);
                if (token == null) return false;

                _context.RefreshTokens.Remove(token);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            try
            {
                var refreshToken = await GetByTokenAsync(token);
                if (refreshToken == null) return false;

                refreshToken.IsRevoked = true;
                refreshToken.UpdatedAt = DateTime.UtcNow;

                return await UpdateAsync(refreshToken);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RevokeAllUserTokensAsync(Guid userId)
        {
            try
            {
                var tokens = await _context.RefreshTokens
                    .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                    .ToListAsync();

                foreach (var token in tokens)
                {
                    token.IsRevoked = true;
                    token.UpdatedAt = DateTime.UtcNow;
                }

                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CleanupExpiredTokensAsync()
        {
            try
            {
                var expiredTokens = await _context.RefreshTokens
                    .Where(rt => rt.ExpiryDate < DateTime.UtcNow || rt.IsRevoked || rt.IsUsed)
                    .ToListAsync();

                _context.RefreshTokens.RemoveRange(expiredTokens);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}

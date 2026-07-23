using EXE201.HeartToHeart.DAL.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.BLL.IServices
{
    public interface ITokenService
    {
        Task<string> GenerateJwtTokenAsync(ApplicationUser user);
        Task<string> GenerateRefreshTokenAsync();
        Task<bool> ValidateJwtTokenAsync(string token);
        Task<bool> RevokeAllUserTokensAsync(Guid userId);
        string? GetUserIdFromToken(string token);
    }
}

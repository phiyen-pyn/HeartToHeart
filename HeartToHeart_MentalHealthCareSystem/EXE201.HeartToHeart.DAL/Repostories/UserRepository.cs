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
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(Guid userId)
        {
            return await _context.Users
                .Include(u => u.RefreshTokens)
                .Include(u => u.CounselorProfile)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.RefreshTokens)
                .Include(u => u.CounselorProfile)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync(int page = 1, int pageSize = 10)
        {
            return await _context.Users
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> CreateUserAsync(ApplicationUser user)
        {
            try
            {
                _context.Users.Add(user);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateUserAsync(ApplicationUser user)
        {
            try
            {
                _context.Users.Update(user);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(Guid userId)
        {
            try
            {
                var user = await GetUserByIdAsync(userId);
                if (user == null) return false;

                _context.Users.Remove(user);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UserExistsAsync(Guid userId)
        {
            return await _context.Users.AnyAsync(u => u.Id == userId);
        }

        public async Task<bool> UserExistsByEmailAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<ApplicationUser>> GetUsersByRoleAsync(string roleName)
        {
            return await _context.Users
                .Join(_context.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { User = u, UserRole = ur })
                .Join(_context.Roles, x => x.UserRole.RoleId, r => r.Id, (x, r) => new { x.User, Role = r })
                .Where(x => x.Role.Name == roleName)
                .Select(x => x.User)
                .ToListAsync();
        }

        public async Task<int> GetTotalUsersCountAsync()
        {
            return await _context.Users.CountAsync();
        }

        public async Task<int> GetTotalUsersCountByRoleAsync(string roleName)
        {
            return await _context.Users
                .Join(_context.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { User = u, UserRole = ur })
                .Join(_context.Roles, x => x.UserRole.RoleId, r => r.Id, (x, r) => new { x.User, Role = r })
                .Where(x => x.Role.Name == roleName)
                .CountAsync();
        }

        public async Task<int> GetTotalCountByMonthAsync(int year, int month)
        {
            return await _context.Users
                .Where(u => u.CreatedAt.Year == year && u.CreatedAt.Month == month)
                .CountAsync();
        }

        public async Task<int> GetTotalCountByRoleAndMonthAsync(string roleName, int year, int month)
        {
            return await _context.Users
                .Join(_context.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { User = u, UserRole = ur })
                .Join(_context.Roles, x => x.UserRole.RoleId, r => r.Id, (x, r) => new { x.User, Role = r })
                .Where(x => x.Role.Name == roleName && x.User.CreatedAt.Year == year && x.User.CreatedAt.Month == month)
                .CountAsync();
        }
    }
}

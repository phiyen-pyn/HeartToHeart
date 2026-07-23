using EXE201.HeartToHeart.DAL.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.IRepositories
{
    public interface IUserRepository
    {
        Task<ApplicationUser?> GetUserByIdAsync(Guid userId);
        Task<ApplicationUser?> GetUserByEmailAsync(string email);
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync(int page = 1, int pageSize = 10);
        Task<bool> CreateUserAsync(ApplicationUser user);
        Task<bool> UpdateUserAsync(ApplicationUser user);
        Task<bool> DeleteUserAsync(Guid userId);
        Task<bool> UserExistsAsync(Guid userId);
        Task<bool> UserExistsByEmailAsync(string email);
        Task<int> GetTotalUsersCountAsync();
        Task<int> GetTotalUsersCountByRoleAsync(string roleName);
        Task<int> GetTotalCountByMonthAsync(int year, int month);
        Task<int> GetTotalCountByRoleAndMonthAsync(string roleName, int year, int month);
        Task<IEnumerable<ApplicationUser>> GetUsersByRoleAsync(string roleName);
    }
}

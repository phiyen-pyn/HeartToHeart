using EXE201.HeartToHeart.DAL.Entities.Identity;
using EXE201.HeartToHeart.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.BLL.IServices
{
    public interface IUserService
    {
        Task<UserDto?> GetUserByIdAsync(Guid userId);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<IEnumerable<UserDto>> GetAllUsersAsync(int page = 1, int pageSize = 10);
        Task<bool> UpdateUserProfileAsync(Guid userId, UpdateUserProfileDto updateDto);
        Task<bool> DeactivateUserAsync(Guid userId);
        Task<bool> ActivateUserAsync(Guid userId);
        Task<int> GetTotalUsersCountAsync();
        Task<int> GetTotalUsersCountByRoleAsync(string roleName);
        Task<int> GetTotalCountByMonthAsync(int year, int month);
        Task<int> GetTotalCountByRoleAndMonthAsync(string roleName, int year, int month);
        Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string roleName);
    }
}

using AutoMapper;
using EXE201.HeartToHeart.BLL.IServices;
using EXE201.HeartToHeart.DAL.DBContext;
using EXE201.HeartToHeart.DAL.Entities.Identity;
using EXE201.HeartToHeart.DAL.IRepositories;
using EXE201.HeartToHeart.DAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.BLL.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository userRepository,
            UserManager<ApplicationUser> userManager,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<UserDto?> GetUserByIdAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                return user == null ? null : MapToUserDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by ID: {UserId}", userId);
                return null;
            }
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            try
            {
                var user = await _userRepository.GetUserByEmailAsync(email);
                return user == null ? null : MapToUserDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by email: {Email}", email);
                return null;
            }
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var users = await _userRepository.GetAllUsersAsync(page, pageSize);
                return users.Select(MapToUserDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users. Page: {Page}, PageSize: {PageSize}", page, pageSize);
                return Enumerable.Empty<UserDto>();
            }
        }

        public async Task<bool> UpdateUserProfileAsync(Guid userId, UpdateUserProfileDto updateDto)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for update: {UserId}", userId);
                    return false;
                }

                // Update user properties
                if (!string.IsNullOrEmpty(updateDto.FirstName))
                    user.FirstName = updateDto.FirstName;

                if (!string.IsNullOrEmpty(updateDto.LastName))
                    user.LastName = updateDto.LastName;

                if (updateDto.DateOfBirth.HasValue)
                    user.DateOfBirth = updateDto.DateOfBirth;

                if (!string.IsNullOrEmpty(updateDto.Gender))
                    user.Gender = updateDto.Gender;

                if (!string.IsNullOrEmpty(updateDto.ProfilePicture))
                    user.ProfilePicture = updateDto.ProfilePicture;

                if (!string.IsNullOrEmpty(updateDto.PhoneNumber))
                {
                    user.PhoneNumber = updateDto.PhoneNumber;
                    // Update through UserManager to handle phone number validation
                    var result = await _userManager.SetPhoneNumberAsync(user, updateDto.PhoneNumber);
                    if (!result.Succeeded)
                    {
                        _logger.LogWarning("Failed to update phone number for user: {UserId}", userId);
                        return false;
                    }
                }

                user.UpdatedAt = DateTime.UtcNow;

                return await _userRepository.UpdateUserAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> DeactivateUserAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for deactivation: {UserId}", userId);
                    return false;
                }

                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;

                return await _userRepository.UpdateUserAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> ActivateUserAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for activation: {UserId}", userId);
                    return false;
                }

                user.IsActive = true;
                user.UpdatedAt = DateTime.UtcNow;

                return await _userRepository.UpdateUserAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating user: {UserId}", userId);
                return false;
            }
        }

        public async Task<int> GetTotalUsersCountAsync()
        {
            try
            {
                return await _userRepository.GetTotalUsersCountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total users count");
                return 0;
            }
        }

        public async Task<int> GetTotalUsersCountByRoleAsync(string roleName)
        {
            try
            {
                return await _userRepository.GetTotalUsersCountByRoleAsync(roleName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total users count by role: {RoleName}", roleName);
                return 0;
            }
        }

        public async Task<int> GetTotalCountByMonthAsync(int year, int month)
        {
            try
            {
                return await _userRepository.GetTotalCountByMonthAsync(year, month);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total users count by month: {Year}-{Month}", year, month);
                return 0;
            }
        }

        public async Task<int> GetTotalCountByRoleAndMonthAsync(string roleName, int year, int month)
        {
            try
            {
                return await _userRepository.GetTotalCountByRoleAndMonthAsync(roleName, year, month);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total users count by role and month: {RoleName} {Year}-{Month}", roleName, year, month);
                return 0;
            }
        }

        public async Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string roleName)
        {
            try
            {
                var users = await _userRepository.GetUsersByRoleAsync(roleName);
                return users.Select(MapToUserDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users by role: {RoleName}", roleName);
                return Enumerable.Empty<UserDto>();
            }
        }

        public static UserDto MapToUserDto(ApplicationUser user)
        {
            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                ProfilePicture = user.ProfilePicture,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                IsActive = user.IsActive,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed
            };
        }
    }
}

using EXE201.HeartToHeart.BLL.IServices;
using EXE201.HeartToHeart.Common.Constants;
using EXE201.HeartToHeart.DAL.Entities.Application;
using EXE201.HeartToHeart.DAL.IRepositories;
using EXE201.HeartToHeart.DAL.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.BLL.Services
{
    public class AnonymousPostService : IAnonymousPostService
    {
        private readonly IAnonymousPostRepository _postRepository;
        private readonly ILogger<AnonymousPostService> _logger;

        public AnonymousPostService(IAnonymousPostRepository postRepository, ILogger<AnonymousPostService> logger)
        {
            _postRepository = postRepository;
            _logger = logger;
        }

        public async Task<bool> CreateAsync(AnonymousPostDto postDto)
        {
            try
            {
                var post = new AnonymousPost
                {
                    UserId = postDto.UserId,
                    Content = postDto.Content,
                    IsReported = postDto.IsReported,
                    CreatedAt = DateTime.UtcNow,
                };

                return await _postRepository.CreateAsync(post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post.");
                return false;
            }
        }

        public async Task<bool> UpdateAsync(Guid postId, AnonymousPostDto postDto)
        {
            try
            {
                var existing = await _postRepository.GetByIdAsync(postId);
                if (existing == null) return false;

                existing.Content = postDto.Content;
                existing.IsReported = postDto.IsReported;
                existing.UpdatedAt = DateTime.UtcNow;

                return await _postRepository.UpdateAsync(existing);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating post {PostId}", postId);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(Guid postId)
        {
            try
            {
                var post = await _postRepository.GetByIdAsync(postId);
                if (post == null)
                {
                    _logger.LogWarning("Post not found for deactivation: {PostId}", postId);
                    return false;
                }

                post.IsActive = false;
                post.UpdatedAt = DateTime.UtcNow;

                return await _postRepository.UpdateAsync(post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating post: {PostId}", postId);
                return false;
            }
        }

        public async Task<IEnumerable<AnonymousPostDto>> GetAllAsync(List<string> role, int page = 1, int pageSize = 10)
        {
            try
            {
                var posts = await _postRepository.GetAllAsync(page, pageSize);
                if (!role.Contains(Roles.Admin) && !role.Contains(Roles.Staff))
                {
                    posts = posts.Where(p => p.IsActive == true);
                }
                return posts.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all posts.");
                return Enumerable.Empty<AnonymousPostDto>();
            }
        }

        public async Task<IEnumerable<AnonymousPostDto>> GetAllByUserIdAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            try
            {
                var posts = await _postRepository.GetAllByUserIdAsync(userId, page, pageSize);
                return posts.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting posts by user ID: {UserId}", userId);
                return Enumerable.Empty<AnonymousPostDto>();
            }
        }

        public async Task<AnonymousPostDto?> GetByIdAsync(List<string> role, Guid postId)
        {
            try
            {
                var post = await _postRepository.GetByIdAsync(postId);
                if (!role.Contains(Roles.Admin) && !role.Contains(Roles.Staff))
                {
                    if (post != null && post.IsActive == false)
                    {
                        return null;
                    }
                }
                return post == null ? null : MapToDto(post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting post by ID: {PostId}", postId);
                return null;
            }
        }

        public async Task<IEnumerable<AnonymousPostDto>> GetReportedPostsAsync(List<string> role, int page = 1, int pageSize = 10)
        {
            try
            {
                var posts = await _postRepository.GetReportedPostsAsync(page, pageSize);
                if (!role.Contains(Roles.Admin) && !role.Contains(Roles.Staff))
                {
                    posts = posts.Where(p => p.IsActive == true);
                }
                return posts.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reported posts.");
                return Enumerable.Empty<AnonymousPostDto>();
            }
        }

        public async Task<IEnumerable<AnonymousPostDto>> GetReportedPostsByUserIdAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            try
            {
                var posts = await _postRepository.GetReportedPostsByUserIdAsync(userId, page, pageSize);
                return posts.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reported posts.");
                return Enumerable.Empty<AnonymousPostDto>();
            }
        }

        public async Task<int> GetTotalCountAsync()
        {
            try
            {
                return await _postRepository.GetTotalCountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total post count.");
                return 0;
            }
        }

        public async Task<int> GetTotalByUserIdCountAsync(Guid userId)
        {
            try
            {
                return await _postRepository.GetTotalByUserIdCountAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total post by user id count.");
                return 0;
            }
        }

        public async Task<int> GetTotalReportedCountAsync()
        {
            try
            {
                return await _postRepository.GetTotalReportedCountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total reported post count.");
                return 0;
            }
        }

        public async Task<int> GetTotalReportedByUserIdCountAsync(Guid userId)
        {
            try
            {
                return await _postRepository.GetTotalReportedByUserIdCountAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total reported post by user id count.");
                return 0;
            }
        }

        public async Task<int> GetTotalCountByMonthAsync(int year, int month)
        {
            try
            {
                return await _postRepository.GetTotalCountByMonthAsync(year, month);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total post count by month.");
                return 0;
            }
        }

        public async Task<int> GetTotalReportedCountByMonthAsync(int year, int month)
        {
            try
            {
                return await _postRepository.GetTotalReportedCountByMonthAsync(year, month);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total reported post count by month.");
                return 0;
            }
        }

        public static AnonymousPostDto MapToDto(AnonymousPost post)
        {
            return new AnonymousPostDto
            {
                Id = post.Id,
                UserId = post.UserId,
                Content = post.Content,
                IsReported = post.IsReported,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt
            };
        }
    }
}

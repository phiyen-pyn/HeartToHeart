using Azure;
using EXE201.HeartToHeart.BLL.IServices;
using EXE201.HeartToHeart.Common.Constants;
using EXE201.HeartToHeart.DAL.Entities.Application;
using EXE201.HeartToHeart.DAL.IRepositories;
using EXE201.HeartToHeart.DAL.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EXE201.HeartToHeart.BLL.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly ILogger<CommentService> _logger;

        public CommentService(ICommentRepository commentRepository, ILogger<CommentService> logger)
        {
            _commentRepository = commentRepository;
            _logger = logger;
        }

        public async Task<bool> CreateAsync(CommentDto dto)
        {
            try
            {
                var comment = new Comment
                {
                    PostId = dto.PostId,
                    UserId = dto.UserId,
                    Content = dto.Content,
                    CreatedAt = DateTime.UtcNow,
                };

                return await _commentRepository.CreateAsync(comment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post.");
                return false;
            }
        }

        public async Task<bool> UpdateAsync(Guid commentId, CommentDto dto)
        {
            try
            {
                var existing = await _commentRepository.GetByIdAsync(commentId);
                if (existing == null) return false;

                existing.Content = dto.Content;
                existing.UpdatedAt = DateTime.UtcNow;

                return await _commentRepository.UpdateAsync(existing);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating comment {CommentId}", commentId);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(Guid commentId)
        {
            try
            {
                var comment = await _commentRepository.GetByIdAsync(commentId);
                if (comment == null)
                {
                    _logger.LogWarning("Comment not found for deactivation: {CommentId}", commentId);
                    return false;
                }

                comment.IsActive = false;
                comment.UpdatedAt = DateTime.UtcNow;

                return await _commentRepository.UpdateAsync(comment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating comment: {CommentId}", commentId);
                return false;
            }
        }

        public async Task<IEnumerable<CommentDto>> GetAllByPostIdAsync(List<string> role, Guid postId)
        {
            try
            {
                var comments = await _commentRepository.GetAllByPostIdAsync(postId);
                if (!role.Contains(Roles.Admin) && !role.Contains(Roles.Staff))
                {
                    comments = comments.Where(p => p.IsActive == true);
                }
                return comments.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all comments by postId.");
                return Enumerable.Empty<CommentDto>();
            }
        }

        public async Task<CommentDto?> GetByIdAsync(List<string> role, Guid commentId)
        {
            try
            {
                var comment = await _commentRepository.GetByIdAsync(commentId);
                if (!role.Contains(Roles.Admin) && !role.Contains(Roles.Staff))
                {
                    if (comment != null && comment.IsActive == false)
                    {
                        return null;
                    }
                }
                return comment == null ? null : MapToDto(comment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comment by ID: {CommentId}", commentId);
                return null;
            }
        }

        public static CommentDto MapToDto(Comment comment)
        {
            return new CommentDto
            {
                Id = comment.Id,
                PostId = comment.PostId,
                UserId = comment.UserId,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
            };
        }
    }
}

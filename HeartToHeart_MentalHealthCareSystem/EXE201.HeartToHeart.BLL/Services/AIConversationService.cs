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
    public class AIConversationService : IAIConversationService
    {
        private readonly IAIConversationRepository _repository;
        private readonly ILogger<AIConversationService> _logger;

        public AIConversationService(IAIConversationRepository repository, ILogger<AIConversationService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<AIConversationDto> AddAsync(AIConversationDto dto)
        {
            try
            {
                var conversation = new AIConversation
                {
                    UserId = dto.UserId,
                    UserMessage = dto.UserMessage,
                    AIResponse = dto.AIResponse,
                    SentAt = DateTime.UtcNow
                };
                var convo = await _repository.AddAsync(conversation);
                return MapToDto(convo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating AI Conversation.");
                return null;
            }
        }

        public async Task<IEnumerable<AIConversationDto>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            try
            {
                var convos = await _repository.GetByUserIdAsync(userId, page, pageSize);
                return convos.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AI conversations by user ID: {UserId}", userId);
                return Enumerable.Empty<AIConversationDto>();
            }
        }

        public async Task<AIConversationDto?> GetByIdAsync(Guid id)
        {
            try
            {
                var convo = await _repository.GetByIdAsync(id);
                if (convo != null && convo.IsActive == false)
                {
                    return null;
                }
                return convo == null ? null : MapToDto(convo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AI conversation by ID: {AIConversationId}", id);
                return null;
            }
        }

        public async Task<AIConversationDto> UpdateAsync(AIConversationDto dto)
        {
            try
            {
                var existing = await _repository.GetByIdAsync(dto.Id);
                existing.UserMessage = dto.UserMessage;
                existing.AIResponse = dto.AIResponse;
                existing.SentAt = DateTime.UtcNow;
                await _repository.UpdateAsync(existing);
                return MapToDto(existing);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating AI Conversation {AIConversationId}", dto.Id);
                return null;
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var existing = await _repository.GetByIdAsync(id);
                if (existing == null)
                {
                    _logger.LogWarning("AI Conversation not found for deactivation: {AIConversationId}", id);
                    return false;
                }
                existing.IsActive = false;
                existing.UpdatedAt = DateTime.UtcNow;
                var result = await _repository.UpdateAsync(existing);
                if (result != null)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating AI Conversation {AIConversationId}", id);
                return false;
            }
        }

        public static AIConversationDto MapToDto(AIConversation convo)
        {
            return new AIConversationDto
            {
                Id = convo.Id,
                UserId = convo.UserId,
                UserMessage = convo.UserMessage,
                AIResponse = convo.AIResponse,
                SentAt = convo.SentAt
            };
        }
    }
}

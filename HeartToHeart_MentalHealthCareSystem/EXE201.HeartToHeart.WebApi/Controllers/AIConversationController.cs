using EXE201.HeartToHeart.BLL.IServices;
using EXE201.HeartToHeart.BLL.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Security.Claims;
using System.Numerics;
using EXE201.HeartToHeart.DAL.Entities.Application;
using EXE201.HeartToHeart.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;
using EXE201.HeartToHeart.Common.Constants;

namespace EXE201.HeartToHeart.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AIConversationController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IAIConversationService _aIConversationService;
        private readonly IDiaryEntryService _diaryEntryService;
        private readonly IEmotionTrackService _emotionTrackService;

        public AIConversationController(IAIConversationService aIConversationService, IDiaryEntryService diaryEntryService, IEmotionTrackService emotionTrackService, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _aIConversationService = aIConversationService;
            _diaryEntryService = diaryEntryService;
            _emotionTrackService = emotionTrackService;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetByUserIdAsync()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var convos = await _aIConversationService.GetByUserIdAsync(userId);
            return Ok(convos);
        }

        [HttpGet("{aIConvoId}")]
        public async Task<IActionResult> GetByIdAsync(Guid aIConvoId)
        {
            var convo = await _aIConversationService.GetByIdAsync(aIConvoId);
            if (convo == null)
                return NotFound(new { Message = "AI Conversation not found" });
            return Ok(convo);
        }


        [HttpPost]
        public async Task<IActionResult> AddAsync([FromBody] string message)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var roles = User.Identity?.IsAuthenticated == true
                ? User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
                : [Roles.Guest];
            if (roles.Contains(Roles.Member))
            {
                var response = new AIConversationDto
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    UserMessage = message,
                    AIResponse = "Hãy mua gói đăng ký Premium để sử dụng chức năng này",
                    SentAt = DateTime.UtcNow
                };
                return Ok(response);
            }
            var reply = await GenerateAIResponse(userId, message);

            if (reply == null) return StatusCode(500, "Failed to get response from AI");

            var dto = new AIConversationDto
            {
                UserId = userId,
                UserMessage = message,
                AIResponse = SanitizeReply(reply),
                SentAt = DateTime.UtcNow
            };

            var result = await _aIConversationService.AddAsync(dto);
            return Ok(result);
        }

        [HttpPost("sentiment-analysis")]
        public async Task<IActionResult> SentimentAnalysisAsync()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            const string promptMessage = "Based on User's recent diary reflections, User's recent emotional states, create my sentiment analysis";
            var reply = await GenerateAIResponse(userId, promptMessage);

            if (reply == null) return StatusCode(500, "Failed to get response from AI");

            var dto = new AIConversationDto
            {
                UserId = userId,
                UserMessage = promptMessage,
                AIResponse = SanitizeReply(reply),
                SentAt = DateTime.UtcNow
            };

            var result = await _aIConversationService.AddAsync(dto);
            return Ok(result);
        }

        [HttpPut("{aIConvoId}")]
        public async Task<IActionResult> UpdateAsync(Guid aIConvoId, [FromBody] string message)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var conversation = await _aIConversationService.GetByIdAsync(aIConvoId);
            if (conversation == null)
                return NotFound(new { Message = "AI Conversation not found" });
            var reply = await GenerateAIResponse(userId, message);
            var dto = new AIConversationDto
            {
                Id = conversation.Id,
                UserId = userId,
                UserMessage = message,
                AIResponse = SanitizeReply(reply),
                SentAt = DateTime.UtcNow
            };
            var result = await _aIConversationService.UpdateAsync(dto);
            return Ok(result);
        }

        [HttpDelete("{aIConvoId}")]
        public async Task<IActionResult> DeleteAsync(Guid aIConvoId)
        {
            var result = await _aIConversationService.DeleteAsync(aIConvoId);
            if (result)
                return Ok(new { Message = "AI Conversation deleted successfully" });
            return BadRequest(new { Message = "AI Conversation deleted failed" });
        }

        private async Task<string?> GenerateAIResponse(Guid userId, string userInput)
        {
            var apiKey = _configuration["OpenRouter:ApiKey"];
            if (string.IsNullOrEmpty(apiKey)) return null;

            var (conversations, diaryEntries, emotionTracks) = await GatherUserContext(userId);
            var systemPrompt = BuildSystemPrompt(diaryEntries, emotionTracks);

            var messages = new List<object>
            {
                new { role = "system", content = systemPrompt }
            };

            foreach (var convo in conversations.OrderBy(c => c.SentAt))
            {
                messages.Add(new { role = "user", content = convo.UserMessage });
                if (!string.IsNullOrEmpty(convo.AIResponse))
                    messages.Add(new { role = "assistant", content = convo.AIResponse });
            }

            messages.Add(new { role = "user", content = userInput });

            var requestBody = new
            {
                model = "deepseek/deepseek-chat-v3-0324:free",
                messages
            };

            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://openrouter.ai/api/v1/chat/completions")
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            var responseString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseString);

            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
        }

        private async Task<(IEnumerable<AIConversationDto>, IEnumerable<DiaryEntryDto>, IEnumerable<EmotionTrackDto>)>
            GatherUserContext(Guid userId)
        {
            var since = DateTime.UtcNow.AddDays(-7);
            var convos = await _aIConversationService.GetByUserIdAsync(userId, 1, int.MaxValue);
            var diaries = await _diaryEntryService.GetUserDiaryEntriesByDateRangeAsync(userId, since, DateTime.UtcNow);
            var emotions = await _emotionTrackService.GetUserEmotionTracksByDateRangeAsync(userId, since, DateTime.UtcNow);
            return (convos, diaries, emotions);
        }

        private static string BuildSystemPrompt(IEnumerable<DiaryEntryDto> diaryEntries, IEnumerable<EmotionTrackDto> emotionTracks)
        {
            var prompt = new StringBuilder();
            prompt.AppendLine("You are a professional and empathetic therapist AI. Speak gently, compassionately, and supportively.");
            prompt.AppendLine("Avoid giving medical advice or diagnosing. Instead, guide the user through emotional reflection and support.");
            prompt.AppendLine("Avoid long lists or formatting like Markdown. Respond in short readable paragraphs.");
            prompt.AppendLine("Your response must be between 200 to 500 words.");
            prompt.AppendLine("Politely reject inappropriate requests with: 'I'm here to support your mental well-being. Please keep questions relevant to emotional or personal topics.'");

            if (emotionTracks.Any())
            {
                prompt.AppendLine("User's recent emotional states include:");
                foreach (var e in emotionTracks)
                {
                    prompt.AppendLine($"- {e.CreatedAt:yyyy-MM-dd}:");
                    prompt.AppendLine($"  + Emotion: {e.Emotion}");
                    prompt.AppendLine($"  + Note: {e.Note} - Intensity Level: {e.IntensityLevel}");
                }
            }

            if (diaryEntries.Any())
            {
                prompt.AppendLine("User's recent diary reflections include:");
                foreach (var d in diaryEntries)
                    prompt.AppendLine($"- {d.Title} ({d.CreatedAt:yyyy-MM-dd}): {d.Content}");
            }

            return prompt.ToString().Trim();
        }

        private static string SanitizeReply(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            var clean = input
                .Replace("\r", "")
                .Replace("\n", " ")
                .Replace("###", "")
                .Replace("**", "")
                .Replace("*", "")
                .Replace("  ", " ");

            return clean.Trim();
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EXE201.HeartToHeart.BLL.IServices;
using System;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class TokenController : ControllerBase
    {
        private readonly ITokenService _tokenService;

        public TokenController(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpPost("validate")]
        public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenDto dto)
        {
            var isValid = await _tokenService.ValidateJwtTokenAsync(dto.Token);
            if (isValid)
                return Ok(new { Message = "Token is valid" });

            return BadRequest(new { Message = "Token is invalid or expired" });
        }

        [HttpPost("revoke-user-tokens")]
        public async Task<IActionResult> RevokeUserTokens([FromBody] RevokeUserTokensDto dto)
        {
            var result = await _tokenService.RevokeAllUserTokensAsync(dto.UserId);
            if (result)
                return Ok(new { Message = "All tokens for user revoked successfully" });

            return BadRequest(new { Message = "Failed to revoke user tokens" });
        }

        [HttpPost("get-user-id")]
        public IActionResult GetUserIdFromToken([FromBody] ValidateTokenDto dto)
        {
            var userId = _tokenService.GetUserIdFromToken(dto.Token);
            if (userId != null)
                return Ok(new { UserId = userId });

            return BadRequest(new { Message = "Failed to extract user ID from token" });
        }
    }

    public class ValidateTokenDto
    {
        public string Token { get; set; }
    }

    public class RevokeUserTokensDto
    {
        public Guid UserId { get; set; }
    }
}
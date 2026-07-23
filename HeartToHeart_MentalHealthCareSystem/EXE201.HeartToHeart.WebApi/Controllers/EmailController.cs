using EXE201.HeartToHeart.BLL.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EXE201.HeartToHeart.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("send-welcome")]
        public async Task<IActionResult> SendWelcomeEmail([FromBody] SendWelcomeEmailDto dto)
        {
            var result = await _emailService.SendWelcomeEmailAsync(dto.Email, dto.FirstName);
            if (result)
                return Ok(new { Message = "Welcome email sent successfully" });

            return BadRequest(new { Message = "Failed to send welcome email" });
        }

        [HttpPost("send-test")]
        public async Task<IActionResult> SendTestEmail([FromBody] SendTestEmailDto dto)
        {
            var subject = "Test Email from HeartToHeart";
            var body = $@"<html><body>
                <h2>Test Email</h2>
                <p>This is a test email sent to {dto.Email}.</p>
                </body></html>";
            var result = await _emailService.SendEmailAsync(dto.Email, subject, body);
            if (result)
                return Ok(new { Message = "Test email sent successfully" });

            return BadRequest(new { Message = "Failed to send test email" });
        }
    }

    public class SendWelcomeEmailDto
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
    }

    public class SendTestEmailDto
    {
        public string Email { get; set; }
    }
}

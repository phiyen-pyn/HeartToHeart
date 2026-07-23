using EXE201.HeartToHeart.BLL.IServices;
using EXE201.HeartToHeart.BLL.Services;
using EXE201.HeartToHeart.Common.Constants;
using EXE201.HeartToHeart.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EXE201.HeartToHeart.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("{reportId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetReportById(Guid reportId)
        {
            var comment = await _reportService.GetByIdAsync(reportId);
            if (comment == null)
                return NotFound(new { Message = "Report not found" });
            return Ok(comment);
        }

        [HttpGet("post/{postId}")]
        [AllowAnonymous]
        //[Authorize]
        public async Task<IActionResult> GetAllByPostIdAsync(Guid postId)
        {
            var comments = await _reportService.GetAllByPostIdAsync(postId);
            return Ok(comments);
        }

        [HttpPost]
        [AllowAnonymous]
        //[Authorize]
        public async Task<IActionResult> CreateAsync([FromBody] CreateReportDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var reportDto = new ReportDto
            {
                PostId = dto.PostId,
                ReporterId = userId,
                Reason = dto.Reason,
            };
            var result = await _reportService.CreateAsync(reportDto);
            return Ok(result);
        }

        [HttpPut("{reportId}")]
        [AllowAnonymous]
        //[Authorize]
        public async Task<IActionResult> UpdateAsync(Guid reportId, [FromBody] UpdateReportDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var reportDto = new ReportDto
            {
                Reason = dto.Reason,
                Status = dto.Status,
            };
            var result = await _reportService.UpdateAsync(reportId, reportDto);
            return Ok(result);
        }

        [HttpDelete("{reportId}")]
        [AllowAnonymous]
        //[Authorize]
        public async Task<IActionResult> DeleteAsync(Guid reportId)
        {
            var result = await _reportService.DeleteAsync(reportId);
            if (result)
                return Ok(new { Message = "Report deleted successfully" });
            return BadRequest(new { Message = "Report deleted failed" });
        }
    }
}

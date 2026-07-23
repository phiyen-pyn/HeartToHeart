using EXE201.HeartToHeart.BLL.IServices;
using EXE201.HeartToHeart.DAL.IRepositories;
using EXE201.HeartToHeart.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EXE201.HeartToHeart.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IHolidaySeederService _holidaySeederService;
        private readonly ILogger<AppointmentController> _logger;
        private readonly ICounselorRepository _counselorRepository;
        private readonly IConfiguration _configuration;

        public AppointmentController(
            IAppointmentService appointmentService,
            IHolidaySeederService holidaySeederService,
            ICounselorRepository counselorRepository,
            ILogger<AppointmentController> logger,
            IConfiguration configuration)
        {
            _appointmentService = appointmentService;
            _holidaySeederService = holidaySeederService;
            _counselorRepository = counselorRepository;
            _logger = logger;
            _configuration = configuration;
        }

        #region Google Calendar Integration

        [HttpGet("google-calendar/auth-url")]
        [Authorize]
        public async Task<IActionResult> GetGoogleCalendarAuthUrl()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == Guid.Empty)
                {
                    return Unauthorized("Invalid user token");
                }

                var isAlreadyAuthorized = await _appointmentService.IsGoogleCalendarAuthorizedAsync(currentUserId);
                if (isAlreadyAuthorized)
                {
                    return Ok(new { message = "Already authorized", authorized = true, authUrl = (string)null });
                }

                var authUrl = await _appointmentService.GetGoogleCalendarAuthUrlAsync(currentUserId);
                if (string.IsNullOrEmpty(authUrl))
                {
                    return StatusCode(500, new { message = "Failed to generate authorization URL" });
                }

                // Return the full backend URL for redirection
                return Ok(new { message = "Authorization URL generated successfully", authorized = false, authUrl = authUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Google Calendar auth URL for user {UserId}", GetCurrentUserId());
                return StatusCode(500, new { message = "An error occurred while generating authorization URL" });
            }
        }

        [HttpPost("google-calendar/callback")]
        [AllowAnonymous]
        public async Task<IActionResult> HandleGoogleCalendarCallbackPost([FromBody] GoogleCalendarCallbackDto callbackDto)
        {
            try
            {
                _logger.LogInformation("Processing Google Calendar POST callback with code: {HasCode}, state: {State}",
                    !string.IsNullOrEmpty(callbackDto.Code), callbackDto.State);

                if (string.IsNullOrEmpty(callbackDto.Code))
                {
                    _logger.LogWarning("Authorization code is missing in POST callback");
                    return BadRequest(new
                    {
                        message = "Authorization code is required",
                        authorized = false,
                        error = "missing_code"
                    });
                }

                if (string.IsNullOrEmpty(callbackDto.State))
                {
                    _logger.LogWarning("State parameter is missing in POST callback");
                    return BadRequest(new
                    {
                        message = "State parameter is required",
                        authorized = false,
                        error = "missing_state"
                    });
                }

                if (!Guid.TryParse(callbackDto.State, out var userId))
                {
                    _logger.LogWarning("Invalid state parameter: {State}", callbackDto.State);
                    return BadRequest(new
                    {
                        message = "Invalid state parameter",
                        authorized = false,
                        error = "invalid_state"
                    });
                }

                var success = await _appointmentService.HandleGoogleCalendarCallbackAsync(userId, callbackDto.Code);
                if (!success)
                {
                    _logger.LogError("Failed to handle Google Calendar POST callback for user {UserId}", userId);
                    return BadRequest(new
                    {
                        message = "Failed to authorize Google Calendar access",
                        authorized = false,
                        error = "auth_failed",
                        userId = userId
                    });
                }

                _logger.LogInformation("Successfully processed Google Calendar POST callback for user {UserId}", userId);
                return Ok(new
                {
                    message = "Google Calendar authorization successful",
                    authorized = true,
                    userId = userId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling Google Calendar POST callback");
                return StatusCode(500, new
                {
                    message = "An error occurred while processing authorization",
                    authorized = false,
                    error = "internal_error"
                });
            }
        }

        [HttpGet("google-calendar/status")]
        [Authorize]
        public async Task<IActionResult> GetGoogleCalendarAuthStatus()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == Guid.Empty)
                {
                    return Unauthorized("Invalid user token");
                }

                var isAuthorized = await _appointmentService.IsGoogleCalendarAuthorizedAsync(currentUserId);
                return Ok(new
                {
                    authorized = isAuthorized,
                    message = isAuthorized ? "Google Calendar is authorized" : "Google Calendar authorization required"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking Google Calendar auth status");
                return StatusCode(500, new { message = "An error occurred while checking authorization status" });
            }
        }

        [HttpGet("google-calendar/callback")]
        [AllowAnonymous]
        public async Task<IActionResult> HandleGoogleCalendarCallbackGet(
                                        [FromQuery] string code,
                                        [FromQuery] string state,
                                        [FromQuery] string? error = null)
        {
            try
            {
                _logger.LogInformation("Processing Google Calendar GET callback with code: {HasCode}, state: {State}, error: {Error}",
                    !string.IsNullOrEmpty(code), state, error);

                // Check for authorization error
                if (!string.IsNullOrEmpty(error))
                {
                    _logger.LogWarning("Google OAuth error: {Error}", error);
                    return Redirect($"{_configuration["AppSettings:FrontendUrl"]}my-appointments?authorized=false&error={error}");
                }

                if (string.IsNullOrEmpty(code))
                {
                    _logger.LogWarning("Authorization code is missing in GET callback");
                    return Redirect($"{_configuration["AppSettings:FrontendUrl"]}my-appointments?authorized=false&error=missing_code");
                }

                if (string.IsNullOrEmpty(state))
                {
                    _logger.LogWarning("State parameter is missing in GET callback");
                    return Redirect($"{_configuration["AppSettings:FrontendUrl"]}my-appointments?authorized=false&error=missing_state");
                }

                if (!Guid.TryParse(state, out var userId))
                {
                    _logger.LogWarning("Invalid state parameter: {State}", state);
                    return Redirect($"{_configuration["AppSettings:FrontendUrl"]}my-appointments?authorized=false&error=invalid_state");
                }

                var success = await _appointmentService.HandleGoogleCalendarCallbackAsync(userId, code);
                if (!success)
                {
                    _logger.LogError("Failed to handle Google Calendar GET callback for user {UserId}", userId);
                    return Redirect($"{_configuration["AppSettings:FrontendUrl"]}my-appointments?authorized=false&error=auth_failed&userId={userId}");
                }

                _logger.LogInformation("Successfully processed Google Calendar GET callback for user {UserId}", userId);
                return Redirect($"{_configuration["AppSettings:FrontendUrl"]}my-appointments?authorized=true&userId={userId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling Google Calendar GET callback");
                return Redirect($"{_configuration["AppSettings:FrontendUrl"]}my-appointments?authorized=false&error=internal_error");
            }
        }

        [HttpGet("{appointmentId}/google-meet-link")]
        [Authorize]
        public async Task<IActionResult> GetGoogleMeetLink(Guid appointmentId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == Guid.Empty)
                {
                    return Unauthorized("Invalid user token");
                }

                var appointment = await _appointmentService.GetAppointmentByIdAsync(appointmentId);
                if (appointment == null)
                {
                    return NotFound(new { message = "Appointment not found" });
                }

                var userRoles = GetUserRoles();
                var isOwner = appointment.UserId == currentUserId;
                var isAdmin = userRoles.Contains("Admin");
                var isCounselor = await IsCounselorForAppointment(appointment.CounselorId, currentUserId);

                if (!isOwner && !isAdmin && !isCounselor)
                {
                    return Forbid("You can only access your own appointments");
                }

                var isAuthorized = await _appointmentService.IsGoogleCalendarAuthorizedAsync(appointment.UserId);
                if (!isAuthorized)
                {
                    return BadRequest(new
                    {
                        message = "Google Calendar authorization required",
                        authorized = false,
                        meetLink = (string)null,
                        authUrl = await _appointmentService.GetGoogleCalendarAuthUrlAsync(appointment.UserId)
                    });
                }

                return Ok(new
                {
                    meetLink = appointment.GoogleMeetLink,
                    hasLink = !string.IsNullOrEmpty(appointment.GoogleMeetLink),
                    message = !string.IsNullOrEmpty(appointment.GoogleMeetLink)
                        ? "Google Meet link retrieved successfully"
                        : "No Google Meet link found for this appointment",
                    appointmentId = appointmentId,
                    googleCalendarEventId = appointment.GoogleCalendarEventId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Google Meet link for appointment {AppointmentId}", appointmentId);
                return StatusCode(500, new { message = "An error occurred while retrieving the Google Meet link" });
            }
        }

        #endregion

        #region Appointment CRUD Operations

        [HttpGet("{appointmentId}")]
        [Authorize]
        public async Task<IActionResult> GetAppointmentById(Guid appointmentId)
        {
            try
            {
                var appointment = await _appointmentService.GetAppointmentByIdAsync(appointmentId);
                if (appointment == null)
                    return NotFound(new { Message = "Appointment not found" });

                var currentUserId = GetCurrentUserId();
                if (appointment.UserId != currentUserId && !User.IsInRole("Admin") && !User.IsInRole("Counselor"))
                {
                    return Forbid();
                }

                return Ok(appointment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting appointment {AppointmentId}", appointmentId);
                return StatusCode(500, new { Message = "An error occurred while retrieving the appointment" });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var currentUserId = GetCurrentUserId();

                _logger.LogInformation("Creating appointment request from user {UserId} for counselor {CounselorId} at {AppointmentDate}",
                    currentUserId, createDto.CounselorId, createDto.AppointmentDate);

                var (appointment, errorMessage) = await _appointmentService.CreateAppointmentAsync(currentUserId, createDto);

                if (appointment == null)
                {
                    _logger.LogWarning("Failed to create appointment for user {UserId}: {ErrorMessage}", currentUserId, errorMessage);

                    // Check if this is a Google Calendar authorization error
                    if (errorMessage != null && errorMessage.Contains("Google Calendar authorization required"))
                    {
                        var authUrl = await _appointmentService.GetGoogleCalendarAuthUrlAsync(currentUserId);
                        return BadRequest(new
                        {
                            message = errorMessage,
                            requireAuth = true,
                            authUrl = authUrl
                        });
                    }

                    return BadRequest(new
                    {
                        message = errorMessage ?? "Failed to create appointment. Please check the appointment details and try again.",
                        details = errorMessage
                    });
                }

                _logger.LogInformation("Successfully created appointment {AppointmentId} for user {UserId} with Google Calendar event {EventId} and Meet link: {HasMeetLink}",
                    appointment.Id, currentUserId, appointment.GoogleCalendarEventId, !string.IsNullOrEmpty(appointment.GoogleMeetLink));

                return CreatedAtAction(
                    nameof(GetAppointmentById),
                    new { appointmentId = appointment.Id },
                    appointment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating appointment for user {UserId}", GetCurrentUserId());
                return StatusCode(500, new { Message = "An unexpected error occurred while creating the appointment" });
            }
        }

        [HttpPut("{appointmentId}")]
        [Authorize]
        public async Task<IActionResult> UpdateAppointment(
            Guid appointmentId,
            [FromBody] UpdateAppointmentDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var appointment = await _appointmentService.GetAppointmentByIdAsync(appointmentId);
                if (appointment == null)
                    return NotFound(new { Message = "Appointment not found" });

                var currentUserId = GetCurrentUserId();
                if (appointment.UserId != currentUserId && !User.IsInRole("Admin"))
                    return Forbid();

                var result = await _appointmentService.UpdateAppointmentAsync(appointmentId, updateDto, currentUserId);
                if (!result)
                    return BadRequest(new { Message = "Failed to update appointment" });

                return Ok(new { Message = "Appointment updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating appointment {AppointmentId}", appointmentId);
                return StatusCode(500, new { Message = "An error occurred while updating the appointment" });
            }
        }

        [HttpPut("{appointmentId}/status")]
        [Authorize(Roles = "Counselor,Admin")]
        public async Task<IActionResult> UpdateAppointmentStatus(
            Guid appointmentId,
            [FromBody] UpdateAppointmentStatusDto statusDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var currentUserId = GetCurrentUserId();
                var result = await _appointmentService.UpdateAppointmentStatusAsync(appointmentId, statusDto, currentUserId);
                if (!result)
                    return BadRequest(new { Message = "Failed to update appointment status" });

                return Ok(new { Message = "Appointment status updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating appointment status for {AppointmentId}", appointmentId);
                return StatusCode(500, new { Message = "An error occurred while updating the appointment status" });
            }
        }

        [HttpPut("{appointmentId}/cancel")]
        [Authorize]
        public async Task<IActionResult> CancelAppointment(Guid appointmentId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _appointmentService.CancelAppointmentAsync(appointmentId, currentUserId);
                if (!result)
                    return BadRequest(new { Message = "Failed to cancel appointment. Please check if you're authorized and the appointment can be cancelled." });

                return Ok(new { Message = "Appointment cancelled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling appointment {AppointmentId}", appointmentId);
                return StatusCode(500, new { Message = "An error occurred while cancelling the appointment" });
            }
        }

        [HttpDelete("{appointmentId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAppointment(Guid appointmentId)
        {
            try
            {
                var result = await _appointmentService.DeleteAppointmentAsync(appointmentId);
                if (!result)
                    return BadRequest(new { Message = "Failed to delete appointment" });

                return Ok(new { Message = "Appointment deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting appointment {AppointmentId}", appointmentId);
                return StatusCode(500, new { Message = "An error occurred while deleting the appointment" });
            }
        }

        #endregion

        #region Appointment Availability and Scheduling

        [HttpGet("counselor-availability")]
        [Authorize]
        public async Task<IActionResult> GetCounselorAvailability(
            [FromQuery] DateTime date,
            [FromQuery] string? specialization = null)
        {
            try
            {
                if (date.Date < DateTime.UtcNow.Date)
                    return BadRequest(new { Message = "Cannot check availability for past dates" });

                var availability = await _appointmentService.GetCounselorAvailabilityAsync(date, specialization);
                return Ok(availability);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting counselor availability for {Date}", date);
                return StatusCode(500, new { Message = "An error occurred while getting counselor availability" });
            }
        }

        [HttpGet("time-slots")]
        [Authorize]
        public async Task<IActionResult> GetAvailableTimeSlots(
            [FromQuery] Guid counselorId,
            [FromQuery] DateTime date)
        {
            try
            {
                if (date.Date < DateTime.UtcNow.Date)
                    return BadRequest(new { Message = "Cannot get time slots for past dates" });

                var timeSlots = await _appointmentService.GetAvailableTimeSlotsAsync(counselorId, date);
                return Ok(new
                {
                    CounselorId = counselorId,
                    Date = date.Date,
                    AvailableSlots = timeSlots
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available time slots for counselor {CounselorId} on {Date}", counselorId, date);
                return StatusCode(500, new { Message = "An error occurred while getting available time slots" });
            }
        }

        [HttpPost("validate-time")]
        [Authorize]
        public async Task<IActionResult> ValidateAppointmentTime([FromBody] ValidateAppointmentTimeDto validateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var (isValid, errorMessage) = await _appointmentService.ValidateAppointmentTimeDetailedAsync(
                    validateDto.CounselorId,
                    validateDto.AppointmentDate,
                    validateDto.DurationMinutes);

                return Ok(new
                {
                    IsValid = isValid,
                    Message = errorMessage
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating appointment time");
                return StatusCode(500, new { Message = "An error occurred while validating the appointment time" });
            }
        }

        #endregion

        #region User-Specific Endpoints

        [HttpGet("my-appointments")]
        [Authorize]
        public async Task<IActionResult> GetMyAppointments(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var appointments = await _appointmentService.GetUserAppointmentsAsync(currentUserId, page, pageSize);
                var totalCount = await _appointmentService.GetUserAppointmentsCountAsync(currentUserId);

                return Ok(new
                {
                    Appointments = appointments,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting my appointments for user {UserId}", GetCurrentUserId());
                return StatusCode(500, new { Message = "An error occurred while getting your appointments" });
            }
        }

        [HttpGet("my-upcoming-appointments")]
        [Authorize]
        public async Task<IActionResult> GetMyUpcomingAppointments([FromQuery] int days = 7)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var appointments = await _appointmentService.GetUpcomingAppointmentsAsync(currentUserId, days);

                return Ok(new
                {
                    UpcomingAppointments = appointments,
                    DaysAhead = days,
                    Count = appointments.Count()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting upcoming appointments for user {UserId}", GetCurrentUserId());
                return StatusCode(500, new { Message = "An error occurred while getting upcoming appointments" });
            }
        }

        [HttpGet("my-appointment-history")]
        [Authorize]
        public async Task<IActionResult> GetMyAppointmentHistory([FromQuery] int months = 3)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var appointments = await _appointmentService.GetAppointmentHistoryAsync(currentUserId, months);

                return Ok(new
                {
                    AppointmentHistory = appointments,
                    MonthsBack = months,
                    Count = appointments.Count()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting appointment history for user {UserId}", GetCurrentUserId());
                return StatusCode(500, new { Message = "An error occurred while getting appointment history" });
            }
        }

        #endregion

        #region Counselor-Specific Endpoints

        [HttpPost("counselor/set-availability")]
        [Authorize(Roles = "Counselor")]
        public async Task<IActionResult> SetCounselorAvailability([FromBody] SetCounselorAvailabilityDto availabilityDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var currentUserId = GetCurrentUserId();
                var counselorId = await GetCounselorIdFromUserIdAsync(currentUserId);
                if (counselorId == Guid.Empty)
                    return BadRequest(new { Message = "Counselor profile not found" });

                var result = await _appointmentService.SetCounselorAvailabilityAsync(counselorId, availabilityDto);
                if (!result)
                    return BadRequest(new { Message = "Failed to set availability" });

                return Ok(new { Message = "Availability updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting counselor availability");
                return StatusCode(500, new { Message = "An error occurred while setting availability" });
            }
        }

        [HttpPost("counselor/set-schedule-template")]
        [Authorize(Roles = "Counselor")]
        public async Task<IActionResult> SetCounselorScheduleTemplate([FromBody] List<CounselorScheduleTemplateDto> scheduleTemplates)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var currentUserId = GetCurrentUserId();
                var counselorId = await GetCounselorIdFromUserIdAsync(currentUserId);
                if (counselorId == Guid.Empty)
                    return BadRequest(new { Message = "Counselor profile not found" });

                var result = await _appointmentService.SetCounselorScheduleTemplateAsync(counselorId, scheduleTemplates);
                if (!result)
                    return BadRequest(new { Message = "Failed to set schedule template" });

                return Ok(new { Message = "Schedule template updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting counselor schedule template");
                return StatusCode(500, new { Message = "An error occurred while setting schedule template" });
            }
        }

        #endregion

        #region Admin Endpoints

        [HttpGet("statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAppointmentStatistics()
        {
            try
            {
                var totalCount = await _appointmentService.GetTotalAppointmentsCountAsync();

                return Ok(new
                {
                    TotalAppointments = totalCount,
                    GeneratedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting appointment statistics");
                return StatusCode(500, new { Message = "An error occurred while getting appointment statistics" });
            }
        }

        [HttpPost("admin/seed-holidays")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SeedHolidays([FromQuery] int year = 0)
        {
            try
            {
                var targetYear = year == 0 ? DateTime.Now.Year : year;
                await _holidaySeederService.SeedVietnamHolidaysAsync(targetYear);

                return Ok(new
                {
                    Message = $"Successfully seeded holidays for year {targetYear}",
                    Year = targetYear
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding holidays for year {Year}", year);
                return StatusCode(500, new { Message = "An error occurred while seeding holidays" });
            }
        }

        #endregion

        #region Helper Methods

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("User ID not found in claims");
            }
            return userId;
        }

        private List<string> GetUserRoles()
        {
            try
            {
                var roles = User.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList();

                if (!roles.Any())
                {
                    roles = User.Claims
                        .Where(c => c.Type == "role" || c.Type == "roles")
                        .Select(c => c.Value)
                        .ToList();
                }

                return roles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user roles for user {UserId}", GetCurrentUserId());
                return new List<string>();
            }
        }

        private async Task<bool> IsCounselorForAppointment(Guid counselorId, Guid userId)
        {
            var counselor = await _counselorRepository.GetCounselorByUserIdAsync(userId);
            return counselor != null && counselor.Id == counselorId;
        }

        private async Task<Guid> GetCounselorIdFromUserIdAsync(Guid userId)
        {
            try
            {
                var counselor = await _counselorRepository.GetCounselorByUserIdAsync(userId);
                return counselor?.Id ?? Guid.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting counselor ID for user {UserId}", userId);
                return Guid.Empty;
            }
        }

        #endregion
    }
}
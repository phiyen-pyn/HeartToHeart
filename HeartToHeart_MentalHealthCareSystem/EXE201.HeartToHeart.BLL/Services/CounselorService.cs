using EXE201.HeartToHeart.BLL.IServices;
using EXE201.HeartToHeart.DAL.Entities.Application;
using EXE201.HeartToHeart.DAL.IRepositories;
using EXE201.HeartToHeart.DAL.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace EXE201.HeartToHeart.BLL.Services
{
    public class CounselorService : ICounselorService
    {
        private readonly ICounselorRepository _counselorRepository;
        private readonly ICounselorAvailabilityRepository _counselorAvailabilityRepository;
        private readonly ILogger<CounselorService> _logger;
        private readonly IGoogleCalendarService _googleCalendarService;

        public CounselorService(
            ICounselorRepository counselorRepository,
            ICounselorAvailabilityRepository counselorAvailabilityRepository,
            ILogger<CounselorService> logger,
            IGoogleCalendarService googleCalendarService)
        {
            _counselorRepository = counselorRepository;
            _counselorAvailabilityRepository = counselorAvailabilityRepository;
            _logger = logger;
            _googleCalendarService = googleCalendarService;
        }

        /// <summary>
        /// Verify a counselor - Only accessible by Admin
        /// </summary>
        public async Task<(bool success, string message)> VerifyCounselorAsync(Guid counselorId, Guid verifiedByUserId)
        {
            try
            {
                var counselor = await _counselorRepository.GetCounselorByIdAsync(counselorId);
                if (counselor == null)
                {
                    return (false, "Counselor not found");
                }

                if (counselor.IsVerified)
                {
                    return (false, "Counselor is already verified");
                }

                counselor.IsVerified = true;
                counselor.UpdatedAt = DateTime.UtcNow;
                counselor.UpdatedBy = verifiedByUserId.ToString();

                var success = await _counselorRepository.UpdateCounselorAsync(counselor);
                if (success)
                {
                    _logger.LogInformation("Counselor {CounselorId} verified by user {VerifiedBy}", counselorId, verifiedByUserId);
                    return (true, "Counselor verified successfully");
                }

                return (false, "Failed to update counselor verification status");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying counselor {CounselorId}", counselorId);
                return (false, "An error occurred while verifying the counselor");
            }
        }

        /// <summary>
        /// Unverify a counselor - Only accessible by Admin
        /// </summary>
        public async Task<(bool success, string message)> UnverifyCounselorAsync(Guid counselorId, Guid unverifiedByUserId)
        {
            try
            {
                var counselor = await _counselorRepository.GetCounselorByIdAsync(counselorId);
                if (counselor == null)
                {
                    return (false, "Counselor not found");
                }

                counselor.IsVerified = false;
                counselor.IsAvailable = false; // Also set unavailable when unverified
                counselor.UpdatedAt = DateTime.UtcNow;
                counselor.UpdatedBy = unverifiedByUserId.ToString();

                var success = await _counselorRepository.UpdateCounselorAsync(counselor);
                if (success)
                {
                    _logger.LogInformation("Counselor {CounselorId} unverified by user {UnverifiedBy}", counselorId, unverifiedByUserId);
                    return (true, "Counselor unverified successfully");
                }

                return (false, "Failed to update counselor verification status");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unverifying counselor {CounselorId}", counselorId);
                return (false, "An error occurred while unverifying the counselor");
            }
        }

        /// <summary>
        /// Set counselor general availability - Can be used by Admin or Counselor themselves
        /// </summary>
        public async Task<(bool success, string message)> SetCounselorGeneralAvailabilityAsync(Guid counselorId, bool isAvailable, Guid updatedByUserId)
        {
            try
            {
                var counselor = await _counselorRepository.GetCounselorByIdAsync(counselorId);
                if (counselor == null)
                {
                    return (false, "Counselor not found");
                }

                if (!counselor.IsVerified && isAvailable)
                {
                    return (false, "Cannot set counselor as available until they are verified");
                }

                counselor.IsAvailable = isAvailable;
                counselor.UpdatedAt = DateTime.UtcNow;
                counselor.UpdatedBy = updatedByUserId.ToString();

                var success = await _counselorRepository.UpdateCounselorAsync(counselor);
                if (success)
                {
                    var status = isAvailable ? "available" : "unavailable";
                    _logger.LogInformation("Counselor {CounselorId} set as {Status} by user {UpdatedBy}", counselorId, status, updatedByUserId);
                    return (true, $"Counselor set as {status} successfully");
                }

                return (false, "Failed to update counselor availability");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting counselor availability for {CounselorId}", counselorId);
                return (false, "An error occurred while updating counselor availability");
            }
        }

        /// <summary>
        /// Update counselor profile information
        /// </summary>
        public async Task<(bool success, string message)> UpdateCounselorProfileAsync(Guid counselorId, UpdateCounselorDto updateDto, Guid updatedByUserId)
        {
            try
            {
                var counselor = await _counselorRepository.GetCounselorByIdAsync(counselorId);
                if (counselor == null)
                {
                    return (false, "Counselor not found");
                }

                // Update fields if provided
                if (!string.IsNullOrEmpty(updateDto.Description))
                    counselor.Description = updateDto.Description;

                if (!string.IsNullOrEmpty(updateDto.Specialization))
                    counselor.Specialization = updateDto.Specialization;

                if (!string.IsNullOrEmpty(updateDto.LicenseNumber))
                    counselor.LicenseNumber = updateDto.LicenseNumber;

                if (updateDto.ExperienceYears.HasValue)
                    counselor.ExperienceYears = updateDto.ExperienceYears.Value;

                if (updateDto.HourlyRate.HasValue)
                    counselor.HourlyRate = updateDto.HourlyRate.Value;

                counselor.UpdatedAt = DateTime.UtcNow;
                counselor.UpdatedBy = updatedByUserId.ToString();

                var success = await _counselorRepository.UpdateCounselorAsync(counselor);
                if (success)
                {
                    _logger.LogInformation("Counselor profile {CounselorId} updated by user {UpdatedBy}", counselorId, updatedByUserId);
                    return (true, "Counselor profile updated successfully");
                }

                return (false, "Failed to update counselor profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating counselor profile for {CounselorId}", counselorId);
                return (false, "An error occurred while updating counselor profile");
            }
        }

        /// <summary>
        /// Get counselor details
        /// </summary>
        public async Task<CounselorDto?> GetCounselorAsync(Guid counselorId)
        {
            try
            {
                var counselor = await _counselorRepository.GetCounselorByIdAsync(counselorId);
                if (counselor == null)
                {
                    return null;
                }

                return MapToCounselorDto(counselor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting counselor {CounselorId}", counselorId);
                return null;
            }
        }

        /// <summary>
        /// Get all counselors with optional filters
        /// </summary>
        public async Task<IEnumerable<CounselorDto>> GetCounselorsAsync(CounselorFilterDto? filter = null)
        {
            try
            {
                IEnumerable<Counselor> counselors;

                if (filter?.Specialization != null)
                {
                    counselors = await _counselorRepository.GetCounselorsBySpecializationAsync(filter.Specialization);
                }
                else if (filter?.OnlyAvailable == true)
                {
                    counselors = await _counselorRepository.GetAvailableCounselorsAsync();
                }
                else
                {
                    counselors = await _counselorRepository.GetAllCounselorsAsync();
                }

                // Apply additional filters
                if (filter != null)
                {
                    if (filter.OnlyVerified.HasValue)
                    {
                        counselors = counselors.Where(c => c.IsVerified == filter.OnlyVerified.Value);
                    }
                }

                return counselors.Select(MapToCounselorDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting counselors");
                return Enumerable.Empty<CounselorDto>();
            }
        }

        public async Task<CounselorDto?> GetCounselorByUserIdAsync(Guid userId)
        {
            try
            {
                return MapToCounselorDto( await _counselorRepository.GetCounselorByUserIdAsync(userId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting counselor by user ID {UserId}", userId);
                return null;
            }
        }

        /// <summary>
        /// Get counselor status for debugging and validation
        /// </summary>
        public async Task<CounselorStatusDto> GetCounselorStatusAsync(Guid counselorId)
        {
            try
            {
                var counselor = await _counselorRepository.GetCounselorByIdAsync(counselorId);
                if (counselor == null)
                {
                    return new CounselorStatusDto
                    {
                        CounselorId = counselorId,
                        Exists = false,
                        IsVerified = false,
                        IsAvailable = false,
                        IsUserActive = false,
                        Message = "Counselor not found"
                    };
                }

                return new CounselorStatusDto
                {
                    CounselorId = counselorId,
                    Exists = true,
                    IsVerified = counselor.IsVerified,
                    IsAvailable = counselor.IsAvailable,
                    IsUserActive = counselor.User?.IsActive ?? false,
                    Message = GetCounselorStatusMessage(counselor)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting counselor status for {CounselorId}", counselorId);
                return new CounselorStatusDto
                {
                    CounselorId = counselorId,
                    Exists = false,
                    Message = "Error retrieving counselor status"
                };
            }
        }

        public async Task<int> GetTotalCounselorsCountAsync()
        {
            try
            {
                return await _counselorRepository.GetTotalCounselorsCountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting counselor count");
                return 0;
            }
        }

        public async Task<int> GetTotalCounselorsCountByMonthAsync(int year, int month)
        {
            try
            {
                return await _counselorRepository.GetTotalCounselorsCountByMonthAsync(year, month);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting counselor count by month for {Year}-{Month}", year, month);
                return 0;
            }
        }

        public async Task<int> GetVerifiedCounselorsCountAsync() 
        { 
            try
            {
                return await _counselorRepository.GetVerifiedCounselorsCountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting verified counselor count");
                return 0;
            }
        }

        public async Task<int> GetVerifiedCounselorsCountByMonthAsync(int year, int month)
        {
            try
            {
                return await _counselorRepository.GetVerifiedCounselorsCountByMonthAsync(year, month);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting verified counselor count by month for { Year}-{ Month}", year, month);
                return 0;
            }
        }

        private string GetCounselorStatusMessage(Counselor counselor)
        {
            if (!counselor.User.IsActive)
                return "User account is inactive";

            if (!counselor.IsVerified)
                return "Counselor is not verified";

            if (!counselor.IsAvailable)
                return "Counselor is not available for bookings";

            return "Counselor is active and available";
        }

        private static CounselorDto MapToCounselorDto(Counselor counselor)
        {
            return new CounselorDto
            {
                Id = counselor.Id,
                UserId = counselor.UserId,
                FirstName = counselor.User.FirstName,
                LastName = counselor.User.LastName,
                Email = counselor.User.Email,
                Description = counselor.Description,
                Specialization = counselor.Specialization,
                LicenseNumber = counselor.LicenseNumber,
                ExperienceYears = counselor.ExperienceYears,
                IsVerified = counselor.IsVerified,
                HourlyRate = counselor.HourlyRate,
                IsAvailable = counselor.IsAvailable,
                CreatedAt = counselor.CreatedAt,
                UpdatedAt = counselor.UpdatedAt
            };
        }

        /// <summary>
        /// Get appointments for a specific counselor with pagination
        /// </summary>
        public async Task<IEnumerable<AppointmentDto>> GetCounselorAppointmentsAsync(Guid counselorId, int page = 1, int pageSize = 10)
        {
            try
            {
                // Validate pagination parameters
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100; // Limit max page size to prevent performance issues

                // First check if counselor exists
                var counselorExists = await _counselorRepository.CounselorExistsAsync(counselorId);
                if (!counselorExists)
                {
                    _logger.LogWarning("Counselor {CounselorId} not found when getting appointments", counselorId);
                    return Enumerable.Empty<AppointmentDto>();
                }

                var appointments = await _counselorRepository.GetCounselorAppointmentsAsync(counselorId, page, pageSize);
                return await MapToAppointmentDtosWithMeetLinksAsync(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting appointments for counselor {CounselorId} with pagination (page: {Page}, pageSize: {PageSize})",
                    counselorId, page, pageSize);
                return Enumerable.Empty<AppointmentDto>();
            }
        }

        private static AppointmentDto MapToAppointmentDto(Appointment appointment)
        {
            return new AppointmentDto
            {
                Id = appointment.Id,
                UserId = appointment.UserId,
                CounselorId = appointment.CounselorId,
                AppointmentDate = appointment.AppointmentDate,
                Reason = appointment.Reason,
                Status = appointment.Status,
                Notes = appointment.Notes,
                DurationMinutes = appointment.DurationMinutes,
                CreatedAt = appointment.CreatedAt,
                UpdatedAt = appointment.UpdatedAt,
                UserName = $"{appointment.User.FirstName} {appointment.User.LastName}".Trim(),
                UserEmail = appointment.User.Email,
                CounselorName = $"{appointment.Counselor.User.FirstName} {appointment.Counselor.User.LastName}".Trim(),
                CounselorSpecialization = appointment.Counselor.Specialization,
                GoogleCalendarEventId = appointment.GoogleCalendarEventId,
                GoogleMeetLink = null // This would need to be populated from wherever Google Meet links are stored
            };
        }

        /// <summary>
        /// Maps appointments to DTOs and includes Google Meet links
        /// </summary>
        private async Task<IEnumerable<AppointmentDto>> MapToAppointmentDtosWithMeetLinksAsync(IEnumerable<Appointment> appointments)
        {
            var appointmentDtos = new List<AppointmentDto>();

            foreach (var appointment in appointments)
            {
                var appointmentDto = MapToAppointmentDto(appointment);

                // Get Google Meet link if calendar event exists
                if (!string.IsNullOrEmpty(appointment.GoogleCalendarEventId))
                {
                    try
                    {
                        var meetLink = await _googleCalendarService.GetGoogleMeetLinkAsync(
                            appointment.GoogleCalendarEventId, appointment.UserId);
                        appointmentDto.GoogleMeetLink = meetLink;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to get Google Meet link for appointment {AppointmentId} with calendar event {EventId}",
                            appointment.Id, appointment.GoogleCalendarEventId);
                        // Continue without the meet link rather than failing the entire operation
                    }
                }

                appointmentDtos.Add(appointmentDto);
            }

            return appointmentDtos;
        }
    }
}
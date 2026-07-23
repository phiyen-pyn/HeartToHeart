using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.Common.Constants
{
    public static class AppointmentConstants
    {
        public static class Status
        {
            public const string Pending = "Pending";
            public const string Confirmed = "Confirmed";
            public const string Completed = "Completed";
            public const string Cancelled = "Cancelled";
            public const string Rescheduled = "Rescheduled";
        }

        public static readonly List<string> ValidStatuses = new()
        {
            Status.Pending,
            Status.Confirmed,
            Status.Completed,
            Status.Cancelled,
            Status.Rescheduled
        };

        public static class BusinessRules
        {
            public const int MinimumAdvanceBookingHours = 24;
            public const int MaximumAdvanceBookingDays = 90;
            public const int MinimumDurationMinutes = 30;
            public const int MaximumDurationMinutes = 180;
            public const int DefaultDurationMinutes = 60;
            public const int WorkingHourStart = 8; // 8 AM
            public const int WorkingHourEnd = 20; // 8 PM
            public const int SlotIntervalMinutes = 30;
        }

        public static bool IsValidStatus(string status)
        {
            return ValidStatuses.Contains(status, StringComparer.OrdinalIgnoreCase);
        }

        public static string GetNormalizedStatus(string status)
        {
            return ValidStatuses.FirstOrDefault(s =>
                string.Equals(s, status, StringComparison.OrdinalIgnoreCase)) ?? Status.Pending;
        }
    }
}

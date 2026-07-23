using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Models
{
    public class GoogleCalendarCallbackDto
    {
        [Required]
        public string Code { get; set; } = string.Empty;

        [Required]
        public string State { get; set; } = string.Empty;

        public string? Error { get; set; }

        public string? ErrorDescription { get; set; }
    }
}

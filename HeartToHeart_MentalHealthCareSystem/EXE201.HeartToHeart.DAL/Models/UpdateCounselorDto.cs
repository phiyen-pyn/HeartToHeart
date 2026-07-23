using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Models
{
    public class UpdateCounselorDto
    {
        public string? Description { get; set; }
        public string? Specialization { get; set; }
        public string? LicenseNumber { get; set; }
        public int? ExperienceYears { get; set; }
        public decimal? HourlyRate { get; set; }
    }
}

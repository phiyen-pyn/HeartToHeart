using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Models
{
    public class RegisterCounselorDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; set; }

        public string? Gender { get; set; }

        // Counselor-specific fields
        public string? LicenseNumber { get; set; }
        public string? Specializations { get; set; }
        public int? YearsOfExperience { get; set; }
        public string? Qualifications { get; set; }
        public string? Bio { get; set; }
    }
}

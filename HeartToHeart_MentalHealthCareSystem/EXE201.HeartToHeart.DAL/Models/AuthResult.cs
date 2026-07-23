using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Models
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public Guid? UserId { get; set; }
        public Guid? CounselorId { get; set; }
        public List<string>? UserRoles { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public Dictionary<string, object>? AdditionalData { get; set; }
    }
}

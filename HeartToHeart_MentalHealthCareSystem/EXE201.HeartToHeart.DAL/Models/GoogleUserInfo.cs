using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Models
{
    public class GoogleUserInfo
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Given_Name { get; set; } // Google uses snake_case
        public string Family_Name { get; set; } // Google uses snake_case
        public string Picture { get; set; }
        public bool Verified_Email { get; set; }

        // Helper properties to make it easier to work with
        public string FirstName
        {
            get
            {
                if (!string.IsNullOrEmpty(Given_Name))
                    return Given_Name;
                if (!string.IsNullOrEmpty(Name))
                {
                    var parts = Name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    return parts.Length > 0 ? parts[0] : "User";
                }
                return "User";
            }
        }

        public string LastName
        {
            get
            {
                if (!string.IsNullOrEmpty(Family_Name))
                    return Family_Name;
                if (!string.IsNullOrEmpty(Name))
                {
                    var parts = Name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    return parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : "";
                }
                return "";
            }
        }
    }
}

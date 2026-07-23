using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Models
{
    public class UpdateReportDto
    {
        [Required]
        public string Reason { get; set; } = string.Empty;
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}

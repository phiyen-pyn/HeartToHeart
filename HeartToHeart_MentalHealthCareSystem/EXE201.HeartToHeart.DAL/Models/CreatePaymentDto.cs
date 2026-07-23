using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Models
{
    public class CreatePaymentDto
    {
        [Required]
        public string PlanName { get; set; } = string.Empty;
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public int DurationDays { get; set; }
        [Required]
        public string PaymentMethod { get; set; } = string.Empty;
    }
}

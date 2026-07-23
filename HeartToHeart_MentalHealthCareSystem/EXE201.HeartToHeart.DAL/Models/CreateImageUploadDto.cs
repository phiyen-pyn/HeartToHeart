using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Models
{
    public class CreateImageUploadDto
    {
        [Required]
        public IFormFile File { get; set; } = null!;

        [MaxLength(100)]
        public string? EntityType { get; set; }

        public Guid? EntityId { get; set; }
    }
}

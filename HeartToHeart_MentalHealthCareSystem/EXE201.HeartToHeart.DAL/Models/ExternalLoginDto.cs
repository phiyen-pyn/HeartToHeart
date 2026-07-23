using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Models
{
    public class ExternalLoginDto
    {
        public string Provider { get; set; }
        public string ReturnUrl { get; set; }
    }
}

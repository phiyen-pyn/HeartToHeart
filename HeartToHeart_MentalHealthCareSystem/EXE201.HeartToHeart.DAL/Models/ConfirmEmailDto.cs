using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Models
{
    public class ConfirmEmailDto
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }
    }
}

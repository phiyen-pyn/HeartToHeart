using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Models
{
    public class DefaultUsersConfig
    {
        public DefaultUserInfo Admin { get; set; } = new();
        public DefaultUserInfo Counselor { get; set; } = new();
    }

}

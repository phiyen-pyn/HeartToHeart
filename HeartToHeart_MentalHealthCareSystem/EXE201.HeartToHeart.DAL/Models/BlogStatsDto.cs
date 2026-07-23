using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Models
{
    public class BlogStatsDto
    {
        public int TotalBlogs { get; set; }
        public int PublishedBlogs { get; set; }
        public int DraftBlogs { get; set; }
        public int FeaturedBlogs { get; set; }
        public int PremiumBlogs { get; set; }
        public int TotalViews { get; set; }
        public int TotalLikes { get; set; }
        public int TotalComments { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
}

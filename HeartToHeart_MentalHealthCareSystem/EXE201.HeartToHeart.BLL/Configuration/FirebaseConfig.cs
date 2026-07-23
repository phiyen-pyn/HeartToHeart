using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.BLL.Configuration
{
    public class FirebaseConfig
    {
        public string ProjectId { get; set; } = string.Empty;
        public string StorageBucket { get; set; } = string.Empty;
        public string ServiceAccountKeyPath { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public int MaxFileSizeInMB { get; set; } = 10;
        public List<string> AllowedFileTypes { get; set; } = new List<string>();
    }
}

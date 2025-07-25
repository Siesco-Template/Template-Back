using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Core.Entities
{
    public class LoginLog
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public bool IsSucceed { get; set; }
        //public string? IP { get; set; }

        public Guid AppUserId { get; set; }
        public AppUser? AppUser { get; set; }
    }
}

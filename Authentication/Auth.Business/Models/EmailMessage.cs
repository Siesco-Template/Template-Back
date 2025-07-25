using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Business.Models
{
    public class EmailMessage
    {
        public string Email { get; set; }

        public string Subject { get; set; }

        public string Content { get; set; }
    }
}

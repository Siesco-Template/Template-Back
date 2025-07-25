using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Business.Dtos
{
    public class ToggleBlockUserDto
    {
        public Guid UserId { get; set; }
        public string? BlockInformation { get; set; }
        public DateTime? LockDownDate { get; set; }
    }
}

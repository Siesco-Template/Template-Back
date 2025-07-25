using SharedLibrary.Dtos.PermissionDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Events
{
    public class UserRegisteredPermissionEvent
    {
        public Guid UserId { get; set; }
        public bool IsBlocked { get; set; }
        public string FullName { get; set; }
        public List<PageDto> Pages { get; set; }
    }
}

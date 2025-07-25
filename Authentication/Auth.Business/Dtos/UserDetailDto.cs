using SharedLibrary.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Business.Dtos
{
    public class UserDetailDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsBlock { get; set; }
        public DateTime? LockDownDate { get; set; }
        public string? BlockInformation { get; set; }
        public UserRole UserRole { get; set; }
    }
}

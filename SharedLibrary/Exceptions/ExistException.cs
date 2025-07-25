using Microsoft.AspNetCore.Http;
using SharedLibrary.Exceptions.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Exceptions
{
    public class ExistException : Exception, IBaseException
    {
        public int StatusCode => StatusCodes.Status400BadRequest;

        public string ErrorMessage { get; }

        public ExistException()
        {
            ErrorMessage = "Bu məlumat artıq mövcuddur";
        }

        public ExistException(string message)
        {
            ErrorMessage = message;
        }
    }
}

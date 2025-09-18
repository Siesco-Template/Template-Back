using Microsoft.AspNetCore.Http;
using SharedLibrary.Exceptions.Common;

namespace SharedLibrary.Exceptions
{
    public class NotFoundException : Exception, IBaseException
    {
        public int StatusCode => StatusCodes.Status404NotFound;

        public string ErrorMessage { get; }

        public NotFoundException()
        {
            ErrorMessage = "Mövcud deyil!";
        }

        public NotFoundException(string message)
        {
            ErrorMessage = message;
        }
    }
}
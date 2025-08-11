using Microsoft.AspNetCore.Http;
using Template.Exceptions.Common;

namespace Template.Exceptions
{
    public class NotFoundException : Exception, IBaseException
    {
        public int StatusCode => StatusCodes.Status404NotFound;

        public string ErrorMessage { get; }

        public NotFoundException() : base()
        {
            ErrorMessage = $"Mövcud deyil.";
        }

        public NotFoundException(string? message) : base(message)
        {
            ErrorMessage = message;
        }
    }
}
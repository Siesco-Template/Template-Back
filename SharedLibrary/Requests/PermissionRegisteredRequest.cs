using SharedLibrary.Dtos.PermissionDtos;

namespace SharedLibrary.Requests
{
    public class PermissionRegisteredRequest
    {
        public List<PageDto> Pages { get; set; } = [];
    }
}
namespace SharedLibrary.Dtos.PermissionDtos
{
    public class PageDto
    {
        public string Key { get; set; } = null!;
        public string Name { get; set; } = null!;
        public List<ActionDto> Actions { get; set; } = [];
    }
}
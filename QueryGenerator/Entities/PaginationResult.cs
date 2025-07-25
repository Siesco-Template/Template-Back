namespace QueryGenerator.Entities
{
    public class PaginationResult
    {
        public List<dynamic> Items { get; set; } = [];
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / Take);
        public int Page { get; set; }
        public int Take { get; set; }
    }
}
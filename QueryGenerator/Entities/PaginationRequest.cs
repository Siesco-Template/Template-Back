namespace QueryGenerator.Entities
{
    public class PaginationRequest
    {
        public int Page { get; set; } = 1;
        public int Take { get; set; }
        public bool IsInfiniteScroll { get; set; } = false;
    }
}
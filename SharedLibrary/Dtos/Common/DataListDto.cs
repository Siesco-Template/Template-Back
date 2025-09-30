namespace SharedLibrary.Dtos.Common
{
    public class DataListDto<T>
    {
        public List<T> Datas { get; set; }
        public int TotalCount { get; set; }
    }
}
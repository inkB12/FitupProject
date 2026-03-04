namespace FitupProject.BLL.Commons
{
    public class ApiResponse<T>
    {
        public int Status { get; set; }
        public string Msg { get; set; } = "";
        public T? Data { get; set; }
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public long TotalItems { get; set; }
    }
}

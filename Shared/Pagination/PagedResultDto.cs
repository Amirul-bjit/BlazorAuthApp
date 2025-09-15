namespace BlazorAuthApp.Shared.Pagination
{
    public class PagedResultDto<T>
    {
        public int TotalCount { get; set; }
        public IReadOnlyList<T> Items { get; set; } = new List<T>();
    }
}
namespace BlazorAuthApp.Shared.Pagination
{
    public class PagedRequestDto
    {
        public int SkipCount { get; set; } = 0;
        public int MaxResultCount { get; set; } = 10;
        public string? Filter { get; set; }
        public string? Sorting { get; set; }
    }
}
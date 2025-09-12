namespace BlazorAuthApp.Shared
{
    public class BlogFilterParameters
    {
        public List<int>? CategoryIds { get; set; }
        public BlogSortBy SortBy { get; set; } = BlogSortBy.Latest;
        public bool IncludeUnpublished { get; set; } = false;
    }
}

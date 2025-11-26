namespace FinanceTracker.BlazorUI.Models.Commons;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = [];

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages { get; set; } 
}

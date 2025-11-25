namespace FinanceTracker.Application.DTOs;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; } = [];

    public int Page { get; }

    public int PageSize { get; }

    public int TotalCount { get; }

    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    public PagedResult(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        Items = items ?? throw new ArgumentNullException(nameof(items));
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }
}

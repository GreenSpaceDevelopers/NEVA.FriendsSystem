namespace Application.Common.Models;

public class PagedList<T>
{
    public int TotalCount { get; set; }
    
    public List<T> Data { get; set; } = [];

    public PagedList<TResult> Map<TResult>(Func<T, TResult> selector)
    {
        return new PagedList<TResult>
        {
            TotalCount = TotalCount,
            Data = Data.Select(selector).ToList()
        };
    }
} 
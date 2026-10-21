using Microsoft.EntityFrameworkCore;

namespace App.Shared.Pagination;

public class PagedList<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }

    public static async Task<PagedList<T>> CreateAsync(
        IQueryable<T> source,
        int page,
        int pageSize,
        bool allPages = false
    )
    {
        var count = await source.CountAsync();

        List<T> items;
        if (allPages)
        {
            items = await source.ToListAsync();
            pageSize = count == 0 ? 1 : count;
            page = 1;
        }
        else
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            items = await source.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        return new PagedList<T>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = count,
            TotalPages = pageSize == 0 ? 0 : (int)Math.Ceiling(count / (double)pageSize)
        };
    }
}

namespace HRMS.Application.Common;

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; init; } = Enumerable.Empty<T>();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;

    public static PagedResult<T> Create(IEnumerable<T> items, int totalCount, int page, int pageSize)
        => new() { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
}

public class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
    public IEnumerable<string> Errors { get; init; } = Enumerable.Empty<string>();

    public static ApiResponse<T> Ok(T data, string? message = null)
        => new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> Fail(string error)
        => new() { Success = false, Errors = new[] { error } };

    public static ApiResponse<T> Fail(IEnumerable<string> errors)
        => new() { Success = false, Errors = errors };
}

public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse Ok(string? message = null)
        => new() { Success = true, Message = message };

    public new static ApiResponse Fail(string error)
        => new() { Success = false, Errors = new[] { error } };
}

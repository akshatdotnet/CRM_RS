namespace PersonalBrand.Shared.Models;

// ─── Generic API Response Wrapper ────────────────────
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = [];
    public int StatusCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> Ok(T data, string message = "Success") =>
        new() { Success = true, Data = data, Message = message, StatusCode = 200 };

    public static ApiResponse<T> Fail(string error, int statusCode = 400) =>
        new() { Success = false, Errors = [error], Message = error, StatusCode = statusCode };

    public static ApiResponse<T> Fail(List<string> errors, int statusCode = 400) =>
        new() { Success = false, Errors = errors, Message = "Validation failed", StatusCode = statusCode };
}

public class PagedResponse<T>
{
    public List<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

// ─── Shared DTOs used by both API and MVC ─────────────
public class LeadDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Service { get; set; } = string.Empty;
    public string Budget { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = "new";
    public string Value { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<string> Notes { get; set; } = [];
}

public class ContactFormDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Service { get; set; } = string.Empty;
    public string Budget { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class NewsletterDto
{
    public string Email { get; set; } = string.Empty;
}

public class AddNoteDto
{
    public string Note { get; set; } = string.Empty;
}

public class UpdateLeadStatusDto
{
    public string Status { get; set; } = string.Empty;
}

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using HRMS.Application.DTOs.Auth;
using HRMS.Application.DTOs.Employee;
using HRMS.Application.DTOs.Documents;
using HRMS.Application.DTOs.Salary;
using HRMS.Application.Common;

namespace HRMS.WebPortal.Services;

/// <summary>
/// Typed HTTP client that calls HRMS.Web API.
/// All methods accept a JWT token from the session.
/// </summary>
public class ApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<ApiClient> _logger;
    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public ApiClient(HttpClient http, ILogger<ApiClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    private void SetAuth(string? token)
    {
        _http.DefaultRequestHeaders.Authorization =
            string.IsNullOrEmpty(token) ? null : new AuthenticationHeaderValue("Bearer", token);
    }

    // ── Auth ─────────────────────────────────────────────────────────────────
    public async Task<AuthResponseDto?> LoginAsync(string email, string password)
    {
        var body = JsonSerializer.Serialize(new { email, password });
        var res = await _http.PostAsync("api/auth/login",
            new StringContent(body, Encoding.UTF8, "application/json"));
        if (!res.IsSuccessStatusCode) return null;
        var json = await res.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<AuthResponseDto>(json, _jsonOpts);
    }

    // ── Employees ─────────────────────────────────────────────────────────────
    public async Task<PagedResult<EmployeeListDto>?> GetEmployeesAsync(string token,
        int page = 1, int pageSize = 15, string? search = null, string? dept = null, string? status = null)
    {
        SetAuth(token);
        var url = $"api/employees?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(search)) url += $"&search={Uri.EscapeDataString(search)}";
        if (!string.IsNullOrEmpty(dept)) url += $"&department={dept}";
        if (!string.IsNullOrEmpty(status)) url += $"&status={status}";

        var res = await _http.GetAsync(url);
        if (!res.IsSuccessStatusCode) return null;
        var json = await res.Content.ReadAsStringAsync();
        var wrapper = JsonSerializer.Deserialize<ApiResponse<PagedResult<EmployeeListDto>>>(json, _jsonOpts);
        return wrapper?.Data;
    }

    public async Task<EmployeeDto?> GetEmployeeAsync(string token, Guid id)
    {
        SetAuth(token);
        var res = await _http.GetAsync($"api/employees/{id}");
        if (!res.IsSuccessStatusCode) return null;
        var json = await res.Content.ReadAsStringAsync();
        var wrapper = JsonSerializer.Deserialize<ApiResponse<EmployeeDto>>(json, _jsonOpts);
        return wrapper?.Data;
    }

    public async Task<(EmployeeDto? Result, string? Error)> CreateEmployeeAsync(string token, CreateEmployeeDto dto)
    {
        SetAuth(token);
        var body = JsonSerializer.Serialize(dto);
        var res = await _http.PostAsync("api/employees", new StringContent(body, Encoding.UTF8, "application/json"));
        var json = await res.Content.ReadAsStringAsync();
        if (res.IsSuccessStatusCode)
        {
            var wrapper = JsonSerializer.Deserialize<ApiResponse<EmployeeDto>>(json, _jsonOpts);
            return (wrapper?.Data, null);
        }
        var err = JsonSerializer.Deserialize<ApiResponse<object>>(json, _jsonOpts);
        return (null, err?.Errors?.FirstOrDefault() ?? "Failed to create employee.");
    }

    // ── Salary Slips ──────────────────────────────────────────────────────────
    public async Task<IEnumerable<SalarySlipDto>?> GetEmployeeSlipsAsync(string token, Guid employeeId)
    {
        SetAuth(token);
        var res = await _http.GetAsync($"api/salaryslips/employee/{employeeId}");
        if (!res.IsSuccessStatusCode) return null;
        var json = await res.Content.ReadAsStringAsync();
        var wrapper = JsonSerializer.Deserialize<ApiResponse<IEnumerable<SalarySlipDto>>>(json, _jsonOpts);
        return wrapper?.Data;
    }

    public async Task<(SalarySlipDto? Result, string? Error)> GenerateSalarySlipAsync(string token, GenerateSalarySlipDto dto)
    {
        SetAuth(token);
        var body = JsonSerializer.Serialize(dto);
        var res = await _http.PostAsync("api/salaryslips", new StringContent(body, Encoding.UTF8, "application/json"));
        var json = await res.Content.ReadAsStringAsync();
        if (res.IsSuccessStatusCode)
        {
            var wrapper = JsonSerializer.Deserialize<ApiResponse<SalarySlipDto>>(json, _jsonOpts);
            return (wrapper?.Data, null);
        }
        var err = JsonSerializer.Deserialize<ApiResponse<object>>(json, _jsonOpts);
        return (null, err?.Errors?.FirstOrDefault() ?? "Failed to generate slip.");
    }

    public async Task<byte[]?> GetSlipPdfAsync(string token, Guid slipId)
    {
        SetAuth(token);
        var res = await _http.GetAsync($"api/salaryslips/{slipId}/pdf");
        return res.IsSuccessStatusCode ? await res.Content.ReadAsByteArrayAsync() : null;
    }

    public async Task<bool> SendSlipAsync(string token, Guid slipId)
    {
        SetAuth(token);
        var res = await _http.PostAsync($"api/salaryslips/{slipId}/send", null);
        return res.IsSuccessStatusCode;
    }

    // ── Documents ─────────────────────────────────────────────────────────────
    public async Task<IEnumerable<DocumentDto>?> GetEmployeeDocumentsAsync(string token, Guid employeeId)
    {
        SetAuth(token);
        var res = await _http.GetAsync($"api/documents/employee/{employeeId}");
        if (!res.IsSuccessStatusCode) return null;
        var json = await res.Content.ReadAsStringAsync();
        var wrapper = JsonSerializer.Deserialize<ApiResponse<IEnumerable<DocumentDto>>>(json, _jsonOpts);
        return wrapper?.Data;
    }

    public async Task<(DocumentDto? Result, string? Error)> GenerateOfferLetterAsync(string token, Guid empId, OfferLetterRequestDto dto)
    {
        SetAuth(token);
        var body = JsonSerializer.Serialize(dto);
        var res = await _http.PostAsync($"api/documents/employee/{empId}/offer-letter",
            new StringContent(body, Encoding.UTF8, "application/json"));
        var json = await res.Content.ReadAsStringAsync();
        if (res.IsSuccessStatusCode)
        {
            var wrapper = JsonSerializer.Deserialize<ApiResponse<DocumentDto>>(json, _jsonOpts);
            return (wrapper?.Data, null);
        }
        var err = JsonSerializer.Deserialize<ApiResponse<object>>(json, _jsonOpts);
        return (null, err?.Errors?.FirstOrDefault() ?? "Failed.");
    }

    public async Task<(DocumentDto? Result, string? Error)> GenerateAppointmentLetterAsync(string token, Guid empId)
    {
        SetAuth(token);
        var res = await _http.PostAsync($"api/documents/employee/{empId}/appointment-letter", null);
        var json = await res.Content.ReadAsStringAsync();
        if (res.IsSuccessStatusCode)
        {
            var wrapper = JsonSerializer.Deserialize<ApiResponse<DocumentDto>>(json, _jsonOpts);
            return (wrapper?.Data, null);
        }
        var err = JsonSerializer.Deserialize<ApiResponse<object>>(json, _jsonOpts);
        return (null, err?.Errors?.FirstOrDefault() ?? "Failed.");
    }

    public async Task<(DocumentDto? Result, string? Error)> GenerateExperienceLetterAsync(string token, Guid empId)
    {
        SetAuth(token);
        var res = await _http.PostAsync($"api/documents/employee/{empId}/experience-letter", null);
        var json = await res.Content.ReadAsStringAsync();
        if (res.IsSuccessStatusCode)
        {
            var wrapper = JsonSerializer.Deserialize<ApiResponse<DocumentDto>>(json, _jsonOpts);
            return (wrapper?.Data, null);
        }
        var err = JsonSerializer.Deserialize<ApiResponse<object>>(json, _jsonOpts);
        return (null, err?.Errors?.FirstOrDefault() ?? "Failed.");
    }

    public async Task<byte[]?> GetDocumentPdfAsync(string token, Guid docId)
    {
        SetAuth(token);
        var res = await _http.GetAsync($"api/documents/{docId}/pdf");
        return res.IsSuccessStatusCode ? await res.Content.ReadAsByteArrayAsync() : null;
    }

    public async Task<bool> SendDocumentAsync(string token, Guid docId)
    {
        SetAuth(token);
        var res = await _http.PostAsync($"api/documents/{docId}/send", null);
        return res.IsSuccessStatusCode;
    }
}

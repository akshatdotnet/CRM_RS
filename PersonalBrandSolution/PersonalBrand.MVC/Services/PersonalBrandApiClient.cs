using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using PersonalBrand.Shared.DTOs;
using PersonalBrand.Shared.Models;

namespace PersonalBrand.MVC.Services;

// ─── Typed API Client ─────────────────────────────────
public class PersonalBrandApiClient
{
    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;
    private readonly ILogger<PersonalBrandApiClient> _logger;
    private static readonly JsonSerializerOptions _opts = new() { PropertyNameCaseInsensitive = true };

    public PersonalBrandApiClient(HttpClient http, IMemoryCache cache, ILogger<PersonalBrandApiClient> logger)
    {
        _http = http;
        _cache = cache;
        _logger = logger;
    }

    // ─── Generic GET helper with cache ───────────────────
    private async Task<T?> GetCachedAsync<T>(string url, string cacheKey, TimeSpan? duration = null)
    {
        if (_cache.TryGetValue(cacheKey, out T? cached)) return cached;
        try
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<T>>(url, _opts);
            if (response?.Success == true && response.Data != null)
            {
                _cache.Set(cacheKey, response.Data, duration ?? TimeSpan.FromMinutes(5));
                return response.Data;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API call failed: GET {Url}", url);
        }
        return default;
    }

    // ─── Generic POST helper ──────────────────────────────
    private async Task<ApiResponse<T>?> PostAsync<T>(string url, object payload)
    {
        try
        {
            var json = JsonSerializer.Serialize(payload, _opts);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var httpResp = await _http.PostAsync(url, content);
            return await httpResp.Content.ReadFromJsonAsync<ApiResponse<T>>(_opts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API call failed: POST {Url}", url);
            return null;
        }
    }

    // ─── Persona ──────────────────────────────────────────
    public Task<PersonaDto?> GetPersonaAsync() =>
        GetCachedAsync<PersonaDto>("api/v1/persona", "mvc_persona", TimeSpan.FromHours(1));

    // ─── Skills ───────────────────────────────────────────
    public Task<List<SkillDto>?> GetSkillsAsync() =>
        GetCachedAsync<List<SkillDto>>("api/v1/skills", "mvc_skills", TimeSpan.FromHours(1));

    // ─── Roadmap ──────────────────────────────────────────
    public Task<List<RoadmapItemDto>?> GetRoadmapAsync() =>
        GetCachedAsync<List<RoadmapItemDto>>("api/v1/roadmap", "mvc_roadmap", TimeSpan.FromHours(6));

    // ─── Courses ──────────────────────────────────────────
    public Task<List<CourseDto>?> GetCoursesAsync() =>
        GetCachedAsync<List<CourseDto>>("api/v1/courses", "mvc_courses", TimeSpan.FromHours(1));

    // ─── Projects ─────────────────────────────────────────
    public Task<List<ProjectDto>?> GetProjectsAsync(bool? featured = null)
    {
        var url = featured == true ? "api/v1/projects?featured=true" : "api/v1/projects";
        var key = featured == true ? "mvc_projects_featured" : "mvc_projects";
        return GetCachedAsync<List<ProjectDto>>(url, key, TimeSpan.FromHours(1));
    }

    // ─── Q&A ──────────────────────────────────────────────
    public Task<List<QAItemDto>?> GetQAAsync(string? level = null, string? search = null)
    {
        var url = "api/v1/qa";
        var key = "mvc_qa";
        if (!string.IsNullOrWhiteSpace(search)) { url += $"?search={Uri.EscapeDataString(search)}"; key = $"mvc_qa_s_{search}"; }
        else if (!string.IsNullOrWhiteSpace(level)) { url += $"?level={level}"; key = $"mvc_qa_{level}"; }
        return GetCachedAsync<List<QAItemDto>>(url, key, string.IsNullOrEmpty(search) ? TimeSpan.FromHours(1) : TimeSpan.FromMinutes(2));
    }

    // ─── Services ─────────────────────────────────────────
    public Task<List<ConsultingServiceDto>?> GetServicesAsync() =>
        GetCachedAsync<List<ConsultingServiceDto>>("api/v1/services", "mvc_services", TimeSpan.FromHours(1));

    // ─── Pipeline ─────────────────────────────────────────
    public Task<PipelineSummaryDto?> GetPipelineAsync() =>
        GetCachedAsync<PipelineSummaryDto>("api/v1/leads/pipeline", "mvc_pipeline", TimeSpan.FromSeconds(30));

    // ─── Blog ─────────────────────────────────────────────
    public Task<PagedResponse<BlogPostDto>?> GetBlogPostsAsync(int page = 1, int pageSize = 6) =>
        GetCachedAsync<PagedResponse<BlogPostDto>>($"api/v1/blog?page={page}&pageSize={pageSize}", $"mvc_blog_{page}", TimeSpan.FromMinutes(10));

    // ─── Testimonials ─────────────────────────────────────
    public Task<List<TestimonialDto>?> GetTestimonialsAsync() =>
        GetCachedAsync<List<TestimonialDto>>("api/v1/testimonials", "mvc_testimonials", TimeSpan.FromHours(1));

    // ─── Submit Contact Form (Lead) ───────────────────────
    public async Task<(bool success, string message, int leadId)> SubmitContactFormAsync(ContactFormDto dto)
    {
        try
        {
            var json = JsonSerializer.Serialize(dto, _opts);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var httpResp = await _http.PostAsync("api/v1/leads", content);
            var result = await httpResp.Content.ReadFromJsonAsync<ApiResponse<LeadResponseDto>>(_opts);
            if (result?.Success == true)
            {
                _cache.Remove("mvc_pipeline"); // invalidate pipeline cache
                return (true, "Your inquiry has been received! I'll respond within 24 hours.", result.Data?.Id ?? 0);
            }
            return (false, result?.Message ?? "Submission failed. Please try again.", 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Contact form submission failed");
            return (false, "Service temporarily unavailable. Please try again later.", 0);
        }
    }

    // ─── Add Note to Lead ─────────────────────────────────
    public async Task<bool> AddLeadNoteAsync(int leadId, string note)
    {
        var result = await PostAsync<string>($"api/v1/leads/{leadId}/notes", new { note });
        return result?.Success == true;
    }

    // ─── Update Lead Status ───────────────────────────────
    public async Task<bool> UpdateLeadStatusAsync(int leadId, string status)
    {
        try
        {
            var json = JsonSerializer.Serialize(new { status }, _opts);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var httpResp = await _http.PatchAsync($"api/v1/leads/{leadId}/status", content);
            var result = await httpResp.Content.ReadFromJsonAsync<ApiResponse<string>>(_opts);
            if (result?.Success == true) _cache.Remove("mvc_pipeline");
            return result?.Success == true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lead status update failed for lead {LeadId}", leadId);
            return false;
        }
    }

    // ─── Newsletter Subscribe ─────────────────────────────
    public async Task<(bool success, string message)> SubscribeNewsletterAsync(string email)
    {
        try
        {
            var json = JsonSerializer.Serialize(new { email }, _opts);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var httpResp = await _http.PostAsync("api/v1/newsletter/subscribe", content);
            var result = await httpResp.Content.ReadFromJsonAsync<ApiResponse<string>>(_opts);
            return (result?.Success == true, result?.Message ?? "Subscription failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Newsletter subscription failed");
            return (false, "Service temporarily unavailable");
        }
    }
}

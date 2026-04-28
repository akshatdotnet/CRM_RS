using PersonalBrand.Shared.DTOs;
using PersonalBrand.Shared.Models;

namespace PersonalBrand.API.Services.Interfaces;

public interface IPersonaService
{
    Task<PersonaDto?> GetPersonaAsync();
}

public interface ISkillService
{
    Task<IEnumerable<SkillDto>> GetAllAsync();
}

public interface IRoadmapService
{
    Task<IEnumerable<RoadmapItemDto>> GetAllAsync();
}

public interface ICourseService
{
    Task<IEnumerable<CourseDto>> GetAllAsync();
    Task<CourseDto?> GetByIdAsync(int id);
}

public interface IProjectService
{
    Task<IEnumerable<ProjectDto>> GetAllAsync();
    Task<IEnumerable<ProjectDto>> GetFeaturedAsync();
    Task<ProjectDto?> GetByIdAsync(int id);
}

public interface IQAService
{
    Task<IEnumerable<QAItemDto>> GetAllAsync();
    Task<IEnumerable<QAItemDto>> GetByLevelAsync(string level);
    Task<IEnumerable<QAItemDto>> SearchAsync(string query);
}

public interface IConsultingService
{
    Task<IEnumerable<ConsultingServiceDto>> GetAllAsync();
}

public interface ILeadService
{
    Task<PipelineSummaryDto> GetPipelineSummaryAsync();
    Task<IEnumerable<LeadResponseDto>> GetAllAsync();
    Task<LeadResponseDto?> GetByIdAsync(int id);
    Task<LeadResponseDto> CreateLeadAsync(ContactFormDto dto);
    Task AddNoteAsync(int leadId, string note);
    Task UpdateStatusAsync(int leadId, string newStatus);
}

public interface IBlogService
{
    Task<IEnumerable<BlogPostDto>> GetPublishedAsync();
    Task<BlogPostDto?> GetBySlugAsync(string slug);
    Task<PagedResponse<BlogPostDto>> GetPagedAsync(int page, int pageSize);
}

public interface ITestimonialService
{
    Task<IEnumerable<TestimonialDto>> GetAllAsync();
}

public interface INewsletterService
{
    Task<bool> SubscribeAsync(string email);
}

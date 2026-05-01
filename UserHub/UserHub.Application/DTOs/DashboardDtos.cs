namespace UserHub.Application.DTOs;

public class DashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalRoles { get; set; }
    public int TotalModules { get; set; }
    public IEnumerable<RecentActivityDto> RecentActivity { get; set; } = Enumerable.Empty<RecentActivityDto>();
}

public class RecentActivityDto
{
    public string Username { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
}

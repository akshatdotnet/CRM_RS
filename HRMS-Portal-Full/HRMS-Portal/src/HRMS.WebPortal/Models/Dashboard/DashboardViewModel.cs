namespace HRMS.WebPortal.Models.Dashboard;

public class DashboardViewModel
{
    public string UserName  { get; set; } = string.Empty;
    public string UserRole  { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public int TotalEmployees  { get; set; }
    public int ActiveEmployees { get; set; }
    public List<EmployeeSummary> RecentEmployees { get; set; } = new();
}

public class EmployeeSummary
{
    public Guid   Id           { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName     { get; set; } = string.Empty;
    public string Department   { get; set; } = string.Empty;
    public string Designation  { get; set; } = string.Empty;
    public string Status       { get; set; } = string.Empty;
    public DateTime JoiningDate { get; set; }
}

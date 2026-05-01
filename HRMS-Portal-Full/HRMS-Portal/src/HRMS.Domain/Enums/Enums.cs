namespace HRMS.Domain.Enums;

public enum UserRole
{
    Admin = 1,
    HR = 2,
    Employee = 3
}

public enum EmploymentStatus
{
    Active = 1,
    OnNotice = 2,
    Resigned = 3,
    Terminated = 4,
    OnLeave = 5
}

public enum Gender
{
    Male = 1,
    Female = 2,
    Other = 3,
    PreferNotToSay = 4
}

public enum DocumentType
{
    OfferLetter = 1,
    AppointmentLetter = 2,
    ExperienceLetter = 3,
    SalarySlip = 4,
    Form16 = 5,
    IncrementLetter = 6,
    RelievingLetter = 7
}

public enum DocumentStatus
{
    Draft = 1,
    Generated = 2,
    Sent = 3,
    Acknowledged = 4
}

public enum EmailStatus
{
    Pending = 1,
    Sent = 2,
    Failed = 3
}

public enum Department
{
    Engineering = 1,
    HumanResources = 2,
    Finance = 3,
    Marketing = 4,
    Sales = 5,
    Operations = 6,
    Legal = 7,
    ProductManagement = 8,
    CustomerSupport = 9,
    Administration = 10
}

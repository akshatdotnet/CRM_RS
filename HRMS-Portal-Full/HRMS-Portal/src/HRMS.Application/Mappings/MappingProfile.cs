using AutoMapper;
using HRMS.Application.DTOs.Documents;
using HRMS.Application.DTOs.Employee;
using HRMS.Application.DTOs.Salary;
using HRMS.Domain.Entities;

namespace HRMS.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Employee
        CreateMap<Employee, EmployeeDto>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FullName))
            .ForMember(d => d.Department, o => o.MapFrom(s => s.Department.ToString()))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.Gender, o => o.MapFrom(s => s.Gender.ToString()));

        CreateMap<Employee, EmployeeListDto>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FullName))
            .ForMember(d => d.Department, o => o.MapFrom(s => s.Department.ToString()))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.GrossMonthly, o => o.MapFrom(s =>
                s.SalaryStructure != null ? (decimal?)s.SalaryStructure.GrossMonthly : null));

        CreateMap<SalaryStructure, SalaryStructureDto>();

        // Documents
        CreateMap<Document, DocumentDto>()
            .ForMember(d => d.EmployeeName, o => o.MapFrom(s => s.Employee != null ? s.Employee.FullName : string.Empty))
            .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.EmailStatus, o => o.MapFrom(s => s.EmailStatus.ToString()));

        // Salary Slip
        CreateMap<SalarySlip, SalarySlipDto>()
            .ForMember(d => d.EmployeeCode, o => o.MapFrom(s => s.Employee != null ? s.Employee.EmployeeCode : string.Empty))
            .ForMember(d => d.EmployeeName, o => o.MapFrom(s => s.Employee != null ? s.Employee.FullName : string.Empty))
            .ForMember(d => d.Designation, o => o.MapFrom(s => s.Employee != null ? s.Employee.Designation : string.Empty))
            .ForMember(d => d.Department, o => o.MapFrom(s => s.Employee != null ? s.Employee.Department.ToString() : string.Empty))
            .ForMember(d => d.PanNumber, o => o.MapFrom(s => s.Employee != null ? s.Employee.PanNumber : null))
            .ForMember(d => d.MonthYear, o => o.MapFrom(s => s.MonthYear));
    }
}

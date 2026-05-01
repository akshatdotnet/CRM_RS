using HRMS.Application.DTOs.Salary;
using System.Text;

namespace HRMS.Application.Services;

/// <summary>
/// Renders salary slip as HTML using StringBuilder — avoids raw string literal
/// conflicts between CSS curly braces and C# interpolation holes.
/// </summary>
public static class SalarySlipTemplate
{
    public static string Render(SalarySlipDto s)
    {
        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html><html><head><meta charset='utf-8'/>");
        sb.Append("<style>");
        sb.Append("body{font-family:Arial,sans-serif;font-size:12px;color:#222;margin:0;padding:20px;}");
        sb.Append(".header{text-align:center;border-bottom:2px solid #003366;padding-bottom:10px;margin-bottom:16px;}");
        sb.Append(".company-name{font-size:20px;font-weight:bold;color:#003366;}");
        sb.Append(".slip-title{font-size:14px;color:#555;margin-top:4px;}");
        sb.Append("table{width:100%;border-collapse:collapse;margin-bottom:12px;}");
        sb.Append("th{background:#003366;color:#fff;padding:6px 10px;text-align:left;}");
        sb.Append("td{padding:5px 10px;border-bottom:1px solid #eee;}");
        sb.Append(".section-label{font-weight:bold;color:#003366;margin:10px 0 4px;}");
        sb.Append(".net-pay{font-size:16px;font-weight:bold;color:#003366;text-align:right;padding:10px;border-top:2px solid #003366;}");
        sb.Append(".footer{font-size:10px;color:#888;text-align:center;margin-top:20px;border-top:1px solid #ddd;padding-top:8px;}");
        sb.Append(".two-col{display:grid;grid-template-columns:1fr 1fr;gap:20px;}");
        sb.Append("</style></head><body>");

        // Header
        sb.Append("<div class='header'>");
        sb.Append("<div class='company-name'>ACME CORPORATION PVT. LTD.</div>");
        sb.AppendFormat("<div class='slip-title'>Salary Slip for {0}</div>", s.MonthYear);
        sb.Append("</div>");

        // Employee info table
        sb.Append("<table>");
        sb.AppendFormat("<tr><td><b>Employee Name</b></td><td>{0}</td><td><b>Employee Code</b></td><td>{1}</td></tr>", s.EmployeeName, s.EmployeeCode);
        sb.AppendFormat("<tr><td><b>Designation</b></td><td>{0}</td><td><b>Department</b></td><td>{1}</td></tr>", s.Designation, s.Department);
        sb.AppendFormat("<tr><td><b>PAN Number</b></td><td>{0}</td><td><b>Days Paid</b></td><td>{1} / {2}</td></tr>", s.PanNumber ?? "N/A", s.PaidDays, s.TotalDays);
        sb.Append("</table>");

        // Two-column layout
        sb.Append("<div class='two-col'>");

        // Earnings
        sb.Append("<div><div class='section-label'>EARNINGS</div><table>");
        sb.Append("<thead><tr><th>Component</th><th>Amount (Rs.)</th></tr></thead><tbody>");
        sb.AppendFormat("<tr><td>Basic Salary</td><td>{0:N2}</td></tr>", s.Basic);
        sb.AppendFormat("<tr><td>House Rent Allowance (HRA)</td><td>{0:N2}</td></tr>", s.HRA);
        sb.AppendFormat("<tr><td>Special Allowance</td><td>{0:N2}</td></tr>", s.SpecialAllowance);
        if (s.OtherAllowances > 0)
            sb.AppendFormat("<tr><td>Other Allowances</td><td>{0:N2}</td></tr>", s.OtherAllowances);
        sb.AppendFormat("<tr><td><b>Gross Earnings</b></td><td><b>{0:N2}</b></td></tr>", s.GrossEarnings);
        sb.Append("</tbody></table></div>");

        // Deductions
        sb.Append("<div><div class='section-label'>DEDUCTIONS</div><table>");
        sb.Append("<thead><tr><th>Component</th><th>Amount (Rs.)</th></tr></thead><tbody>");
        sb.AppendFormat("<tr><td>Provident Fund (Employee)</td><td>{0:N2}</td></tr>", s.EmployeePF);
        sb.AppendFormat("<tr><td>Professional Tax</td><td>{0:N2}</td></tr>", s.ProfessionalTax);
        sb.AppendFormat("<tr><td>Income Tax (TDS)</td><td>{0:N2}</td></tr>", s.IncomeTax);
        if (s.OtherDeductions > 0)
            sb.AppendFormat("<tr><td>Other Deductions</td><td>{0:N2}</td></tr>", s.OtherDeductions);
        sb.AppendFormat("<tr><td><b>Total Deductions</b></td><td><b>{0:N2}</b></td></tr>", s.TotalDeductions);
        sb.Append("</tbody></table></div>");

        sb.Append("</div>"); // end two-col

        // Net Pay
        sb.AppendFormat("<div class='net-pay'>Net Pay: Rs. {0:N2}</div>", s.NetPay);

        // Footer
        sb.AppendFormat(
            "<div class='footer'>System-generated salary slip. No signature required. | Generated: {0:dd-MMM-yyyy HH:mm} UTC</div>",
            DateTime.UtcNow);

        sb.Append("</body></html>");
        return sb.ToString();
    }
}

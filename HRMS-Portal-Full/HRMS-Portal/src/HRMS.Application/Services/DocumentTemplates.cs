using HRMS.Application.DTOs.Documents;
using HRMS.Domain.Entities;
using System.Text;

namespace HRMS.Application.Services;

/// <summary>
/// HR document HTML templates built with StringBuilder — avoids raw string literal
/// conflicts between CSS curly braces and C# string interpolation.
/// </summary>
public static class DocumentTemplates
{
    // ── Shared letterhead styles ─────────────────────────────────────────────
    private static void AppendLetterheadStyles(StringBuilder sb)
    {
        sb.Append("<style>");
        sb.Append("body{font-family:Arial,sans-serif;font-size:13px;color:#222;margin:0;padding:40px;}");
        sb.Append(".lh{border-bottom:3px solid #003366;padding-bottom:12px;margin-bottom:24px;}");
        sb.Append(".company{font-size:22px;font-weight:bold;color:#003366;}");
        sb.Append(".addr{font-size:11px;color:#555;margin-top:2px;}");
        sb.Append("h2{color:#003366;font-size:15px;margin:20px 0 8px;}");
        sb.Append("p{line-height:1.7;margin:8px 0;}");
        sb.Append(".sign{margin-top:60px;}");
        sb.Append(".footer{border-top:1px solid #ccc;margin-top:40px;padding-top:8px;font-size:10px;color:#888;text-align:center;}");
        sb.Append("table{width:100%;border-collapse:collapse;margin:10px 0;}");
        sb.Append("th{background:#003366;color:#fff;padding:6px 10px;text-align:left;}");
        sb.Append("td{padding:5px 10px;border-bottom:1px solid #eee;}");
        sb.Append(".dashed{border-top:1px dashed #ccc;padding-top:10px;margin-top:30px;}");
        sb.Append("</style>");
    }

    private static void AppendLetterhead(StringBuilder sb)
    {
        sb.Append("<div class='lh'>");
        sb.Append("<div class='company'>ACME CORPORATION PVT. LTD.</div>");
        sb.Append("<div class='addr'>123 Business Park, Andheri East, Mumbai - 400069 | CIN: U74999MH2010PTC123456 | hr@acmecorp.in</div>");
        sb.Append("</div>");
    }

    // ── Offer Letter ─────────────────────────────────────────────────────────
    public static string OfferLetter(Employee e, OfferLetterRequestDto r)
    {
        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html><html><head><meta charset='utf-8'/>");
        AppendLetterheadStyles(sb);
        sb.Append("</head><body>");
        AppendLetterhead(sb);

        sb.AppendFormat("<p>Date: {0:dd MMMM yyyy}</p>", DateTime.UtcNow);
        sb.AppendFormat("<p><b>{0}</b><br/>{1}</p>", e.FullName, e.Address ?? "—");
        sb.Append("<h2>LETTER OF OFFER</h2>");
        sb.AppendFormat("<p>Dear {0},</p>", e.FirstName);
        sb.AppendFormat(
            "<p>We are pleased to offer you the position of <b>{0}</b> at Acme Corporation Pvt. Ltd." +
            "{1}. This offer is contingent upon successful completion of background verification.</p>",
            r.DesignationOffered,
            r.WorkLocation != null ? ", based at <b>" + r.WorkLocation + "</b>" : "");

        sb.Append("<h2>Compensation Summary</h2>");
        sb.Append("<table><thead><tr><th>Component</th><th>Annual (Rs.)</th></tr></thead><tbody>");
        sb.AppendFormat("<tr><td>Cost to Company (CTC)</td><td>{0:N2}</td></tr>", r.OfferedCTC);
        sb.Append("</tbody></table>");
        sb.Append("<p>A detailed salary breakup will be provided in your Appointment Letter.</p>");

        sb.AppendFormat(
            "<p>Please confirm your acceptance by <b>{0:dd MMMM yyyy}</b> by signing and returning this letter.</p>",
            r.JoiningDeadline);

        if (!string.IsNullOrEmpty(r.SpecialConditions))
            sb.AppendFormat("<p><b>Special Conditions:</b> {0}</p>", r.SpecialConditions);
        if (!string.IsNullOrEmpty(r.ReportingManager))
            sb.AppendFormat("<p>You will be reporting to <b>{0}</b>.</p>", r.ReportingManager);

        sb.Append("<p>We look forward to welcoming you to our team.</p>");
        sb.Append("<div class='sign'><p>Sincerely,</p><p><b>HR Department</b><br/>Acme Corporation Pvt. Ltd.</p></div>");
        sb.AppendFormat(
            "<div class='dashed'>I, <b>{0}</b>, accept the above offer.<br/>Signature: _________________________ &nbsp;&nbsp; Date: _____________</div>",
            e.FullName);
        sb.Append("<div class='footer'>Confidential | Acme Corporation Pvt. Ltd.</div>");
        sb.Append("</body></html>");
        return sb.ToString();
    }

    // ── Appointment Letter ───────────────────────────────────────────────────
    public static string AppointmentLetter(Employee e)
    {
        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html><html><head><meta charset='utf-8'/>");
        AppendLetterheadStyles(sb);
        sb.Append("</head><body>");
        AppendLetterhead(sb);

        sb.AppendFormat("<p>Date: {0:dd MMMM yyyy}</p>", e.JoiningDate);
        sb.AppendFormat("<p>Ref: APPT/{0}/{1}</p>", e.EmployeeCode, e.JoiningDate.Year);
        sb.AppendFormat("<p><b>{0}</b></p>", e.FullName);
        sb.Append("<h2>APPOINTMENT LETTER</h2>");
        sb.AppendFormat("<p>Dear {0},</p>", e.FirstName);
        sb.AppendFormat(
            "<p>We are pleased to appoint you as <b>{0}</b> in the <b>{1}</b> department, effective <b>{2:dd MMMM yyyy}</b>.</p>",
            e.Designation, e.Department, e.JoiningDate);

        sb.Append("<h2>Terms of Appointment</h2>");
        sb.Append("<table><tbody>");
        sb.AppendFormat("<tr><td><b>Employee Code</b></td><td>{0}</td></tr>", e.EmployeeCode);
        sb.AppendFormat("<tr><td><b>Designation</b></td><td>{0}</td></tr>", e.Designation);
        sb.AppendFormat("<tr><td><b>Department</b></td><td>{0}</td></tr>", e.Department);
        sb.AppendFormat("<tr><td><b>Date of Joining</b></td><td>{0:dd MMMM yyyy}</td></tr>", e.JoiningDate);
        sb.AppendFormat("<tr><td><b>Gross Monthly CTC</b></td><td>Rs. {0:N2}</td></tr>", e.SalaryStructure?.GrossMonthly);
        sb.AppendFormat("<tr><td><b>Annual CTC</b></td><td>Rs. {0:N2}</td></tr>", e.SalaryStructure?.AnnualCTC);
        sb.Append("</tbody></table>");

        sb.Append("<h2>Monthly Salary Breakup</h2>");
        sb.Append("<table><thead><tr><th>Component</th><th>Amount (Rs.)</th></tr></thead><tbody>");
        sb.AppendFormat("<tr><td>Basic Salary</td><td>{0:N2}</td></tr>", e.SalaryStructure?.Basic);
        sb.AppendFormat("<tr><td>House Rent Allowance (HRA)</td><td>{0:N2}</td></tr>", e.SalaryStructure?.HRA);
        sb.AppendFormat("<tr><td>Special Allowance</td><td>{0:N2}</td></tr>", e.SalaryStructure?.SpecialAllowance);
        sb.AppendFormat("<tr><td><b>Gross Earnings</b></td><td><b>{0:N2}</b></td></tr>", e.SalaryStructure?.GrossMonthly);
        sb.AppendFormat("<tr><td>Employee PF Deduction</td><td>{0:N2}</td></tr>", e.SalaryStructure?.EmployeePF);
        sb.AppendFormat("<tr><td>Professional Tax</td><td>{0:N2}</td></tr>", e.SalaryStructure?.ProfessionalTax);
        sb.AppendFormat("<tr><td><b>Net Take-Home (Approx.)</b></td><td><b>{0:N2}</b></td></tr>", e.SalaryStructure?.NetMonthly);
        sb.Append("</tbody></table>");

        sb.Append("<p>This appointment is subject to a probation period of <b>6 months</b>. " +
                  "Notice period post-confirmation: <b>60 days</b>.</p>");
        sb.Append("<div class='sign'><p>For Acme Corporation Pvt. Ltd.,</p><p><b>HR Department</b></p></div>");
        sb.AppendFormat(
            "<div class='dashed'>I, <b>{0}</b>, acknowledge receipt of this appointment letter.<br/>Signature: _________________________ &nbsp;&nbsp; Date: _____________</div>",
            e.FullName);
        sb.Append("<div class='footer'>Confidential | Acme Corporation Pvt. Ltd.</div>");
        sb.Append("</body></html>");
        return sb.ToString();
    }

    // ── Experience Letter ────────────────────────────────────────────────────
    public static string ExperienceLetter(Employee e)
    {
        var lwd = e.LastWorkingDate ?? DateTime.UtcNow;
        var months = e.GetTenureInMonths();
        var years = months / 12;
        var remMonths = months % 12;
        var tenure = years > 0
            ? string.Format("{0} year{1} and {2} month{3}", years, years > 1 ? "s" : "", remMonths, remMonths != 1 ? "s" : "")
            : string.Format("{0} month{1}", remMonths, remMonths != 1 ? "s" : "");
        var pronoun = e.Gender == Domain.Enums.Gender.Female ? "She" : "He";

        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html><html><head><meta charset='utf-8'/>");
        AppendLetterheadStyles(sb);
        sb.Append("</head><body>");
        AppendLetterhead(sb);

        sb.AppendFormat("<p>Date: {0:dd MMMM yyyy}</p>", lwd);
        sb.AppendFormat("<p>Ref: EXP/{0}/{1}</p>", e.EmployeeCode, lwd.Year);
        sb.Append("<h2>EXPERIENCE LETTER</h2>");
        sb.Append("<p>To Whom It May Concern,</p>");
        sb.AppendFormat(
            "<p>This is to certify that <b>{0}</b> (Employee Code: <b>{1}</b>) was employed with " +
            "Acme Corporation Pvt. Ltd. as <b>{2}</b> in the <b>{3}</b> department from " +
            "<b>{4:dd MMMM yyyy}</b> to <b>{5:dd MMMM yyyy}</b>, a tenure of <b>{6}</b>.</p>",
            e.FullName, e.EmployeeCode, e.Designation, e.Department, e.JoiningDate, lwd, tenure);
        sb.AppendFormat(
            "<p>During this period, {0} demonstrated professionalism and dedication to the organization. " +
            "{1} was a valued member of the team and contributed positively to the department.</p>",
            e.FirstName, pronoun);
        sb.AppendFormat("<p>We wish {0} the very best in future endeavors.</p>", e.FirstName);

        sb.Append("<div class='sign'><p>For Acme Corporation Pvt. Ltd.,</p>");
        sb.Append("<br/><p>________________________<br/><b>Authorized Signatory</b><br/>HR Department</p></div>");
        sb.Append("<div class='footer'>Confidential | Acme Corporation Pvt. Ltd.</div>");
        sb.Append("</body></html>");
        return sb.ToString();
    }

    // ── Form 16 ──────────────────────────────────────────────────────────────
    public static string Form16(Employee e, List<SalarySlip> slips, int financialYear)
    {
        var totalGross = slips.Sum(s => s.GrossEarnings);
        var totalPF    = slips.Sum(s => s.EmployeePF);
        var totalPT    = slips.Sum(s => s.ProfessionalTax);
        var totalTDS   = slips.Sum(s => s.IncomeTax);
        var pf80C      = Math.Min(totalPF, 150000m);

        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html><html><head><meta charset='utf-8'/>");
        AppendLetterheadStyles(sb);
        sb.Append("</head><body>");
        AppendLetterhead(sb);

        sb.AppendFormat("<h2 style='text-align:center'>FORM 16 - Part A &amp; Part B</h2>");
        sb.AppendFormat(
            "<p style='text-align:center'>Assessment Year: {0}-{1} | Financial Year: {2}-{3}</p>",
            financialYear + 1, financialYear + 2, financialYear, financialYear + 1);

        sb.Append("<p><b>COMPLIANCE NOTE:</b> This is a system-generated Form 16 summary. " +
                  "The official Form 16 must be digitally signed per Income Tax Act, 1961. " +
                  "Consult a tax professional before filing ITR.</p>");

        // Part A
        sb.Append("<h2>PART A — TDS Summary</h2><table><tbody>");
        sb.Append("<tr><td><b>Name of Employer</b></td><td>Acme Corporation Pvt. Ltd.</td></tr>");
        sb.Append("<tr><td><b>TAN of Employer</b></td><td>MUMB12345A</td></tr>");
        sb.Append("<tr><td><b>PAN of Employer</b></td><td>AAACA1234A</td></tr>");
        sb.AppendFormat("<tr><td><b>Name of Employee</b></td><td>{0}</td></tr>", e.FullName);
        sb.AppendFormat("<tr><td><b>PAN of Employee</b></td><td>{0}</td></tr>", e.PanNumber ?? "NOT PROVIDED");
        sb.AppendFormat(
            "<tr><td><b>Period of Employment</b></td><td>{0:dd-MMM-yyyy} to {1}</td></tr>",
            e.JoiningDate,
            e.LastWorkingDate.HasValue ? e.LastWorkingDate.Value.ToString("dd-MMM-yyyy") : "Continuing");
        sb.AppendFormat("<tr><td><b>Total Tax Deducted (TDS)</b></td><td>Rs. {0:N2}</td></tr>", totalTDS);
        sb.Append("</tbody></table>");

        // Part B
        sb.Append("<h2>PART B — Salary Details</h2>");
        sb.Append("<table><thead><tr><th>Particulars</th><th>Amount (Rs.)</th></tr></thead><tbody>");
        sb.AppendFormat("<tr><td>Gross Salary [Sec 17(1)]</td><td>{0:N2}</td></tr>", totalGross);
        sb.AppendFormat("<tr><td>Professional Tax Paid</td><td>{0:N2}</td></tr>", totalPT);
        sb.AppendFormat("<tr><td>Income Chargeable under Salaries</td><td>{0:N2}</td></tr>", totalGross - totalPT);
        sb.AppendFormat("<tr><td>Standard Deduction [Sec 16(ia)]</td><td>50,000.00</td></tr>");
        sb.AppendFormat("<tr><td>Net Taxable Salary</td><td>{0:N2}</td></tr>", Math.Max(0, totalGross - totalPT - 50000));
        sb.Append("</tbody></table>");

        // Chapter VI-A
        sb.Append("<h2>Chapter VI-A Deductions</h2>");
        sb.Append("<table><thead><tr><th>Section</th><th>Particulars</th><th>Amount (Rs.)</th></tr></thead><tbody>");
        sb.AppendFormat("<tr><td>80C</td><td>Provident Fund Contribution</td><td>{0:N2}</td></tr>", pf80C);
        sb.AppendFormat("<tr><td colspan='2'><b>Total Deductions</b></td><td><b>{0:N2}</b></td></tr>", pf80C);
        sb.Append("</tbody></table>");

        sb.Append("<table style='margin-top:16px'><tbody>");
        sb.AppendFormat("<tr><td><b>Total TDS Deducted</b></td><td><b>Rs. {0:N2}</b></td></tr>", totalTDS);
        sb.Append("</tbody></table>");

        sb.Append("<div class='sign'><p>Certified that the above information is true and correct.</p>");
        sb.Append("<p>________________________<br/><b>Authorized Signatory</b><br/>Acme Corporation Pvt. Ltd.</p></div>");
        sb.AppendFormat(
            "<div class='footer'>Generated on {0:dd-MMM-yyyy} | Reference only. Official Form 16 issued separately.</div>",
            DateTime.UtcNow);
        sb.Append("</body></html>");
        return sb.ToString();
    }
}
